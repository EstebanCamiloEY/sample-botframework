using Microsoft.Bot.Schema;

namespace Sample_BF
{
    public class ConversationData
    {
        public string Transport { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        public Attachment Picture { get; set; }
    }
}
