using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics;
using WinRT.Interop;


namespace WinFloat
{
    public sealed partial class MainWindow : Window
    {
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

            AppCrossHairButton.AddHandler(UIElement.PointerPressedEvent,new PointerEventHandler(AppCrossHairButton_PointerPressed), handledEventsToo: true);
            AppCrossHairButton.AddHandler(UIElement.PointerReleasedEvent,new PointerEventHandler(AppCrossHairButton_PointerReleased), handledEventsToo: true);

            Win32MsgHook.RegisterWin32Hook();


            RegisterShortcuts();
        }


        private void RegisterShortcuts()
        {
            // stop mouse drag operation
            KBShortcut escape = new KBShortcut(false, false, false,false, Windows.System.VirtualKey.Escape, Shortcut_CancelWindowSelection);

            // pin currenlty active window
            KBShortcut pin = new KBShortcut(false, true, false, true, Windows.System.VirtualKey.P, Shortcut_PinForegroundWindow);   
            
            // unpin currenlty pinned window
            KBShortcut unpin = new KBShortcut(false, true, false, true, Windows.System.VirtualKey.U, Shortcut_UnPinForegroundWindow);

            Win32MsgHook.AddShortcut(escape);
            Win32MsgHook.AddShortcut(pin);
            Win32MsgHook.AddShortcut(unpin);
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
                presenter.Minimize();
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


        private void AppCrossHairButton_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (Win32Cursor.isCrossHairActive) return;
            Win32Cursor.SetGlobalCrossCursor();
        }

        private void AppCrossHairButton_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!Win32Cursor.isCrossHairActive) return;
            Win32Cursor.RestoreGlobalCursor();

            IntPtr hwnd = Win32Window.GetWindowUnderCursor();
            Win32Window.MakeWindowTopMost(hwnd);
        }


        private void Shortcut_PinForegroundWindow()
        {
            IntPtr hwnd = Win32Window.GetActiveWindow();
            Win32Window.MakeWindowTopMost(hwnd);

        }

        private void Shortcut_UnPinForegroundWindow()
        {
            IntPtr hwnd = Win32Window.GetActiveWindow();
            Win32Window.MakeWindowNonTopMost(hwnd);
        }

        private void Shortcut_CancelWindowSelection()
        {
            if (!Win32Cursor.isCrossHairActive) return;
            Win32Cursor.RestoreGlobalCursor();
        }

    }
}

