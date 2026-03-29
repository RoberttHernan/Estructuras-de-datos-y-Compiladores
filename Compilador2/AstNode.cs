public class AstNode
{
    public string tipo { get; set; }

    public List<AstNode> hijos { get; set; } = new List<AstNode>();
}