using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;
using System.Timers;
using System.Windows.Interop;

namespace Lumberjack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);
        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData,
   UIntPtr dwExtraInfo);
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };


        const int VK_UP = 0x26; //up key
        const int VK_DOWN = 0x28;  //down key
        const int VK_LEFT = 0x25;
        const int VK_RIGHT = 0x27;
        const uint KEYEVENTF_KEYUP = 0x0002;
        const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

        private IntPtr _windowHandle;
        private HwndSource _source;
        private const int HOTKEY_ID = 9000;
        private const uint MOD_NONE = 0x0000; //(none)
        private const uint MOD_ALT = 0x0001; //ALT
        private const uint MOD_CONTROL = 0x0002; //CTRL
        private const uint MOD_SHIFT = 0x0004; //SHIFT
        private const uint MOD_WIN = 0x0008; //WINDOWS
        private const uint VK_CAPITAL = 0x14;


        private static int[] xcoord = { 900, 1020 };
        private static int[] ycoord = { 675, 630, 585, 540, 495, 450, 405, 360, 315, 270, 225 };
        private static int preYCoord = 720;
        private Color[,] pixels = new Color[2, ycoord.Length];
        private bool[] left = new bool[ycoord.Length - 2];
        private bool ready = false;
        private Color tree = Color.FromRgb(161, 116, 56);
        private Color button = Color.FromRgb(161, 116, 56);
        private int xyz = 0;
        private bool executing = false;
        private bool currLeft = true;
        public MainWindow()
        {
            InitializeComponent();
            var myTimer = new System.Timers.Timer();
            myTimer.Elapsed += new ElapsedEventHandler(MyEvent);
            myTimer.Interval = 50;
            myTimer.Enabled = true;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_CONTROL, VK_CAPITAL); //CTRL + CAPS_LOCK
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            int vkey = (((int)lParam >> 16) & 0xFFFF);
                            if (vkey == VK_CAPITAL)
                            {
                                toggle();
                            }
                            handled = true;
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            base.OnClosed(e);
        }
        public static Point GetMousePosition()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);

            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        private void MyEvent(object source, ElapsedEventArgs e)
        {
            //Point mousePos = GetMousePosition();
            //Trace.WriteLine(mousePos + " " + tree.Equals(GetPixelColor((int)mousePos.X, (int)mousePos.Y)));
            if (ready && !executing && GetPixelColor(900, 940).Equals(button))
            {
                //ready && GetPixelColor(900, 940).Equals(button)
                //Trace.WriteLine(GetPixelColor(900, 940));

                DoAction();
            }
        }

        private void DoAction()
        {
            executing = true;
            tree = Color.FromRgb(161, 116, 56);
            for (int i = 0; i < pixels.GetLength(0); i++)
            {
                for (int j = 0; j < pixels.GetLength(1); j++)
                {
                    pixels[i, j] = GetPixelColor(xcoord[i], ycoord[j]);
                }
            }
            for (int i = 0; i < left.Length; i++)
            {
                if (i == 0)
                {
                    //(currLeft && GetPixelColor(xcoord[1], preYCoord) != tree)

                    if (pixels[0, 0] == tree || (!currLeft && GetPixelColor(xcoord[0], preYCoord) == tree))
                    {
                        left[i] = false;
                    }
                    /*if (pixels[0, 0] == tree)
                    {
                        left[i] = false;
                    }*/
                    else
                    {
                        left[i] = true;
                    }
                }
                else
                {
                    if (pixels[0, i] != tree && pixels[0, i - 1] != tree)
                    {
                        left[i] = true;
                    }
                    else
                    {
                        left[i] = false;
                    }
                }
            }
            for (int i = 0; i < left.Length; i++)
            {
                if (left[i])
                {
                    Trace.WriteLine("Left");
                    //keybd_event((byte)VK_LEFT, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
                    SendKeys.SendWait("{LEFT}");
                }
                else
                {
                    Trace.WriteLine("Right");
                    //keybd_event((byte)VK_RIGHT, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
                    SendKeys.SendWait("{RIGHT}");
                }

            }
            currLeft = left[left.Length - 1];
            Trace.WriteLine("end sequence" + xyz);
            xyz++;
            Thread.Sleep(150);
            executing = false;
            
        }

        private void toggle()
        {
            if (!ready)
            {
                ready = true;
                Status.Text = "On";
            }
            else
            {
                ready = false;
                Status.Text = "Off";
            }
            Trace.WriteLine("Toggled to: " + ready);
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            toggle();
        }

        public void PlayGame(object sender, RoutedEventArgs e)
        {
            ready = true;
            tree = Color.FromRgb(161, 116, 56);
            xyz = 0;
            while (ready)
            {
                if (ApplicationIsActivated())
                {

                }
                else
                {
                    for (int i = 0; i < pixels.GetLength(0); i++)
                    {
                        for (int j = 0; j < pixels.GetLength(1); j++)
                        {
                            pixels[i, j] = GetPixelColor(xcoord[i], ycoord[j]);
                        }
                    }
                    Color[] leftpixel = new Color[10];
                    leftpixel[0] = GetPixelColor(900, 618);
                    leftpixel[1] = GetPixelColor(900, 565);
                    leftpixel[2] = GetPixelColor(900, 512);
                    leftpixel[3] = GetPixelColor(900, 465);
                    leftpixel[4] = GetPixelColor(900, 415);
                    leftpixel[5] = GetPixelColor(900, 365);
                    leftpixel[6] = GetPixelColor(900, 315);
                    leftpixel[7] = GetPixelColor(900, 265);
                    leftpixel[8] = GetPixelColor(900, 215);
                    leftpixel[9] = GetPixelColor(900, 165);
                    Color[] rightpixel = new Color[10];
                    rightpixel[0] = GetPixelColor(1020, 618);
                    rightpixel[1] = GetPixelColor(1020, 565);
                    rightpixel[2] = GetPixelColor(1020, 512);
                    rightpixel[3] = GetPixelColor(1020, 465);
                    rightpixel[4] = GetPixelColor(1020, 415);
                    rightpixel[5] = GetPixelColor(1020, 365);
                    rightpixel[6] = GetPixelColor(1020, 315);
                    rightpixel[7] = GetPixelColor(1020, 265);
                    rightpixel[8] = GetPixelColor(1020, 215);
                    rightpixel[9] = GetPixelColor(1020, 165);
                    bool[] left = new bool[10];
                    for (int i = 0; i < left.Length; i++)
                    {
                        if (i == 0)
                        {
                            if (GetPixelColor(1020, 665) == tree)
                            {
                                left[i] = true;
                            }
                            else if (GetPixelColor(900, 665) == tree)
                            {
                                left[i] = false;
                            }
                            else if (GetPixelColor(900, 618) == tree)
                            {
                                left[i] = false;
                            }
                            else
                            {
                                left[i] = true;
                            }
                        }
                        else
                        {
                            if (pixels[0, i] != tree && pixels[0, i - 1] != tree)
                            {
                                left[i] = true;
                            }
                            else
                            {
                                left[i] = false;
                            }
                        }
                    }
                    for (int i = 0; i < left.Length; i++)
                    {
                        if (left[i])
                        {
                            SendKeys.SendWait("{LEFT}");
                            Thread.Sleep(20);
                        }
                        else
                        {
                            SendKeys.SendWait("{RIGHT}");
                            Thread.Sleep(20);
                        }

                    }

                }
                xyz++;
                Thread.Sleep(220);
            }
            return;
        }
        static public Color GetPixelColor(int x, int y)
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            uint pixel = GetPixel(hdc, x, y);
            ReleaseDC(IntPtr.Zero, hdc);
            Color color = Color.FromRgb(
                (byte)(pixel & 0x000000FF),
                (byte)((pixel & 0x0000FF00) >> 8),
                (byte)((pixel & 0x00FF0000) >> 16));
            return color;
        }
        public static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }
    }
}
