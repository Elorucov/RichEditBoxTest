using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace RichEditBoxTest {
    public class TestRichEditBox : RichEditBox {
        public TestRichEditBox() {
            Loaded += VKRichEditBox_Loaded;
            Unloaded += VKRichEditBox_Unloaded;
        }

        Microsoft.UI.Xaml.Controls.CommandBarFlyout _flyout = new Microsoft.UI.Xaml.Controls.CommandBarFlyout();
        ITextSelection _textSelection;
        PointerEventHandler pointerEventHandler;

        AppBarToggleButton _boldABTB;
        AppBarToggleButton _italicABTB;
        AppBarToggleButton _underlineABTB;
        AppBarToggleButton _linkABTB;
        AppBarButton _undoABB;
        AppBarButton _redoABB;
        AppBarButton _cutABB;
        AppBarButton _copyABB;
        AppBarButton _pasteABB;
        AppBarButton _selectAllABB;

        private AppBarButton CreateABB(string glyph, string label, string accessKey = null) {
            AppBarButton abb = new AppBarButton {
                Icon = new FontIcon { Glyph = glyph },
                Label = label,
                AccessKey = accessKey
            };
            string tt = string.IsNullOrEmpty(accessKey) ? abb.Label : $"{abb.Label} (Ctrl + {accessKey})";
            ToolTipService.SetToolTip(abb, tt);
            return abb;
        }

        private AppBarToggleButton CreateABTB(string glyph, string label, string accessKey = null) {
            AppBarToggleButton abtb = new AppBarToggleButton {
                Icon = new FontIcon { Glyph = glyph },
                Label = label,
                AccessKey = accessKey
            };
            string tt = string.IsNullOrEmpty(accessKey) ? abtb.Label : $"{abtb.Label} (Ctrl + {accessKey})";
            ToolTipService.SetToolTip(abtb, tt);
            return abtb;
        }

        private void VKRichEditBox_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            _boldABTB = CreateABTB("", "Bold", "B");
            _boldABTB.Checked += BoldTB_Checked;
            _boldABTB.Unchecked += BoldTB_Unchecked;

            _italicABTB = CreateABTB("", "Italic", "I");
            _italicABTB.Checked += ItalicTB_Checked;
            _italicABTB.Unchecked += ItalicTB_Unchecked;

            _underlineABTB = CreateABTB("", "Underline", "U");
            _underlineABTB.Checked += UnderlineTB_Checked;
            _underlineABTB.Unchecked += UnderlineTB_Unchecked;

            _linkABTB = CreateABTB("", "Link", "L");
            _linkABTB.Click += LinkTB_Click;

            _undoABB = CreateABB("", "Undo", "Z");
            _undoABB.Click += Undo_Click;

            _redoABB = CreateABB("", "Redo", "Y");
            _redoABB.Click += Redo_Click;

            _cutABB = CreateABB("", "Cut", "X");
            _cutABB.Click += Cut_Click;

            _copyABB = CreateABB("", "Copy", "C");
            _copyABB.Click += Copy_Click;

            _pasteABB = CreateABB("", "Paste", "V");
            _pasteABB.Click += Paste_Click;

            _selectAllABB = CreateABB("", "Select all", "A");
            _selectAllABB.Click += SelectAll_Click;

            Paste += VKRichEditBox_Paste;
            _flyout.Opening += Menu_Opening;

            ContextFlyout = _flyout;
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7)) {
                SelectionFlyout = _flyout;
            } else {
                ContextMenuOpening += OnContextMenuOpening;
                pointerEventHandler = new PointerEventHandler(OnPointerReleased);
                AddHandler(RichEditBox.PointerReleasedEvent, pointerEventHandler, true);
            }
        }

        private void VKRichEditBox_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            Loaded -= VKRichEditBox_Loaded;
            Unloaded -= VKRichEditBox_Unloaded;
            _flyout.Opening -= Menu_Opening;
            Paste -= VKRichEditBox_Paste;

            if (!ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7)) {
                ContextMenuOpening -= OnContextMenuOpening;
                RemoveHandler(RichEditBox.PointerReleasedEvent, pointerEventHandler);
            }

            _boldABTB.Checked -= BoldTB_Checked;
            _boldABTB.Unchecked -= BoldTB_Unchecked;

            _italicABTB.Checked -= ItalicTB_Checked;
            _italicABTB.Unchecked -= ItalicTB_Unchecked;

            _underlineABTB.Checked -= UnderlineTB_Checked;
            _underlineABTB.Unchecked -= UnderlineTB_Unchecked;

            _linkABTB.Click -= LinkTB_Click;
            _undoABB.Click -= Undo_Click;
            _redoABB.Click -= Redo_Click;
            _cutABB.Click -= Cut_Click;
            _copyABB.Click -= Copy_Click;
            _pasteABB.Click -= Paste_Click;
            _selectAllABB.Click -= SelectAll_Click;
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e) {
            if (Document.Selection.Length > 0) {
                SetupTextFlyout(_flyout, false);
                _flyout.Placement = Windows.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Top;

                _flyout.Opening -= Menu_Opening;
                _flyout.ShowAt(this);
                _flyout.Opening += Menu_Opening;
            }
        }

        private void Menu_Opening(object sender, object e) {
            SetupTextFlyout(sender as Microsoft.UI.Xaml.Controls.CommandBarFlyout, true);
        }

        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e) {
            SetupTextFlyout(_flyout, true);
            e.Handled = true; // to avoid displaying system popup on old versions of Win10.
        }

        private void SetupTextFlyout(Microsoft.UI.Xaml.Controls.CommandBarFlyout flyout, bool includeExtra = false) {
            flyout.PrimaryCommands.Clear();
            flyout.SecondaryCommands.Clear();

            // Text formatting
            _textSelection = Document.Selection;
            if (_textSelection.Length > 0) {
                var format = _textSelection.CharacterFormat;

                _boldABTB.IsChecked = format.Bold == FormatEffect.On;
                flyout.PrimaryCommands.Add(_boldABTB);

                _italicABTB.IsChecked = format.Italic == FormatEffect.On;
                flyout.PrimaryCommands.Add(_italicABTB);

                _underlineABTB.IsChecked = format.Underline == UnderlineType.Single;
                flyout.PrimaryCommands.Add(_underlineABTB);

                _linkABTB.IsChecked = !string.IsNullOrEmpty(_textSelection.Link);
                flyout.PrimaryCommands.Add(_linkABTB);
            }

            // EditBox actions
            if (!includeExtra) return;
            if (Document.CanUndo()) flyout.SecondaryCommands.Add(_undoABB);
            if (Document.CanRedo()) flyout.SecondaryCommands.Add(_redoABB);
            if (_textSelection.Length > 0 && Document.CanCopy()) {
                flyout.SecondaryCommands.Add(_cutABB);
                flyout.SecondaryCommands.Add(_copyABB);
            }

            var clipboard = Clipboard.GetContent();
            if (clipboard.Contains(StandardDataFormats.Text) && Document.CanPaste()) flyout.SecondaryCommands.Add(_pasteABB);

            Document.GetText(TextGetOptions.None, out string text);
            if (text.Length > 0) flyout.SecondaryCommands.Add(_selectAllABB);
        }

        private void BoldTB_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            Document.Selection.CharacterFormat.Bold = FormatEffect.On;
        }

        private void BoldTB_Unchecked(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            Document.Selection.CharacterFormat.Bold = FormatEffect.Off;
        }

        private void ItalicTB_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            Document.Selection.CharacterFormat.Italic = FormatEffect.On;
        }

        private void ItalicTB_Unchecked(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            Document.Selection.CharacterFormat.Italic = FormatEffect.Off;
        }

        private void UnderlineTB_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            Document.Selection.CharacterFormat.Underline = UnderlineType.Single;
        }

        private void UnderlineTB_Unchecked(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            Document.Selection.CharacterFormat.Underline = UnderlineType.None;
        }

        private void LinkTB_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            var target = _flyout.Target;

            TextBox tb = new TextBox {
                Header = "Enter link:"
            };
            if (!string.IsNullOrEmpty(_textSelection.Link)) tb.Text = _textSelection.Link.Trim('"');

            Grid btns = new Grid {
                Margin = new Thickness(0, 12, 0, 0)
            };
            btns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star)});
            btns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12, GridUnitType.Pixel)});
            btns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star)});

            Button acceptBtn = new Button {
                Style = App.Current.Resources["AccentButtonStyle"] as Style,
                Content = "Add",
                IsEnabled = false,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            Button clearBtn = new Button {
                Content = "Clear",
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            Grid.SetColumn(clearBtn, 2);
            btns.Children.Add(acceptBtn);
            btns.Children.Add(clearBtn);

            StackPanel sp = new StackPanel {
                Width = 280
            };
            sp.Children.Add(tb);
            sp.Children.Add(btns);

            Flyout flyout = new Flyout {
                Content = sp
            };

            tb.TextChanging += (a, b) => {
                acceptBtn.IsEnabled = Uri.IsWellFormedUriString(a.Text, UriKind.Absolute);
            };
            acceptBtn.Click += (a, b) => {
                if (!Uri.IsWellFormedUriString(tb.Text, UriKind.Absolute)) return;
                var range = Document.Selection;
                range.CharacterFormat = Document.GetDefaultCharacterFormat();
                range.CharacterFormat.BackgroundColor = Color.FromArgb(32, 128, 128, 128);
                range.CharacterFormat.Underline = UnderlineType.None;
                range.Link = $"\"{tb.Text}\"";
                flyout.Hide();
            };
            clearBtn.Click += (a, b) => {
                Document.Selection.Link = "\"\"";
                flyout.Hide();
            };

            _flyout.Hide();
            flyout.ShowAt(target);
        }

        private void Undo_Click(object sender, RoutedEventArgs e) {
            if (Document.CanUndo()) Document.Undo();
        }

        private void Redo_Click(object sender, RoutedEventArgs e) {
            if (Document.CanRedo()) Document.Redo();
        }

        private void Cut_Click(object sender, RoutedEventArgs e) {
            if (Document.CanCopy()) Document.Selection.Cut();
        }

        private void Copy_Click(object sender, RoutedEventArgs e) {
            if (Document.CanCopy()) Document.Selection.Copy();
        }

        private void Paste_Click(object sender, RoutedEventArgs e) {
            Action act = new Action(async () => await TryPasteAsync());
            act();
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e) {
            Document.GetText(TextGetOptions.None, out string text);
            Document.Selection.SetRange(0, text.Length);
        }

        private void VKRichEditBox_Paste(object sender, TextControlPasteEventArgs e) {
            Action act = new Action(async () => await TryPasteAsync(e));
            act();
        }

        // https://learn.microsoft.com/en-us/windows/win32/dataxchg/standard-clipboard-formats
        private async Task TryPasteAsync(TextControlPasteEventArgs e = null) {
            try {
                var package = Clipboard.GetContent();

                // If the user tries to paste RTF content from any TOM control (Visual Studio, Word, Wordpad, browsers)
                // we have to handle the pasting operation manually to allow plaintext only.
                if (package.AvailableFormats.Contains(StandardDataFormats.Text) /*&& package.Contains("Rich Text Format")*/) {
                    e.Handled = true;

                    var text = await package.GetTextAsync();
                    var start = Document.Selection.StartPosition;
                    var length = Math.Abs(Document.Selection.Length);

                    // TODO also insert link.
                    Document.Selection.SetText(TextSetOptions.Unhide, text);
                    Document.Selection.SetRange(start + text.Length, start + text.Length);
                }
            } catch { }
        }
    }
}
