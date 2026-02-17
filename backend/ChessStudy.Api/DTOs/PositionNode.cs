namespace ChessStudy.Api.DTOs;

public class PositionNode
{
    public int PositionId { get; set; }
    public int? ParentPositionId { get; set; }
    public string Fen { get; set; } = string.Empty;
    public string? MoveUci { get; set; }
    public string? MoveSan { get; set; }
    public int Ply { get; set; }
    public int SiblingOrder { get; set; }

    public List<PositionNode> Children { get; set; } = new();
}
