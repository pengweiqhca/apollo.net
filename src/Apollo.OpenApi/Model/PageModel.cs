namespace Com.Ctrip.Framework.Apollo.OpenApi.Model;

public class PageModel<T>
{
    public IReadOnlyList<T>? Content { get; set; }

    public int Page { get; set; }

    public int Size { get; set; }

    public int Total { get; set; }
}
