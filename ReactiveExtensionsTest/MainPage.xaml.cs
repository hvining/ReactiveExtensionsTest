using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ReactiveExtensionsTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        IDisposable eventBlah;
        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = new MainViewModel();
            CoreApplication.Properties.Add("Dispatcher", this.Dispatcher);
            //var window = Window.Current.CoreWindow;

            //var te = from tap in Observable.FromEventPattern<TappedRoutedEventArgs>(this, "Tapped")
            //         select tap.EventArgs.GetPosition(this);

            //var thirdQuadPts = from pos in te
            //                   where pos.X < (window.Bounds.Width / 2) && pos.Y > (window.Bounds.Height / 2)
            //                   group pos by pos.X;

            //eventBlah = thirdQuadPts.Subscribe(pt =>
            //   {
            //       Ellipse ellipse = new Ellipse();
            //       ellipse.Fill = new SolidColorBrush(Colors.Blue);
            //       ellipse.Width = 10;
            //       ellipse.Height = ellipse.Width;

            //       canvas.Children.Add(ellipse);
            //       Canvas.SetLeft(ellipse, pt.FirstOrDefault().X);
            //       Canvas.SetTop(ellipse, pt.FirstOrDefault().Y);
            //   });

            //var secondQuadPts = from pos in te.Buffer(TimeSpan.FromSeconds(1))
            //                   select pos.Count;

            //secondQuadPts.Subscribe(async (pt) =>
            //{
            //    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            //        {
            //            txtBox.Text = pt.ToString();
            //        });
            //});
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            eventBlah.Dispose();
        }
    }
}
