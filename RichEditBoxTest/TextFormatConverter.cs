using System;
using System.Collections.Generic;
using Windows.UI.Text;
using Windows.UI;
using System.Diagnostics;
using Windows.UI.Xaml.Documents;

namespace RichEditBoxTest
{
    class TextFormatConverter {
        struct FormatInfo {
            public string Type;
            public int Offset;
            public int Length;
            public string Url;
        }


        // From VK's MessageFormat to Windows' format
        public static void FromVKFormat(ITextDocument document, MessageFormatData format) {
            if (format == null) return;
            document.BatchDisplayUpdates();
            document.GetText(TextGetOptions.AdjustCrlf | TextGetOptions.NoHidden, out string text);
            document.Selection.CharacterFormat = document.GetDefaultCharacterFormat();
            document.SetText(TextSetOptions.None | TextSetOptions.ApplyRtfDocumentDefaults, text);
            var textLength = text.Length;
            foreach (var item in format.Items) {
                SetFormatByItem(document, item, textLength);
            }
            document.ApplyDisplayUpdates();
        }

        private static void SetFormatByItem(ITextDocument document, MessageFormatDataItem item, int maxLength) {
            int start = item.Offset;
            int end = Math.Min(maxLength, start + item.Length);

            var range = document.GetRange(start, end);
            var format = range.CharacterFormat;
            var u = format.Underline;

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
                    format.Underline = UnderlineType.None;
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

            document.GetText(TextGetOptions.AdjustCrlf, out string text);
            int length = text.Length;
            int hidden = 0;

            FormatInfo bold = new FormatInfo { Type = MessageFormatDataTypes.BOLD, Offset = 0, Length = 0 };
            FormatInfo italic = new FormatInfo { Type = MessageFormatDataTypes.ITALIC, Offset = 0, Length = 0 };
            FormatInfo underline = new FormatInfo { Type = MessageFormatDataTypes.UNDERLINE, Offset = 0, Length = 0 };
            FormatInfo link = new FormatInfo { Type = MessageFormatDataTypes.LINK, Offset = 0, Length = 0 };

            for (int i = 0; i < length; i++) {
                var range = document.GetRange(i, i + 1);
                if (range.CharacterFormat.Hidden == FormatEffect.On) {
                    range.Expand(TextRangeUnit.Hidden);
                    hidden += range.Length;
                    i += range.Length - 1;
                    continue;
                }

                var format = range.CharacterFormat;
                bool last = length - 1 == i;
                CheckTextFormat(i - hidden, format.Bold, ref bold, data.Items, last);
                CheckTextFormat(i - hidden, format.Italic, ref italic, data.Items, last);
                CheckTextFormatUnderline(i - hidden, format.Underline, ref underline, data.Items, last);
                CheckTextFormatLink(i - hidden, range.Link, ref link, data.Items, last);
            }

            return data;
        }

        private static void CheckTextFormat(int offset, FormatEffect effect, ref FormatInfo formatInfo, List<MessageFormatDataItem> items, bool last) {
            if (effect == FormatEffect.On) {
                if (formatInfo.Length > 0) {
                    formatInfo.Length++;
                } else {
                    formatInfo.Offset = offset;
                    formatInfo.Length = 1;
                }
            } else if (last || (effect == FormatEffect.Off && formatInfo.Length > 0)) {
                items.Add(new MessageFormatDataItem {
                    Type = formatInfo.Type,
                    Offset = formatInfo.Offset,
                    Length = formatInfo.Length
                });
                formatInfo.Length = 0;
            }
        }

        private static void CheckTextFormatUnderline(int offset, UnderlineType effect, ref FormatInfo formatInfo, List<MessageFormatDataItem> items, bool last) {
            if (effect == UnderlineType.Single) {
                if (formatInfo.Length > 0) {
                    formatInfo.Length++;
                } else {
                    formatInfo.Offset = offset;
                    formatInfo.Length = 1;
                }
            } else if (last || (effect != UnderlineType.Single && formatInfo.Length > 0)) {
                items.Add(new MessageFormatDataItem {
                    Type = formatInfo.Type,
                    Offset = formatInfo.Offset,
                    Length = formatInfo.Length
                });
                formatInfo.Length = 0;
            }
        }

        private static void CheckTextFormatLink(int offset, string link, ref FormatInfo formatInfo, List<MessageFormatDataItem> items, bool last) {
            if (!string.IsNullOrEmpty(link)) {
                if (formatInfo.Length > 0) {
                    formatInfo.Length++;
                } else {
                    formatInfo.Offset = offset;
                    formatInfo.Length = 1;
                    formatInfo.Url = link.Trim('"');
                }
            } else if (last || (link != formatInfo.Url && formatInfo.Length > 0)) {
                items.Add(new MessageFormatDataItem {
                    Type = formatInfo.Type,
                    Offset = formatInfo.Offset,
                    Length = formatInfo.Length,
                    Url = formatInfo.Url,
                });
                formatInfo.Length = 0;
                formatInfo.Url = null;
            }
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
