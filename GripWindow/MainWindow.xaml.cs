using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace GripWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point? _prevPoint;
        private IntPtr? _movableWindow;

        //Rect取得用
        private struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        //座標取得用
        private struct Point
        {
            public int x;
            public int y;
        }

        //ウィンドウのRect取得
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

        //指定座標にあるウィンドウのハンドル取得
        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(Point point);

        //マウスカーソルの位置取得
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out Point lpPoint);

        // ウィンドウの移動
        // https://dobon.net/vb/dotnet/process/movewindow.html
        [DllImport("user32.dll")]
        private static extern int MoveWindow(IntPtr hwnd, int x, int y,
            int nWidth, int nHeight, int bRepaint);

        // ウィンドウの最大化
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        private const int SW_MAXIMIZE = 3;

        private readonly DispatcherTimer _myTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            //1秒毎のタイマー
            _myTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            _myTimer.Tick += MyTimer_Tick;
            _myTimer.Start();
        }

        private void MyTimer_Tick(object? sender, EventArgs e)
        {
            var isDownAlt = Keyboard.IsKeyDown(Key.LeftAlt) ||
                            Keyboard.IsKeyDown(Key.RightAlt);
            var isUpAlt = Keyboard.IsKeyUp(Key.LeftAlt) ||
                          Keyboard.IsKeyUp(Key.Right);
            var isDownShift = Keyboard.IsKeyDown(Key.LeftShift) ||
                              Keyboard.IsKeyDown(Key.RightShift);
            var isDownAltShift = isDownAlt && isDownShift;

            if (isDownAltShift)
            {
                OnDownAltShift();
            }
            else if (isDownAlt)
            {
                OnDownAlt();
            }
            else if (isUpAlt)
            {
                OnUpAlt();
            }
            else
            {
                OnKeyUp();
            }
        }

        private void OnDownAlt()
        {
            GetCursorPos(out var cursorP);
            MoveWindow(cursorP);
            _prevPoint = cursorP;
        }

        private void OnUpAlt()
        {
            AeroSnap();
        }

        private void OnKeyUp()
        {
            _movableWindow = null;
            _prevPoint = null;
        }

        private void OnDownAltShift()
        {
            GetCursorPos(out var cursorP);
            ResizeWindow(cursorP);
            _prevPoint = cursorP;
        }

        private void ResizeWindow(Point targetPoint)
        {
            _movableWindow ??= GetWindow();
            _prevPoint ??= targetPoint;

            GetWindowRect((IntPtr) _movableWindow, out var wRect);
            var xDiff = targetPoint.x - _prevPoint.Value.x;
            var yDiff = targetPoint.y - _prevPoint.Value.y;
            var width = wRect.right - wRect.left + xDiff;
            var height = wRect.bottom - wRect.top + yDiff;
            MoveWindow((IntPtr) _movableWindow, wRect.left, wRect.top, width, height, 1);
        }

        private void MoveWindow(Point targetPoint)
        {
            _movableWindow ??= GetWindow();
            _prevPoint ??= targetPoint;

            GetWindowRect((IntPtr) _movableWindow, out var wRect);
            var xDiff = targetPoint.x - _prevPoint.Value.x;
            var yDiff = targetPoint.y - _prevPoint.Value.y;

            var windowLeft = wRect.left + xDiff;
            var windowTop = wRect.top + yDiff;
            var width = wRect.right - wRect.left;
            var height = wRect.bottom - wRect.top;
            MoveWindow((IntPtr) _movableWindow, windowLeft, windowTop, width, height, 1);
        }

        private void AeroSnap()
        {
            var window = GetWindow();
            _ = HitDisplayEdge(window) switch
            {
                Edge.Top => MaximizeWindow(window),
                Edge.Left => LeftMaximizeWindow(window),
                Edge.Right => RightMaximizeWindow(window),
                Edge.Bottom => MaximizeWindow(window),
                _ => false,
            };
        }

        public enum Edge
        {
            Top,
            Left,
            Bottom,
            Right,
            None,
        }

        private Edge HitDisplayEdge(IntPtr window)
        {
            GetWindowRect(window, out var rect);
            if (rect.top <= 0) return Edge.Top;
            if (rect.bottom <= 0) return Edge.Bottom;
            if (rect.right <= 0) return Edge.Right;
            if (rect.left <= 0) return Edge.Left;
            return Edge.None;
        }

        private bool MaximizeWindow(IntPtr window)
        {
            return ShowWindow(window, SW_MAXIMIZE);
        }

        private bool LeftMaximizeWindow(IntPtr window)
        {
            var width = (int) SystemParameters.PrimaryScreenWidth / 2;
            var height = (int) SystemParameters.PrimaryScreenHeight;
            MoveWindow(window, 0, 0, width, height, 0);

            return true;
        }

        private bool RightMaximizeWindow(IntPtr window)
        {
            var width = (int) SystemParameters.PrimaryScreenWidth;
            var height = (int) SystemParameters.PrimaryScreenHeight / 2;
            MoveWindow(window, 0, 0, width, height, 0);
            return true;
        }

        private IntPtr GetWindow()
        {
            //マウスカーソルの位置取得
            GetCursorPos(out var cursorP);
            //マウスカーソルの下にあるウィンドウのハンドル取得
            //右クリックメニューやボタン、テキストボックスとかのコントロールも取得される
            var window = WindowFromPoint(cursorP);

            if (GetWindowRect(window, out var wRect))
            {
                MyTextBlock1.Text = $"WindowRect 左上座標({wRect.left}, {wRect.top}) 右下({wRect.right}, {wRect.bottom})";
                MyTextBlock2.Text = $"Mouse x:{cursorP.x} y:{cursorP.y}";
            }

            return window;
        }
    }
}