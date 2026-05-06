namespace Corevia.Transporter.Abstractions;

public class PagedResponse<TResponse>
{
    public ICollection<TResponse> Items { get; set; }
    public int TotalCount { get; set; }
}