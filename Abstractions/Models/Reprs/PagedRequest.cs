namespace Corevia.Transporter.Abstractions;

public class PagedRequest
{
    public int Top { get; set; }
    public int Skip { get; set; }
    public string Query { get; set; }
    public string Sort { get; set; } = "Id";
}