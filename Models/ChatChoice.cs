namespace AzureOpenAIConsole.Models
{
    public class ChatChoice
    {
        public ChatMessage message { get; set; }
        public string finish_reason { get; set; }
    }
}
