using System.Reflection;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Raymond.Bot.Discord;

public static class DiscordExtensions
{
    extension(IServiceCollection services)
    {
        public void AddDiscordBot(string token,
            DiscordIntents intents,
            Assembly commandsAssembly,
            string commandPrefix,
            Assembly? eventHandlersAssembly = null)
        {
            // Add Discord client
            services.AddDiscordClient(token, intents)
                .ConfigureEventHandlers(b =>
                {
                    if (eventHandlersAssembly == null) return;
                
                    IReadOnlyList<Type> handlerTypes = eventHandlersAssembly.GetTypes()
                        .Where(t => t.IsClass && !t.IsAbstract)
                        .Where(t => typeof(BaseHandler).IsAssignableFrom(t))
                        .ToList();

                    b.AddEventHandlers(handlerTypes, ServiceLifetime.Scoped);
                });
        
            // Register Bot commands
            services.AddCommandsExtension((_, extension) =>
                {
                    extension.AddCommands(commandsAssembly);

                    var textProcessor = new TextCommandProcessor(
                        new TextCommandConfiguration
                        {
                            PrefixResolver = new DefaultPrefixResolver(true, commandPrefix).ResolvePrefixAsync
                        });

                    var slashProcessor = new SlashCommandProcessor();

                    extension.AddProcessors(textProcessor, slashProcessor);
                },
                new CommandsConfiguration
                {
                    RegisterDefaultCommandProcessors = true
                });
        }

        public void AddBotLogging()
        {
            services.AddLogging(logging =>
            {
#if DEBUG
                logging.SetMinimumLevel(LogLevel.Debug);
#else
            logging.SetMinimumLevel(LogLevel.Warning);
#endif
            
                logging.AddConsole();
            });
        }
    }
}