using DSharpPlus.Entities;

namespace Raymond.Bot.Discord;

public class M
{
    public string? Content { get; set; }
    public E? Embed { get; set; }
    public List<Button>? Buttons { get; set; }
    public AttachedFile? File { get; set; }
    public bool EveryoneMention { get; set; } = false;
    public List<DiscordRole>? RoleMentions { get; set; }
    public List<DiscordUser>? UserMentions { get; set; }

    public DiscordMessageBuilder Build()
    {
        var builder = new DiscordMessageBuilder
        {
            Content = Content ?? string.Empty
        };

        if (Embed != null)
        {
            builder.AddEmbed(Embed.Build());
        }

        if (Buttons != null)
        {
            List<DiscordButtonComponent> buttons = [];
            foreach (var button in Buttons)
            {
                buttons.Add(new DiscordButtonComponent(button.Style, button.Id, button.Text, button.Disabled, new DiscordComponentEmoji(button.Emoji)));
            }
            
            builder.AddActionRowComponent(buttons);
        }
        
        if (File != null)
        {
            builder.AddFile(File.FileName, File.Stream);
        }

        if (UserMentions is not null)
        {
            foreach (var user in UserMentions)
            {
                builder.AddMention(new UserMention(user));
            }
        }
        
        if (RoleMentions is not null)
        {
            foreach (var role in RoleMentions)
            {
                builder.AddMention(new RoleMention(role));
            }
        }

        if (EveryoneMention)
        {
            builder.AddMention(new EveryoneMention());
        }

        return builder;
    }
}

public record Button(DiscordButtonStyle Style, string Id, string Text, string Emoji, bool Disabled = false);

public record AttachedFile(string FileName, Stream Stream);