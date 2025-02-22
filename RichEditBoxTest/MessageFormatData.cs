using Newtonsoft.Json;
using System.Collections.Generic;

namespace RichEditBoxTest
{
    public static class MessageFormatDataTypes {
        public const string BOLD = "bold";
        public const string ITALIC = "italic";
        public const string UNDERLINE = "underline";
        public const string LINK = "url";
    }

    public class MessageFormatDataItem {
        [JsonProperty("offset")]
        public int Offset { get; set; }

        [JsonProperty("length")]
        public int Length { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class MessageFormatData {
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("items")]
        public List<MessageFormatDataItem> Items { get; set; }
    }
}
