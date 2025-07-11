namespace UniversalRAGAssistant.Models
{
    public class ChatRequest
    {
        public ChatMessage[] messages { get; set; } = Array.Empty<ChatMessage>();
        public int max_tokens { get; set; } = 4096;
        public float temperature { get; set; } = 0.7f;
        public float top_p { get; set; } = 0.95f;
    }
}
