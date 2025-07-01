namespace UniversalRAGAssistant.Models
{
    public class ChatChoice
    {
        public ChatMessage message { get; set; } = new ChatMessage();
        public string finish_reason { get; set; } = string.Empty;
    }
}
