using System;
using System.Collections.Generic;
using Windows.UI.Text;
using Windows.UI;

namespace RichEditBoxTest
{
    class TextFormatConverter {
        // From VK's MessageFormat to Windows' format
        public static void FromVKFormat(ITextDocument document, MessageFormatData format) {
            if (format == null) return;
            document.GetText(TextGetOptions.AdjustCrlf | TextGetOptions.NoHidden, out string text);
            document.SetText(TextSetOptions.None, text);
            var textLength = text.Length;
            foreach (var item in format.Items) {
                SetFormatByItem(document, item, textLength);
            }
        }

        private static void SetFormatByItem(ITextDocument document, MessageFormatDataItem item, int maxLength) {
            int start = item.Offset;
            int end = Math.Min(maxLength, start + item.Length);

            var range = document.GetRange(start, end);
            var format = range.CharacterFormat;
            switch (item.Type) {
                case MessageFormatDataTypes.BOLD:
                    format.Bold = FormatEffect.On;
                    break;
                case MessageFormatDataTypes.ITALIC:
                    format.Italic = FormatEffect.On;
                    break;
                case MessageFormatDataTypes.UNDERLINE:
                    format.Underline = UnderlineType.Single;
                    break;
                case MessageFormatDataTypes.LINK:
                    range.Link = $"\"{item.Url}\"";
                    format.ForegroundColor = Color.FromArgb(255, 0, 122, 204);
                    format.Outline = FormatEffect.On;
                    break;
            }
        }

        // From Windows' format to VK's MessageFormat
        public static MessageFormatData ToVKFormat(ITextDocument document) {
            MessageFormatData data = new MessageFormatData {
                Version = 1,
                Items = new List<MessageFormatDataItem>()
            };
            data.Items.Clear();

            document.GetText(TextGetOptions.AdjustCrlf | TextGetOptions.NoHidden, out string text);
            int length = text.Length;

            data.Items.AddRange(GetItemsByType(document, length, f => f.Bold == FormatEffect.On, TextRangeUnit.Bold, MessageFormatDataTypes.BOLD));
            data.Items.AddRange(GetItemsByType(document, length, f => f.Italic == FormatEffect.On, TextRangeUnit.Italic, MessageFormatDataTypes.ITALIC));
            data.Items.AddRange(GetItemsByType(document, length, f => f.Underline == UnderlineType.Single, TextRangeUnit.Underline, MessageFormatDataTypes.UNDERLINE));
            data.Items.AddRange(GetLinks(document, length));

            return data;
        }

        private static List<MessageFormatDataItem> GetItemsByType(ITextDocument document, int length, Predicate<ITextCharacterFormat> condition, TextRangeUnit textRangeUnit, string itemType) {
            List<MessageFormatDataItem> items = new List<MessageFormatDataItem>();
            for (int i = 0; i < length; i++) {
                var range = document.GetRange(i, i + 1);
                var format = range.CharacterFormat;
                var clone = range.GetClone();
                if (condition(format)) {
                    int expanded = clone.Expand(textRangeUnit);
                    int rangeLength = expanded + 1;
                    items.Add(new MessageFormatDataItem {
                        Type = itemType,
                        Offset = i,
                        Length = rangeLength
                    });
                    i += rangeLength;
                }
            }
            return items;
        }

        private static List<MessageFormatDataItem> GetLinks(ITextDocument document, int length) {
            List<MessageFormatDataItem> items = new List<MessageFormatDataItem>();
            for (int i = 0; i < length; i++) {
                var range = document.GetRange(i, i + 1);
                var format = range.CharacterFormat;
                var clone = range.GetClone();
                clone.StartOf(TextRangeUnit.Link, true);
                if (!string.IsNullOrEmpty(clone.Link)) {
                    range.Expand(TextRangeUnit.Link);
                    range.GetText(TextGetOptions.NoHidden, out string text);
                    int rangeLength = text.Length;
                    items.Add(new MessageFormatDataItem {
                        Type = MessageFormatDataTypes.LINK,
                        Offset = i,
                        Length = rangeLength,
                        Url = range.Link.Trim('"')
                    });
                    i += rangeLength;
                }
            }
            return items;
        }

        public static void CloneTextWithFormat(ITextDocument first, ITextDocument second) {
            first.GetText(TextGetOptions.AdjustCrlf, out string dump);
            var range = first.GetRange(0, dump.Length);

            second.SetText(TextSetOptions.None, dump);
            var range2 = second.GetRange(0, dump.Length);
            range2.FormattedText = range.FormattedText;
        }
    }
}
