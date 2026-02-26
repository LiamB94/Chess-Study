using ChessStudy.Api.Data;
using ChessStudy.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChessStudy.Api.DTOs;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;

namespace ChessStudy.Api.Controllers;

[ApiController]
[Authorize]
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

    public record UpdatePositionRequest(string? Fen, string? MoveUci, string? MoveSan);


    // GET /api/positions/tree?fileId=2
    [HttpGet("tree")]
    public async Task<IActionResult> GetPositionTree([FromQuery] int fileId)
    {

        var nodes = await _db.Positions
            .Where(p => p.ChessFileId == fileId)
            .Select(NodeSelector)
            .ToListAsync();

        if (nodes.Count == 0) return NotFound("File or Positions not found.");

        var root = nodes.SingleOrDefault(n => n.ParentPositionId == null);
        if (root == null) return NotFound("Root position not found for this file.");

        var childrenByParent = nodes
            .Where(n => n.ParentPositionId != null)
            .GroupBy(n => n.ParentPositionId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(n => n.SiblingOrder).ToList());

        void Attach(PositionNode node)
        {
            if (!childrenByParent.TryGetValue(node.PositionId, out var kids)) return;
            node.Children = kids;
            foreach (var k in kids) Attach(k);
        }

        Attach(root);
        return Ok(root);

    }

    public record CreateRootPositionRequest(string Fen);

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

        return CreatedAtAction(nameof(GetPosition), new { positionId = root.PositionId }, new
        {
            root.PositionId,
            root.ChessFileId,
            root.Fen
        });
    }

    public record CreateChildPositionRequest(int ParentPositionId, string Fen, string? MoveUci, string? MoveSan, int? InsertAt);

    // POST /api/positions/child
    [HttpPost("child")]
    public async Task<IActionResult> CreateChild([FromBody] CreateChildPositionRequest req)
    {

        var parent = await _db.Positions.FirstOrDefaultAsync(p => p.PositionId == req.ParentPositionId);
        if (parent == null) return NotFound("Parent position not found.");

        using var tx = await _db.Database.BeginTransactionAsync();

        var siblings = await _db.Positions
            .Where(p => p.ParentPositionId == parent.PositionId)
            .OrderBy(p => p.SiblingOrder)
            .ToListAsync();

        var insertAt = req.InsertAt.HasValue ? Math.Clamp(req.InsertAt.Value, 0, siblings.Count) : siblings.Count;

        for (int i = insertAt; i < siblings.Count; i++)
            siblings[i].SiblingOrder += 1;

        var child = new Position
        {
            ChessFileId = parent.ChessFileId,
            ParentPositionId = parent.PositionId,
            Fen = req.Fen,
            MoveUci = req.MoveUci,
            MoveSan = req.MoveSan,
            Ply = parent.Ply + 1, // MIGHT CHANGE PLY OUT
            SiblingOrder = insertAt
        };

        _db.Positions.Add(child);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return CreatedAtAction(nameof(GetPosition), new { positionId = child.PositionId }, new
        {
            child.PositionId,
            child.ParentPositionId,
            child.ChessFileId,
            child.MoveUci,
            child.MoveSan,
            child.SiblingOrder
        });

    }


    public record CreateRootParentRequest(string Fen, string? MoveUci, string? MoveSan);

    [HttpPost("{rootId}/prepend-root")]
    public async Task<IActionResult> AddParentAboveRoot([FromRoute] int rootId, [FromBody] CreateRootParentRequest req)
    {
        using var tx = await _db.Database.BeginTransactionAsync();

        var oldRoot = await _db.Positions.FirstOrDefaultAsync(p => p.PositionId == rootId);
        if (oldRoot == null) return NotFound("Root position not found.");

        if (oldRoot.ParentPositionId != null)
            return BadRequest("Can only add a parent above the current root.");

        // New root
        var newRoot = new Position
        {
            ChessFileId = oldRoot.ChessFileId,
            ParentPositionId = null,
            Fen = req.Fen,
            MoveUci = req.MoveUci,
            MoveSan = req.MoveSan,
            Ply = 0,            // potentially remove lateer
            SiblingOrder = 0
        };

        _db.Positions.Add(newRoot);
        await _db.SaveChangesAsync(); // get PositionId

        // Make old root a child of new root 
        oldRoot.ParentPositionId = newRoot.PositionId;
        oldRoot.SiblingOrder = 0;

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        var created = await _db.Positions
            .Where(p => p.PositionId == newRoot.PositionId)
            .Select(NodeSelector)
            .FirstAsync();

        return CreatedAtAction(nameof(GetPosition), new { positionId = newRoot.PositionId }, created);
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

    // SOMEWHAT REDUNDANT WITH TREE ENDPOINT
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

    // PATCH /api/positions/{positionId}
    [HttpPatch("{positionId}")]
    public async Task<IActionResult> PatchNode([FromRoute] int positionId, [FromBody] UpdatePositionRequest req)
    {
        var position = await _db.Positions.FirstOrDefaultAsync(p => p.PositionId == positionId);
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


    [HttpDelete("{positionId}")]
    public async Task<IActionResult> DeleteNode([FromRoute] int positionId)
    {
        using var tx = await _db.Database.BeginTransactionAsync();

        var position = await _db.Positions.FirstOrDefaultAsync(p => p.PositionId == positionId);
        if (position == null) return NotFound("Position not found.");

        if (position.ParentPositionId == null)
            return BadRequest("Cannot delete root position.");

        var parentId = position.ParentPositionId.Value;
        var fileId = position.ChessFileId;

        // Load all positions for this file (single query)
        var allPositions = await _db.Positions
            .Where(p => p.ChessFileId == fileId)
            .ToListAsync();

        // Build lookup: parentId -> children
        var lookup = allPositions
            .Where(p => p.ParentPositionId != null)
            .GroupBy(p => p.ParentPositionId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

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

        // include the node itself + descendants
        toDelete.Add(position);
        CollectDescendants(positionId);

        _db.Positions.RemoveRange(toDelete);
        await _db.SaveChangesAsync();

        // Compact remaining siblings under the old parent
        var remainingSiblings = await _db.Positions
            .Where(p => p.ParentPositionId == parentId)
            .OrderBy(p => p.SiblingOrder)
            .ToListAsync();

        for (int i = 0; i < remainingSiblings.Count; i++)
            remainingSiblings[i].SiblingOrder = i;

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return NoContent();
    }
                            

    // PATCH /api/positions/{parentId}/reorder
    [HttpPatch("{parentId}/reorder")]
    public async Task<IActionResult> ReorderSiblings([FromRoute] int parentId, [FromBody] List<int> orderedIds)
    {
        var siblings = await _db.Positions
            .Where(p => p.ParentPositionId == parentId)
            .ToListAsync();

        if (siblings.Count == 0)
            return NotFound("No siblings found for this parent.");

        if (orderedIds.Count != siblings.Count)
            return BadRequest("Invalid sibling IDs.");

        if (orderedIds.Distinct().Count() != orderedIds.Count)
            return BadRequest("Duplicate IDs.");

        var siblingById = siblings.ToDictionary(s => s.PositionId);
        foreach (var id in orderedIds)
            if (!siblingById.ContainsKey(id))
                return BadRequest("Invalid sibling IDs.");

        for (int i = 0; i < orderedIds.Count; i++)
            siblingById[orderedIds[i]].SiblingOrder = i;

        await _db.SaveChangesAsync();
        return NoContent();
    }
}
