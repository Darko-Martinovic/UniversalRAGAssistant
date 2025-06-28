namespace AzureOpenAIConsole.Models
{
    public class ChatResponse
    {
        public ChatChoice[] choices { get; set; }
        public Usage usage { get; set; }
    }
}
