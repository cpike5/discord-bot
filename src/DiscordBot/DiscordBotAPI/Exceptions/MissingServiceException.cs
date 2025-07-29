namespace DiscordBotAPI.Exceptions
{
    [Serializable]
    internal class MissingServiceException : Exception
    {
        public MissingServiceException()
        {
        }

        public MissingServiceException(string? message) : base(message)
        {
        }

        public MissingServiceException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
