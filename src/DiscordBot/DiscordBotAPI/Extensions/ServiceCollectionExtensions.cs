using Discord;
using Discord.WebSocket;
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

            // Register the Discord client
            services.AddSingleton<DiscordSocketClient>(provider =>
            {
                var config = new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Info,
                    MessageCacheSize = 1000
                };
                return new DiscordSocketClient(config);
            });

            services.AddScoped<IDiscordBotLoggerService, DiscordBotLoggerService>();
            services.AddScoped<IMessageHandler, LogMessageHandler>();

            // Register the bot service
            services.AddHostedService<BotService>();
            
            return services;
        }
    }
}
