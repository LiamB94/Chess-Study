using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChessStudy.Api.Models;

public class Arrow {
    public int ArrowId { get; set; }

    [Required]
    public int PositionId { get; set; }

    [ForeignKey(nameof(PositionId))]
    public Position Position { get; set; } = null!;

    [Required]
    [MaxLength(5)]
    public string FromSquare { get; set; } = string.Empty; // e.g. "e2"
    
    [Required]
    [MaxLength(5)]
    public string ToSquare { get; set; } = string.Empty; // e.g. "e4"

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}