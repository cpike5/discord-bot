using Discord;
using Discord.WebSocket;

namespace DiscordBotAPI.Commands
{
    /// <summary>
    /// Interface for Discord slash command implementations
    /// </summary>
    public interface ISlashCommand
    {
        /// <summary>
        /// Gets the command builder with name, description, and parameters
        /// </summary>
        /// <returns>A configured SlashCommandBuilder</returns>
        SlashCommandBuilder GetCommandBuilder();

        /// <summary>
        /// Handles the execution of the command
        /// </summary>
        /// <param name="command">The command interaction to handle</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task HandleCommandAsync(SocketSlashCommand command);
    }


}