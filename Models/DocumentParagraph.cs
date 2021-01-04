using Newtonsoft.Json;

namespace WorkReportCreator.Models
{
    public class DocumentParagraph
    {
        [JsonIgnore]
        public string Text { get; set; }

        public string FontFamily { get; set; }

        public int FontSize { get; set; }

        public string Style { get; set; }

        public DocumentParagraph(string text, string fontFamily = "Times New Roman", int fontSize = 14, string style = null)
        {
            Text = text;
            FontFamily = fontFamily;
            FontSize = fontSize;
            Style = style;
        }
    }
}
