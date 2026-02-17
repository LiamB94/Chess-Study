using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChessStudy.Api.Models;

public class Position
{
    public int PositionId { get; set; }

    [Required]
    public int ChessFileId { get; set; }

    [ForeignKey(nameof(ChessFileId))]
    public ChessFile ChessFile { get; set; } = null!;

    // Parent node - should be null for root
    public int? ParentPositionId { get; set; }

    [ForeignKey(nameof(ParentPositionId))]
    public Position? ParentPosition { get; set; }

    //children nodes
    public List<Position> ChildPositions { get; set; } = new();

    // move that creates new postition (parent -> this)
    [MaxLength(12)]
    public string? MoveUci { get; set; } 

    [MaxLength(32)]
    public string? MoveSan { get; set; }

    [Required]
    public string Fen { get; set; } = string.Empty;

    public int Ply { get; set; } // 0 for root +1 for each move

    public int SiblingOrder { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}