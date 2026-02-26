using ChessStudy.api.Data; 
using ChessStudy.api.Models;
using Microsoft.AspNetCore.Mvc; 
using Microsoft.EntityFrameworkCore; 
namespace ChessStudy.api.Controllers; 

[ApiController] 
[Route("api/positions")] 
public class NotesController : ControllerBase 
{ 
    private readonly AppDbContext _db; 
    public NotesController(AppDbContext db) 
    { 
        _db = db; 
    }

    // Get api/positions/{positionId}/notes
    [HttpGet("{positionId}/notes")]
    public async Task<ActionResult> GetNotesForPosition([FromRoute] int positionId)
    {
        // check if position exists
        var positionExists = await _db.Positions.AnyAsync(p => p.PositionId == positionId);
        if (!positionExists) {
            return NotFound("Position not found.");
        }
        var notes = await _db.Notes.Where(n => n.PositionId == positionId).Select(n => new { n.NoteId, n.Text, n.CreatedAt }).OrderByDescending(n => n.CreatedAt).ToListAsync();

        return Ok(notes);
    }
}