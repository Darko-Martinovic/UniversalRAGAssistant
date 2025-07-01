namespace UniversalRAGAssistant.Models
{
    public class ChatResponse
    {
        public string id { get; set; } = string.Empty;
        public string @object { get; set; } = string.Empty;
        public long created { get; set; }
        public string model { get; set; } = string.Empty;
        public ChatChoice[] choices { get; set; } = Array.Empty<ChatChoice>();
        public Usage usage { get; set; } = new Usage();
    }
}
