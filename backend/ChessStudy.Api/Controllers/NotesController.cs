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

    public record CreateNoteRequest(string? Text);

    // POST api/positions/{positionId}/notes
    [HttpPost("{positionId}/notes")]
    public async Task<ActionResult> CreateNoteForPosition([FromRoute] int positionId, [FromBody] CreateNoteRequest request)
    {
        var positionExists = await _db.Positions.AnyAsync(p => p.PositionId == positionId);
        if (!positionExists) {
            return NotFound("Position not found.");
        }

        var text = (request.Text ?? "").Trim();
        if (text.Length == 0) return BadRequest("Text cannot be empty.");
        note.Text = text;

        var note = new Note
        {
            PositionId = positionId,
            Text = text,
            CreatedAt = DateTime.UtcNow
        };

        _db.Notes.Add(note);
        await _db.SaveChangesAsync();

        return Created($"/api/notes/{note.NoteId}", new { note.NoteId, note.Text, note.CreatedAt, note.PositionId });
    }

    public record UpdateNoteRequest(string? Text);

    // PATCH api/notes/{noteId}
    [HttpPatch("/api/notes/{noteId}")]
    public async Task<ActionResult> UpdateNote([FromRoute] int noteId, [FromBody] UpdateNoteRequest request)
    {
        var note = await _db.Notes.FindAsync(noteId);
        if (note == null)
        {
            return NotFound("Note not found.");
        }

        if (request.Text is not null) note.Text = request.Text;
        note.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE api/notes/{noteId}
    [HttpDelete("/api/notes/{noteId}")]
    public async Task<ActionResult> DeleteNote([FromRoute] int noteId)
    {
        var note = await _db.Notes.FindAsync(noteId);
        if (note == null)
        {
            return NotFound("Note not found.");
        }

        _db.Notes.Remove(note);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}