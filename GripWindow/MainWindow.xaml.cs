using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace GripWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private POINT? _prevPoint;
        private IntPtr? _movableWindow;

        //Rect取得用
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        //座標取得用
        private struct POINT
        {
            public int x;
            public int y;
        }

        //ウィンドウのRect取得
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        //指定座標にあるウィンドウのハンドル取得
        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(POINT pOINT);

        //マウスカーソルの位置取得
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        // ウィンドウの移動
        // https://dobon.net/vb/dotnet/process/movewindow.html
        [DllImport("user32.dll")]
        private static extern int MoveWindow(IntPtr hwnd, int x, int y,
            int nWidth, int nHeight, int bRepaint);

        DispatcherTimer MyTimer;

        public MainWindow()
        {
            InitializeComponent();

            //1秒毎のタイマー
            MyTimer = new DispatcherTimer();
            MyTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            MyTimer.Tick += MyTimer_Tick;
            MyTimer.Start();
        }

        private void MyTimer_Tick(object sender, EventArgs e)
        {
            var isDownAlt = Keyboard.IsKeyDown(Key.LeftAlt) ||
                            Keyboard.IsKeyDown(Key.RightAlt);
            //TODO: MoveWindowからAlt押しっぱなしでResizeWindowに移ったときの
            if (isDownAlt)
            {
                OnDownAlt();
            }
            else
            {
                OnUpAlt();
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
            _movableWindow = null;
            _prevPoint = null;
        }

        private void MoveWindow(POINT targetPoint)
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