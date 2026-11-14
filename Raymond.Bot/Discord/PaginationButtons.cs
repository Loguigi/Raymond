using DSharpPlus.Entities;
using Raymond.Bot.Utilities;

namespace Raymond.Bot.Discord;

public static class PaginationButtons
{
    public static List<Button> Create<T>(Paginator<T> paginator, Func<string, string> customIdFactory)
    {
        return
        [
            new Button(
                DiscordButtonStyle.Primary,
                customIdFactory(Paginator<T>.FirstPage),
                "",
                "⏮️",
                paginator.CurrentPage == 1),

            new Button(
                DiscordButtonStyle.Primary,
                customIdFactory((paginator.CurrentPage - 1).ToString()),
                "",
                "⏪",
                !paginator.HasPreviousPage),

            new Button(
                DiscordButtonStyle.Primary,
                customIdFactory((paginator.CurrentPage + 1).ToString()),
                "",
                "⏩",
                paginator.CurrentPage == paginator.TotalPages),
            
            new Button(
                DiscordButtonStyle.Primary,
                customIdFactory(Paginator<T>.LastPage),
                "",
                "⏭️",
                paginator.CurrentPage == paginator.TotalPages)
        ];
    }
}