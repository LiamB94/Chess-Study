using ChessStudy.Api.Data;
using ChessStudy.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChessStudy.Api.Controllers;

[ApiController]
[Route("api/files")]
public class ChessFilesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ChessFilesController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/files?userId=1
    [HttpGet]
    public async Task<IActionResult> GetFiles([FromQuery] int userId)
    {
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


    public record CreateChessFileRequest(string Name, string? Description);

    [HttpPost]
    public async Task<IActionResult> CreateFile([FromQuery] int userId, [FromBody] CreateChessFileRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest("Name is required.");

        var file = new ChessFile
        {
            UserId = userId,
            Name = req.Name.Trim(),
            Description = req.Description
        };

        _db.ChessFiles.Add(file);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFiles), new { userId }, new
        {
            file.ChessFileId,
            file.Name,
            file.Description,
            file.CreatedAt
        });
    }

}
