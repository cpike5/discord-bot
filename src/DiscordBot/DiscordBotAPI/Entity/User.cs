using Discord;
using Discord.WebSocket;

namespace DiscordBotAPI.Entity
{
    public class User
    {
        public ulong Id { get; set; }
        public string Username { get; set; }
        public string? Email { get; set; } = string.Empty;

        public User(ulong id, string username, string? email = "")
        {
            Id = id;
            Username = username;
            Email = email;
        }

        public User(IUser user)
        {
            Id = user.Id;
            Username = user.Username;
        }
    }
}
