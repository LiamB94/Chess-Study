using ChessStudy.Api.Data;
using ChessStudy.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ChessStudy.Api.Extensions;

namespace ChessStudy.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/files")]
public class ChessFilesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ChessFilesController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/files
    [HttpGet]
    public async Task<IActionResult> GetFiles()
    {
        var userId = User.GetUserId();

        var files = await _db.ChessFiles
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new
            {
                f.ChessFileId,
                f.Name,
                f.Description,
                f.CreatedAt
            })
            .ToListAsync();

        return Ok(files);
    }

    // GET /api/files/{fileId}
    [HttpGet("{fileId:int}")]
    public async Task<IActionResult> GetFileById([FromRoute] int fileId)
    {
        var userId = User.GetUserId();

        var file = await _db.ChessFiles
            .Where(f => f.ChessFileId == fileId && f.UserId == userId)
            .Select(f => new
            {
                f.ChessFileId,
                f.Name,
                f.Description,
                f.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (file == null) {
            return NotFound("File not found.");
        }

        return Ok(file);
    }

    public record CreateChessFileRequest(string Name, string? Description);

    [HttpPost]
    public async Task<IActionResult> CreateFile([FromBody] CreateChessFileRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest("Name is required.");

        var userId = User.GetUserId();
        var file = new ChessFile
        {
            UserId = userId,
            Name = req.Name.Trim(),
            Description = req.Description
        };

        _db.ChessFiles.Add(file);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFileById), new { fileId = file.ChessFileId }, new
        {
            file.ChessFileId,
            file.Name,
            file.Description,
            file.CreatedAt
        });
    }

    public record UpdateChessFileRequest(string? Name, string? Description);

    [HttpPatch("{fileId}")]
    public async Task<IActionResult> UpdateFile([FromRoute] int fileId, [FromBody] UpdateChessFileRequest req) {
        var userId = User.GetUserId();
        var file = await _db.ChessFiles.FirstOrDefaultAsync(f => f.ChessFileId == fileId && f.UserId == userId);
        if (file == null) return NotFound("File not found.");

        var changed = false;

        if (req.Name != null) {
            var trimmed = req.Name.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) return BadRequest("Name cannot be empty.");

            file.Name = trimmed;
            changed = true;
        }

        if (req.Description != null) {
            file.Description = req.Description;
            changed = true;
        }

        if (!changed) return BadRequest("No fields provided to update.");

        await _db.SaveChangesAsync();
        return NoContent();
    }



    [HttpDelete("{fileId}")]
    public async Task<IActionResult> DeleteFile([FromRoute] int fileId) 
    {
        var userId = User.GetUserId();
        var file = await _db.ChessFiles.FirstOrDefaultAsync(f => f.ChessFileId == fileId && f.UserId == userId);
        if (file == null) return NotFound("File not found.");

        _db.ChessFiles.Remove(file);
        await _db.SaveChangesAsync();
        return NoContent();
    }

}
