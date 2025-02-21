using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RichEditBoxTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            reb.Document.SetText(TextSetOptions.None, "The quick brown fox.");
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            reb.Document.GetText(TextGetOptions.None, out string value);

            value = value.TrimEnd('\v', '\r');

            for (int i = 0; i < value.Length; i++) {
                var range = reb.Document.GetRange(i, i + 1);
                var spaceAfter = range.ParagraphFormat.SpaceAfter;
                var text = range.Text;

                ITextCharacterFormat cf = range.CharacterFormat;
                var bold = cf.Bold == FormatEffect.On;
                var italic = cf.Italic == FormatEffect.On;
                var underline = cf.Underline == UnderlineType.Single;
                var fg = cf.ForegroundColor;

                var link = range.Link;

                int j = 0;
            }
        }

        private void Button2_Click(object sender, RoutedEventArgs e) {
            reb.Document.BatchDisplayUpdates();

            reb.Document.SetText(TextSetOptions.None, "And link!");
            var range = reb.Document.GetRange(4, 8);
            range.CharacterFormat = reb.Document.GetDefaultCharacterFormat();
            range.CharacterFormat.BackgroundColor = Color.FromArgb(48, 0, 122, 204);
            range.CharacterFormat.Underline = UnderlineType.None;
            var fc = range.CharacterFormat.ForegroundColor;
            range.Link = "\"http://www.msn.com\"";

            reb.Document.Selection.SetRange(range.EndPosition, range.EndPosition);
            reb.Document.ApplyDisplayUpdates();
        }
    }
}
