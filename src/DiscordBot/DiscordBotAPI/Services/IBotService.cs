namespace DiscordBotAPI.Services
{
    public interface IBotService
    {
        Task StartBotAsync();
        Task StopBotAsync();
    }
}
