using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBotAPI.Commands;
using DiscordBotAPI.Configuration;
using DiscordBotAPI.Services;

namespace DiscordBotAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBotServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure the options
            services.Configure<BotOptions>(configuration.GetSection(BotOptions.SectionName));

            // Register the core bot services
            services.RegisterCoreBotServices();

            // Register the consent services
            services.RegisterConsentServices();

            return services;
        }

        private static IServiceCollection RegisterCoreBotServices(this IServiceCollection services)
        {
            // Register the command registration service
            // Register the Discord client
            services.AddSingleton<DiscordSocketClient>(provider =>
            {
                var config = new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Info,
                    MessageCacheSize = 1000,
                    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.DirectMessages | GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
                };
                return new DiscordSocketClient(config);
            });

            // Register the bot service
            services.AddHostedService<BotService>();

            // Register the logger
            services.AddScoped<IDiscordBotLoggerService, DiscordBotLoggerService>();

            // Rester the interaction service
            services.AddSingleton<InteractionService>(provider =>
            {
                var discordClient = provider.GetRequiredService<DiscordSocketClient>();
                return new InteractionService(discordClient, new InteractionServiceConfig
                {
                    AutoServiceScopes = true
                });
            });

            // Resister the command handler
            services.AddSingleton<CommandScanner>();
            services.AddScoped<ICommandRegistrationService, CommandRegistrationService>();
            services.AddScoped<CommandHandlerService>();

            // Reigster the message handler
            services.AddScoped<IMessageHandler, LogMessageHandler>();

            return services;
        }

        private static IServiceCollection RegisterConsentServices(this IServiceCollection services)
        {
            // Register the consent service
            services.AddTransient<IConsentService, FileConsentService>();
            return services;
        }
    }
}
