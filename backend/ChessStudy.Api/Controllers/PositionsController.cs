using ChessStudy.Api.Data;
using ChessStudy.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChessStudy.Api.Controllers;

[ApiController]
[Route("api/positions")]
public class PositionsController : ControllerBase
{
    private readonly AppDbContext _db;

    public PositionsController(AppDbContext db)
    {
        _db = db;
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
}
