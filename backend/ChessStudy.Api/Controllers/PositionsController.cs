using ChessStudy.Api.Data;
using ChessStudy.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChessStudy.Api.DTOs;
using System.Linq.Expressions;

namespace ChessStudy.Api.Controllers;

[ApiController]
[Route("api/positions")]
public class PositionsController : ControllerBase
{
    private static readonly Expression<Func<Position, PositionNode>> NodeSelector =
    p => new PositionNode
    {
        PositionId = p.PositionId,
        ParentPositionId = p.ParentPositionId,
        Fen = p.Fen,
        MoveUci = p.MoveUci,
        MoveSan = p.MoveSan,
        Ply = p.Ply,
        SiblingOrder = p.SiblingOrder
    };

    private readonly AppDbContext _db;

    public PositionsController(AppDbContext db)
    {
        _db = db;
    }

    public record CreateRootPositionRequest(string Fen);
    public record UpdatePositionRequest(string? Fen, string? MoveUci, string? MoveSan);


    // POST /api/positions/root?fileId=1
    [HttpPost("root")]
    public async Task<IActionResult> CreateRoot([FromQuery] int fileId, [FromBody] CreateRootPositionRequest req)
    {
        var fileExists = await _db.ChessFiles.AnyAsync(f => f.ChessFileId == fileId);
        if (!fileExists) return NotFound("File not found.");

        // Prevent duplicate roots
        var existingRoot = await _db.Positions.AnyAsync(p => p.ChessFileId == fileId && p.ParentPositionId == null);
        if (existingRoot) return BadRequest("Root position already exists for this file.");

        var root = new Position
        {
            ChessFileId = fileId,
            ParentPositionId = null,
            Fen = req.Fen,
            Ply = 0,
            SiblingOrder = 0
        };

        _db.Positions.Add(root);
        await _db.SaveChangesAsync();

        return Ok(new { root.PositionId, root.ChessFileId, root.Fen, root.Ply });
    }

    public record CreateChildPositionRequest(int ParentPositionId, string Fen, string? MoveUci, string? MoveSan, int? SiblingOrder);

    // POST /api/positions/child
    [HttpPost("child")]
    public async Task<IActionResult> CreateChild([FromBody] CreateChildPositionRequest req)
    {
        var parent = await _db.Positions.FirstOrDefaultAsync(p => p.PositionId == req.ParentPositionId);
        if (parent == null) return NotFound("Parent position not found.");

        var order = req.SiblingOrder ?? await _db.Positions
            .Where(p => p.ParentPositionId == req.ParentPositionId)
            .Select(p => (int?)p.SiblingOrder)
            .MaxAsync() ?? -1;

        if (req.SiblingOrder == null) order += 1;

        var child = new Position
        {
            ChessFileId = parent.ChessFileId,
            ParentPositionId = parent.PositionId,
            Fen = req.Fen,
            MoveUci = req.MoveUci,
            MoveSan = req.MoveSan,
            Ply = parent.Ply + 1,
            SiblingOrder = order
        };

        _db.Positions.Add(child);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            child.PositionId,
            child.ParentPositionId,
            child.ChessFileId,
            child.MoveUci,
            child.MoveSan,
            child.Ply,
            child.SiblingOrder
        });

    }

    // GET /api/positions?fileId=2
    [HttpGet]
    public async Task<IActionResult> GetPositions([FromQuery] int fileId)
    {
        var fileExists = await _db.ChessFiles.AnyAsync(f => f.ChessFileId == fileId);
        if (!fileExists) return NotFound("File not found.");

        var positions = await _db.Positions
            .Where(p => p.ChessFileId == fileId)
            .OrderBy(p => p.Ply)
            .ThenBy(p => p.SiblingOrder)
            .Select(NodeSelector)
            .ToListAsync();

        return Ok(positions);
    }

    // GET /api/positions/tree?fileId=2
    [HttpGet("tree")]
    public async Task<IActionResult> GetPositionTree([FromQuery] int fileId)
    {
        var fileExists = await _db.ChessFiles.AnyAsync(f => f.ChessFileId == fileId);
        if (!fileExists) return NotFound("File not found.");

        var rootPosition = await _db.Positions
            .Where(p => p.ChessFileId == fileId && p.ParentPositionId == null)
            .Select(NodeSelector)
            .FirstOrDefaultAsync();

        if (rootPosition == null) return NotFound("Root position not found for this file.");

        await BuildChildrenAsync(rootPosition);
        return Ok(rootPosition);
    }

    private async Task BuildChildrenAsync(PositionNode node) {
        var children = await _db.Positions
            .Where(p => p.ParentPositionId == node.PositionId)
            .OrderBy(p => p.SiblingOrder)
            .Select(NodeSelector)
            .ToListAsync();

        node.Children = children;
        foreach (var child in node.Children)
        {
            await BuildChildrenAsync(child);
        }
    }

    // GET /api/positions/{positionId}
    [HttpGet("{positionId}")]
    public async Task<IActionResult> GetPosition([FromRoute] int positionId)
    {
        var position = await _db.Positions
            .Where(p => p.PositionId == positionId)
            .Select(NodeSelector)
            .SingleOrDefaultAsync();

        if (position == null) return NotFound("Position not found.");

        return Ok(position);
    }

    // PUT /api/positions/{positionId}
    [HttpPut("{positionId}")]
    public async Task<IActionResult> PutNode([FromRoute] int positionId, [FromBody] UpdatePositionRequest req)
    {
        var position = await _db.Positions
            .FirstOrDefaultAsync(p => p.PositionId == positionId);

        if (position == null) return NotFound("Position not found.");

        if (req.Fen != null) position.Fen = req.Fen;
        if (req.MoveUci != null) position.MoveUci = req.MoveUci;
        if (req.MoveSan != null) position.MoveSan = req.MoveSan;

        await _db.SaveChangesAsync();

        var updatedNode = await _db.Positions
            .Where(p => p.PositionId == positionId)
            .Select(NodeSelector)
            .FirstAsync();

        return Ok(updatedNode);
    }

    // DELETE /api/positions/{positionId}
    [HttpDelete("{positionId}")]
    public async Task<IActionResult> DeleteNode([FromRoute] int positionId)
    {
        var position = await _db.Positions.FirstOrDefaultAsync(p => p.PositionId == positionId);

        if (position == null) return NotFound("Position not found.");

        // Optional: prevent deleting root
        if (position.ParentPositionId == null) return BadRequest("Cannot delete root position.");

        // Load all positions for this file
        var allPositions = await _db.Positions.Where(p => p.ChessFileId == position.ChessFileId).ToListAsync();

        // Build lookup: parentId -> children
        var lookup = allPositions.Where(p => p.ParentPositionId != null).GroupBy(p => p.ParentPositionId!.Value).ToDictionary(g => g.Key, g => g.ToList());

        // Collect subtree
        var toDelete = new List<Position>();

       void CollectDescendants(int id)
        {
            if (!lookup.TryGetValue(id, out var kids)) return;

            foreach (var child in kids)
            {
                toDelete.Add(child);
                CollectDescendants(child.PositionId);
            }
        }


        // Add the node itself
        toDelete.Add(position);

        // Collect its descendants
        CollectDescendants(positionId);

        _db.Positions.RemoveRange(toDelete);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
