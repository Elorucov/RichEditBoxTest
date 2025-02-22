using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Display.Core;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
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

            AddHandler(RichEditBox.PointerPressedEvent, new PointerEventHandler(OnPointerPressed), true);
            AddHandler(RichEditBox.PointerReleasedEvent, new PointerEventHandler(OnPointerReleased), true);
            AddHandler(RichEditBox.ManipulationStartedEvent, new ManipulationStartedEventHandler(OnManipulationStarted), true);
            AddHandler(RichEditBox.ManipulationCompletedEvent, new ManipulationCompletedEventHandler(OnManipulationCompleted), true);
            AddHandler(RichEditBox.PointerCaptureLostEvent, new PointerEventHandler(OnPointerCaptureLost), true);

            CoreApplication.GetCurrentView().CoreWindow.PointerPressed += CoreWindow_PointerPressed;
            CoreApplication.GetCurrentView().CoreWindow.PointerReleased += CoreWindow_PointerReleased;
        }

        StringBuilder _sb = new StringBuilder();

        private void Debug(string line) {
            _sb.Insert(0, $"\n");
            _sb.Insert(0, line);
            dbg.Text = _sb.ToString();
        }

        private void reb_SelectionChanged(object sender, RoutedEventArgs e) {
            var sel = reb.Document.Selection;
            Debug($"SelectionChanged {sel.StartPosition}, {sel.EndPosition}, {sel.Length}");
        }

        private void OnContextRequested(UIElement sender, ContextRequestedEventArgs args) {
            Debug($"ContextRequested");
        }

        private void OnHolding(object sender, HoldingRoutedEventArgs e) {
            Debug($"Holding {e.HoldingState}");
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e) {
            Debug($"ManipulationCompleted {e.PointerDeviceType}");
        }

        private void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e) {
            Debug($"ManipulationStarted {e.PointerDeviceType}");
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e) {
            Debug($"PointerPressed {e.Pointer.PointerDeviceType}");
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e) {
            Debug($"PointerReleased {e.Pointer.PointerDeviceType}");
        }

        private void OnTapped(object sender, TappedRoutedEventArgs e) {
            Debug($"Tapped {e.PointerDeviceType}");
        }

        private void OnPointerCaptureLost(object sender, PointerRoutedEventArgs e) {
            Debug($"PointerCaptureLost {e.Pointer.PointerDeviceType}");
        }

        private void Gr_ManipulationStarted(Windows.UI.Input.GestureRecognizer sender, Windows.UI.Input.ManipulationStartedEventArgs args) {
            Debug($"GR ManipulationStarted {args.PointerDeviceType}");
        }

        private void Gr_ManipulationCompleted(Windows.UI.Input.GestureRecognizer sender, Windows.UI.Input.ManipulationCompletedEventArgs args) {
            Debug($"GR ManipulationCompleted {args.PointerDeviceType}");
        }

        private void CoreWindow_PointerPressed(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs args) {
            Debug($"CW PointerPressed {args.CurrentPoint.PointerDevice.PointerDeviceType}");
        }

        private void CoreWindow_PointerReleased(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.PointerEventArgs args) {
            Debug($"CW PointerReleased {args.CurrentPoint.PointerDevice.PointerDeviceType}");
        }

        //private void Button_Click(object sender, RoutedEventArgs e) {
        //    reb.Document.GetText(TextGetOptions.AdjustCrlf, out string dump);
        //    var range = reb.Document.GetRange(0, dump.Length);

        //    reb2.Document.SetText(TextSetOptions.None, dump);
        //    var range2 = reb2.Document.GetRange(0, dump.Length);
        //    range2.FormattedText = range.FormattedText;
        //}

        private void ToFormatData(object sender, RoutedEventArgs e) {
            var formatData = TextFormatConverter.ToVKFormat(reb.Document);
            fd.Text = JsonConvert.SerializeObject(formatData, Formatting.Indented, new JsonSerializerSettings { 
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        private void FromFormatData(object sender, RoutedEventArgs e) {
            new Action(async () => {
                try {
                    var formatData = JsonConvert.DeserializeObject<MessageFormatData>(fd.Text);
                    TextFormatConverter.FromVKFormat(reb.Document, formatData);
                } catch (Exception ex) {
                    await new MessageDialog(ex.Message, "Error").ShowAsync();
                }
            })();
        }
    }
}
