
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Reflection;
using System.Text;


using Application = System.Windows.Application;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;


namespace WinFloat
{
    public partial class MainWindow : Window
    {
        

        (double X1, double X2, double XX1, double XX2) crsh_x_point;
        (double Y1, double Y2, double YY1, double YY2) crsh_y_point;

        private NotifyIcon systemTrayIcon;

        private bool IsSelectingWindow = false;
        private bool IsSettingsActive = false;

        private Win32Window W32Window;


        /// <summary>
        /// Interaction logic for MainWindow.xaml
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            InitializeSystemTrayIcon();

            AppConfig.Initialize(PinForegroundWindow, UnPinForegroundWindow);
            if (!AppConfig.LoadFromStorage()) AppConfig.ResetToDefault();
        }


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            ResolveBtnPinState();
            ResolveSettingsState();

            var window = new WindowInteropHelper(this);
            W32Window = new Win32Window(window.Handle);

            if (AppConfig.PinShortcut != null) W32Window.AddShortcut(AppConfig.PinShortcut);
            if (AppConfig.UnPinShortcut != null) W32Window.AddShortcut(AppConfig.UnPinShortcut);

            W32Window.AddShortcut(new Shortcut(0, Key.Escape, CancelWindowSelection));

            W32Window.RegisterWakeUpMsg(App.WakeUpMsg, RestoreWindow);

            Win32Cursor.Initialize();
            Win32Cursor.AddOnMouseUpHandler(CompleteWindowSelection);
        }


        // +++++++++++++++++++++++++ Events +++++++++++++++++++++++++
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            IsSettingsActive = btnSettings.IsChecked ?? false;
            ResolveSettingsState();
        }

        private void BtnPin_Click(object sender, RoutedEventArgs e)
        {
            Topmost = !Topmost;
            ResolveBtnPinState();
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            if (AppConfig.MoveToTrayOnExit)
            {
                systemTrayIcon.Visible = true;
                Hide();
                return;
            }

            ExitApplication();
        }

        private void Crosshair_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsSelectingWindow = true;

            crsh_y_point = (crshr_y1.Y1, crshr_y1.Y2, crshr_y2.Y1, crshr_y2.Y2);
            crsh_x_point = (crshr_x1.X1, crshr_x1.X2, crshr_x2.X1, crshr_x2.X2);

            crshr_y1.Y1 = crshr_y1.Y1 + 10;
            crshr_y1.Y2 = crshr_y1.Y2 + 10;

            crshr_y2.Y1 = crshr_y2.Y1 - 10;
            crshr_y2.Y2 = crshr_y2.Y2 - 10;

            crshr_x1.X1 = crshr_x1.X1 + 10;
            crshr_x1.X2 = crshr_x1.X2 + 10;

            crshr_x2.X1 = crshr_x2.X1 - 10;
            crshr_x2.X2 = crshr_x2.X2 - 10;

            Win32Cursor.SetGlobalCrossCursor();
        }

        private void ShortcutTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            if (sender is not System.Windows.Controls.TextBox txtBox) return;

            Key key = (e.Key == Key.System)? e.SystemKey : e.Key;           // why Alt key

            if (Shortcut.AllowedModifierKeys.Contains(key)) return;

            ModifierKeys modifiers = Keyboard.Modifiers;
            bool winDown = Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin);

            if (modifiers == ModifierKeys.None && !winDown) return;

            StringBuilder shortcutText = new StringBuilder();

            if (winDown) shortcutText.Append("Win + ");
            if (modifiers.HasFlag(ModifierKeys.Control)) shortcutText.Append("Ctrl + ");
            if (modifiers.HasFlag(ModifierKeys.Shift)) shortcutText.Append("Shift + ");
            if (modifiers.HasFlag(ModifierKeys.Alt)) shortcutText.Append("Alt + ");

            shortcutText.Append(key.ToString());

            txtBox.Text = shortcutText.ToString();
        }


        private void ShortcutTextBox_GotFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            W32Window.DisableShortcuts = true;
            if (sender is not System.Windows.Controls.TextBox txtBox) return;

            if(txtBox == txtBoxPin)
            {
                if(AppConfig.PinShortcut != null) 
                    W32Window.RemoveShortcut(AppConfig.PinShortcut);
            }
            else if (txtBox == txtBoxUnPin)
            {
                if (AppConfig.UnPinShortcut != null)
                    W32Window.RemoveShortcut(AppConfig.UnPinShortcut);
            }
        }


        private void ShortcutTextBox_LostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            W32Window.DisableShortcuts = false;
            if (sender is not System.Windows.Controls.TextBox txtBox) return;

            if (txtBox == txtBoxPin)
            {
                if (AppConfig.PinShortcut != null)
                {
                    AppConfig.PinShortcut = new Shortcut(txtBoxPin.Text, PinForegroundWindow);
                    W32Window.AddShortcut(AppConfig.PinShortcut);
                }
            }
            else if (txtBox == txtBoxUnPin)
            {
                if (AppConfig.UnPinShortcut != null)
                {
                    AppConfig.UnPinShortcut = new Shortcut(txtBoxUnPin.Text, UnPinForegroundWindow); ;
                    W32Window.AddShortcut(AppConfig.UnPinShortcut);
                }
            }
        }

        private void SettingsResetClick(object sender, RoutedEventArgs e)
        {
            AppConfig.ResetToDefault();
            ResolveSettingsState();
            AppConfig.SaveToStorage();
        }

        private void SettingsSaveClick(object sender, RoutedEventArgs e)
        {
            AppConfig.MoveToTrayOnExit = chkBoxMinimizeToTray.IsChecked ?? false;
            AppConfig.CanSelectAppWindow = chkBoxWindowSelect.IsChecked ?? false;

            AppConfig.SaveToStorage();
        }
        // ----------------------- End Events -----------------------


        private void CancelWindowSelection()
        {
            if (!IsSelectingWindow) return;
            IsSelectingWindow = false;

            (crshr_y1.Y1, crshr_y1.Y2, crshr_y2.Y1, crshr_y2.Y2) = crsh_y_point;
            (crshr_x1.X1, crshr_x1.X2, crshr_x2.X1, crshr_x2.X2) = crsh_x_point;

            Win32Cursor.RestoreGlobalCursor();
        }


        private void CompleteWindowSelection()
        {
            if (!IsSelectingWindow) return;

            Application.Current.Dispatcher.BeginInvoke(() =>
            {
                CancelWindowSelection();

                var helper = new WindowInteropHelper(this);
                IntPtr w32Window = Win32Window.GetWindowUnderCursor();
                if (w32Window != helper.Handle || AppConfig.CanSelectAppWindow)
                    Win32Window.MakeWindowTopMost(w32Window);
            });
        }


        private void ResolveSettingsState()
        {
            SettingsPanel.Visibility = IsSettingsActive? Visibility.Visible : Visibility.Hidden;
            MainPanel.Visibility = IsSettingsActive? Visibility.Hidden : Visibility.Visible;

            txtBoxPin.Text = AppConfig.PinShortcut?.ToString() ?? "Not Set";
            txtBoxUnPin.Text = AppConfig.UnPinShortcut?.ToString() ?? "Not Set";
            chkBoxMinimizeToTray.IsChecked = AppConfig.MoveToTrayOnExit;
            chkBoxWindowSelect.IsChecked = AppConfig.CanSelectAppWindow;
        }


        private void ResolveBtnPinState()
        {
            btnPin.Tag = Topmost ? "-45" : "0";
            btnPin.IsChecked = Topmost ? true : false;
        }

        private void PinForegroundWindow()
        {
            IntPtr foreWindow = Win32Window.GetActiveWindow();
            IntPtr winHandle = new WindowInteropHelper(this).Handle;

            if (foreWindow != winHandle)
            {
                Win32Window.MakeWindowTopMost(foreWindow);
                return;
            }

            Topmost = true;
            ResolveBtnPinState();
        }

        private void UnPinForegroundWindow()
        {
            IntPtr foreWindow = Win32Window.GetActiveWindow();
            IntPtr winHandle = new WindowInteropHelper(this).Handle;

            if (foreWindow != winHandle)
            {
                Win32Window.MakeWindowNonTopMost(foreWindow);
                return;
            }

            Topmost = false;
            ResolveBtnPinState();
        }

        private void InitializeSystemTrayIcon()
        {
            systemTrayIcon = new NotifyIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location),
                Text = Assembly.GetExecutingAssembly().GetName().Name,
                Visible = false
            };

            var ctxMenu = new ContextMenuStrip();

            ctxMenu.ShowImageMargin = false;
            ctxMenu.ShowCheckMargin = false;

            ctxMenu.BackColor = System.Drawing.Color.FromArgb(45, 45, 48);
            ctxMenu.ForeColor = System.Drawing.Color.White;

            ctxMenu.Items.Add("Restore", null, (s, e) => RestoreWindow());
            ctxMenu.Items.Add("Quit", null, (s, e) => ExitApplication());

            systemTrayIcon.ContextMenuStrip = ctxMenu;

            systemTrayIcon.Click += (s, e) => { 
                if (((MouseEventArgs)e).Button == MouseButtons.Left) 
                    RestoreWindow();     
            };
        }

        private void RestoreWindow()
        {
            Show();
            Activate();
            systemTrayIcon.Visible = false;
        }

        private void ExitApplication()
        {
            systemTrayIcon.Visible = false;
            systemTrayIcon.Dispose();
            Close();
            Application.Current.Shutdown();
        }
    }
}