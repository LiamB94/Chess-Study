using ChessStudy.Api.Data;
using ChessStudy.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ChessStudy.Api.Extensions;

namespace ChessStudy.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/positions")]
public class ArrowsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ArrowsController(AppDbContext db)
    {
        _db = db;
    }


    // GET /api/positions/{positionId}/arrows
    [HttpGet("{positionId}/arrows")]
    public async Task<IActionResult> GetArrowsForPosition([FromRoute] int positionId) {
        var userId = User.GetUserId();
        var positionExists = await _db.Positions.AnyAsync(p => p.PositionId == positionId && p.ChessFile.UserId == userId);
        if (!positionExists) {
            return NotFound("Position not found.");
        }

        var arrows = await _db.Arrows
            .Where(a => a.PositionId == positionId)
            .Select(a => new { a.ArrowId, a.FromSquare, a.ToSquare})
            .OrderByDescending(a => a.ArrowId)
            .ToListAsync();

        return Ok(arrows);
    }

    public record CreateArrowRequest(string FromSquare, string ToSquare);

    // POST /api/positions/{positionId}/arrows
    [HttpPost("{positionId}/arrows")]
    public async Task<IActionResult> CreateArrowForPosition([FromRoute] int positionId, [FromBody] CreateArrowRequest request) {
        var userId = User.GetUserId();
        var positionExists = await _db.Positions.AnyAsync(p => p.PositionId == positionId && p.ChessFile.UserId == userId);
        if (!positionExists) {
            return NotFound("Position not found.");
        }

        var fromSquare = request.FromSquare.Trim().ToLower();
        var toSquare = request.ToSquare.Trim().ToLower();

        if (!IsValidSquare(fromSquare) || !IsValidSquare(toSquare)) {
            return BadRequest("Invalid square. Must be in format 'e2'.");
        }
        if (fromSquare == toSquare) {
            return BadRequest("FromSquare and ToSquare cannot be the same.");
        }

        var arrow = new Arrow
        {
            PositionId = positionId,
            FromSquare = fromSquare,
            ToSquare = toSquare
        };

        _db.Arrows.Add(arrow);
        await _db.SaveChangesAsync();

        return Created($"/api/arrows/{arrow.ArrowId}", new { arrow.ArrowId, arrow.FromSquare, arrow.ToSquare, arrow.PositionId });
    }

    private bool IsValidSquare(string square) {
        if (square.Length != 2) return false;
        char file = square[0];
        char rank = square[1];
        return file >= 'a' && file <= 'h' && rank >= '1' && rank <= '8';
    }

    // DELETE /api/arrows/{arrowId}
    [HttpDelete("/api/arrows/{arrowId}")]
    public async Task<IActionResult> DeleteArrow([FromRoute] int arrowId) {
        var userId = User.GetUserId();
        var arrow = await _db.Arrows.FirstOrDefaultAsync(a => a.ArrowId == arrowId && a.Position.ChessFile.UserId == userId);
        if (arrow == null) {
            return NotFound("Arrow not found.");
        }

        _db.Arrows.Remove(arrow);
        await _db.SaveChangesAsync();

        return NoContent();
    }

}