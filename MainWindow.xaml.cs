using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;

//using Windows.UI.WindowManagement;
using WinRT.Interop;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinFloat
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private IntPtr hWnd;
        private WindowId windowId;
        private AppWindow appWindow;

        public MainWindow()
        {
            this.InitializeComponent();

            appWindow = GetAppWindowForCurrentWindow();

            appWindow.Resize(new Windows.Graphics.SizeInt32(300, 360));
            appWindow.SetPresenter(AppWindowPresenterKind.Overlapped);

            if (appWindow.Presenter is OverlappedPresenter overlappedPresenter)
            {
                overlappedPresenter.SetBorderAndTitleBar(false, false);
                overlappedPresenter.IsMaximizable = false;
                overlappedPresenter.IsResizable = false;
            }

            AppTitleBar.Loaded += AppTitleBar_Loaded;
            AppTitleBar.SizeChanged += AppTitleBar_SizeChanged;

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
        }


        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(wndId);
        }


        private void AppTitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            if (!ExtendsContentIntoTitleBar) return;
            SetTitleBarInteractiveRegions();
        }

        private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!ExtendsContentIntoTitleBar) return;
            SetTitleBarInteractiveRegions();
        }

        public Rect GetElementBounds(FrameworkElement element)
        {
            var transform = element.TransformToVisual(null); // null = window coordinates
            var point = transform.TransformPoint(new Point(0, 0));

            return new Rect(point.X, point.Y, element.ActualWidth, element.ActualHeight);
        }

        private void SetTitleBarInteractiveRegions()
        {
            var settingsBtnRect = GetElementBounds(AppSettingsButton);
            var pinBtnRect = GetElementBounds(AppPinButton);
            var minimizeBtnRect = GetElementBounds(AppMinimizeButton);
            var closeBtnRect = GetElementBounds(AppCloseButton);

            var rectArray = new Windows.Graphics.RectInt32[] { 
                new RectInt32((int)settingsBtnRect.X, (int)settingsBtnRect.Y, (int)settingsBtnRect.Width, (int)settingsBtnRect.Height ) ,
                new RectInt32((int)pinBtnRect.X, (int)pinBtnRect.Y, (int)pinBtnRect.Width, (int)pinBtnRect.Height ) ,
                new RectInt32((int)minimizeBtnRect.X, (int)minimizeBtnRect.Y, (int)minimizeBtnRect.Width, (int)minimizeBtnRect.Height ) ,
                new RectInt32((int)closeBtnRect.X, (int)closeBtnRect.Y, (int)closeBtnRect.Width, (int)closeBtnRect.Height ) 
            };

            InputNonClientPointerSource nonClientInputSrc =
                InputNonClientPointerSource.GetForWindowId(this.AppWindow.Id);
            nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, rectArray);
        }



        private void AppSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            

        }

        private void AppPinButton_Click(object sender, RoutedEventArgs e)
        {
            if (appWindow.Presenter is OverlappedPresenter presenter)
            {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              
                presenter.Minimize();
            }
        }


        private void AppMinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            appWindow.Hide();
            Task.Delay(3000).ContinueWith(t => appWindow.Show());
        }

        private void AppCloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
