using System.Text;

namespace Raymond.Bot.Utilities;

public sealed class Paginator<T>
{
    public const string FirstPage = "first";
    public const string LastPage = "last";
    
    public Paginator(IEnumerable<T> items, int itemsPerPage, int currentPage)
    {
        if (itemsPerPage <= 0)
            throw new ArgumentOutOfRangeException(nameof(itemsPerPage));

        Items = items.ToList();

        ItemsPerPage = itemsPerPage;
        TotalPages = Math.Max(1, (int)Math.Ceiling(Items.Count / (double)itemsPerPage));
        CurrentPage = Math.Clamp(currentPage, 1, TotalPages);
    }

    public Paginator(IEnumerable<T> items, int itemsPerPage, string pageToken)
    {
        if (itemsPerPage <= 0)
            throw new ArgumentOutOfRangeException(nameof(itemsPerPage));

        var masterItems = items.ToList();
        Items = masterItems;

        ItemsPerPage = itemsPerPage;
        TotalPages = Math.Max(1, (int)Math.Ceiling(masterItems.Count / (double)itemsPerPage));
        CurrentPage = ResolvePage(pageToken);
    }

    public int ItemsPerPage { get; }
    public int CurrentPage { get; }
    public int TotalPages { get; }
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public string PageStatus => $"Page {CurrentPage} of {TotalPages}";
    
    public IReadOnlyList<T> Items => field.Skip((CurrentPage - 1) * ItemsPerPage).Take(ItemsPerPage).ToList();

    public string RenderPage(Func<T, int, string> formatter)
    {
        var builder = new StringBuilder();
        
        for (var i = 0; i < Items.Count; i++)
        {
            var globalIndex = (CurrentPage - 1) * ItemsPerPage + i + 1;
            builder.AppendLine(formatter(Items[i], globalIndex));
        }

        return builder.ToString();
    }
    
    private int ResolvePage(string pageToken)
    {
        return pageToken.ToLowerInvariant() switch
        {
            FirstPage => 1,
            LastPage => TotalPages,
            _ when int.TryParse(pageToken, out var page) => Math.Clamp(page, 1, TotalPages),
            _ => 1
        };
    }
}