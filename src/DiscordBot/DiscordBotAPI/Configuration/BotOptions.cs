namespace DiscordBotAPI.Configuration
{
    public class BotOptions
    {
        public readonly static string SectionName = "DiscordBot";

        public string Token { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;

        public string DefaultGuildId { get; set; } = string.Empty;
        public string DefaultChannelId { get; set; } = string.Empty;

    }
}
