/*
SimpleReference - alpha
developed by keroroxzz
Alpha - 1.0.2
*/
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Brushes = System.Windows.Media.Brushes;
using Control = System.Windows.Forms.Control;
using Keys = System.Windows.Forms.Keys;
using MouseButtons = System.Windows.Forms.MouseButtons;
using Point = System.Windows.Point;

namespace SimpRef
{
    //global varialbes
    public static class GloVar
    {
        //constant variables
        public static double BarHeight = 25;

        //variables
        public static double DpiRatio;
        public static bool AutoFocus = true;
        public static double Opacity_norm = 1.0;
        public static bool AutoHiding = true;
    }

    //MainWindow class
    public partial class MainWindow : Window
    {
        double left_, top_, right_, bottom_;    //position and size for other threads

        bool waiting,   //is waiting for mouse to leave
            adjusting;  //is adjusting

        Thread waitThread, mouseCheckThread;

        Images images;  //ImageQueue

        public MainWindow()
        {
            InitializeComponent();

            waiting = false;
            adjusting = false;
            ToolBarHeight.Height = new GridLength(GloVar.BarHeight);

            images = new Images();

            WindowsThread = new Thread(UpdateActiveWindow);
            WindowsThread.Start();

            mouseCheckThread = new Thread(MouseCheck);
            mouseCheckThread.Start();
        }

        //============================ToolBar======================================

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            PreviousImage();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            NextImage();
        }

        private void DeleteImage_Click(object sender, RoutedEventArgs e)
        {
            imgbox.Source = images.Remove().image;

            if (images.Length() > 0)
            {
                images.Current().apply(this);
            }

            UpdateIndexBox();
        }

        private void AddWindButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow n = new MainWindow();
            n.Show();
            n.Activate();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            //images.Dispose();
            Close();
        }

        private void ToolBar_MouseLeave(object sender, MouseEventArgs e)
        {
            ToolBar.Opacity = 0.5;
        }

        private void ToolBar_MouseEnter(object sender, MouseEventArgs e)
        {
            ToolBar.Opacity = 1.0;
        }

        private void ToolBar_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                NextImage();
            }

            if (e.Delta < 0)
            {
                PreviousImage();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                NextImage();
            }
            else if (e.Key == Key.Left)
            {
                PreviousImage();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (waitThread != null)
            {
                waitThread.Abort();
            }

            if (WindowsThread != null)
            {
                WindowsThread.Abort();
            }

            if (mouseCheckThread != null)
            {
                mouseCheckThread.Abort();
            }
        }

        //========================Auto Hiding================================

        private void ImgArea_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!waiting && GloVar.AutoHiding)
            {
                if (!adjusting)
                {
                    ImgArea.Opacity = 0.0;
                }

                UpdateSize();

                waitThread = new Thread(WaitForExit);
                waitThread.Start();
            }
        }

        private void MouseCheck()
        {
            while (true)
            {
                Dispatcher.Invoke(delegate () { UpdateSize();});

                if (IsMouseInside() && !waiting && GloVar.AutoHiding)
                {
                    if (!adjusting)
                    {
                        Dispatcher.Invoke(delegate () { ImgArea.Opacity = 0.0; });
                    }

                    waitThread = new Thread(WaitForExit);
                    waitThread.Start();
                }

                SpinWait.SpinUntil(() => false, 100);
            }
        }

        //Waiting for mouse to exit
        private void WaitForExit()
        {
            int count = 0;

            waiting = true;
            while (waiting)
            {
                if (!IsMouseInside())
                {
                    if (!operating)
                    {
                        if (--count <= 0)
                        {
                            adjusting = false;
                            waiting = false;
                            break;
                        }
                    }
                    else
                    {
                        count = 15;
                    }
                }
                else if (operating)
                    count = 0;

                SpinWait.SpinUntil(() => false, 80);
            }

            Dispatcher.Invoke(delegate ()
            {
                if (!adjusting)
                {
                    Adjust.Foreground = Brushes.Transparent;
                    ImgArea.Opacity = GloVar.Opacity_norm;
                    ActiveTarget();
                }
            });
        }

        //Update the size variables
        private void UpdateSize()
        {
            left_ = Left;
            top_ = Top + GloVar.BarHeight;
            right_ = Left + Width;
            bottom_ = Top + Height;
        }

        //check if the mouse is inside the image area.
        private bool IsMouseInside()
        {
            Point mp = MousePos();
            return !( mp.X < left_ || mp.Y < top_ - ( adjusting ? GloVar.BarHeight : 0 ) || mp.X > right_ || mp.Y > bottom_ );
        }

        private bool IsMouseAround(double r)
        {
            Point mp = MousePos();
            return !( mp.X < left_ - r || mp.Y < top_ - r - ( adjusting ? GloVar.BarHeight : 0 ) || mp.X > right_ + r || mp.Y > bottom_ + r );
        }

        //Lock the window state
        private void Window_StateChanged(object sender, EventArgs e)
        {
            WindowState = WindowState.Normal;
        }

        //====================Grip Resizing========================

        Point MouseOrigin, Size;
        private void Grip_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseOrigin = MousePos();
            Size.X = Width;
            Size.Y = Height;

            new Thread(Griping).Start();
        }

        private void Griping()
        {
            GloVar.AutoHiding = false;
            while (Control.MouseButtons == MouseButtons.Left)
            {
                Vector displacement = MousePos() - MouseOrigin;

                Dispatcher.Invoke(delegate ()
                {
                    Width = Size.X + displacement.X;
                    Height = Size.Y + displacement.Y;
                });

                SpinWait.SpinUntil(() => false, 20);
            }
            GloVar.AutoHiding = true;
        }

        //Get the mouse position
        private Point MousePos()
        {
            return new Point(Control.MousePosition.X * GloVar.DpiRatio, Control.MousePosition.Y * GloVar.DpiRatio);
        }

        //====================drop=========================

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            GloVar.AutoHiding = false;
            e.Effects = DragDropEffects.Copy;
        }

        //Drop the image
        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                String[] paths = (String[])e.Data.GetData(DataFormats.FileDrop, true);

                if (paths.Length > 0)
                {
                    if (paths.Length == 1 && Directory.Exists(paths[0]))
                    {
                        images.Insert(Directory.GetFiles(paths[0]));
                        ApplyCurretnImage();
                    }
                    else
                    {
                        images.Insert(paths);
                        ApplyCurretnImage();
                    }
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.Html, false))
            {
                String Html = (String)e.Data.GetData(DataFormats.Html, false),
                    url = GetImageUrl(Html);

                if (url.Length > 5)
                {
                    Msgbox msg = new Msgbox("Downloading...", true);

                    images.Insert(url);
                    images.Current().image.DownloadCompleted += (s, ee) => DownloadeFinish(images.Current().image, msg);
                    images.Current().image.DownloadProgress += (s, ee) => msg.UpdateProgressBar(s, ee);
                }
            }

            UpdateIndexBox();
            Adjust_Click(sender, e);
            GloVar.AutoHiding = true;
        }

        private void DownloadeFinish(BitmapSource LoadedImage, Msgbox msg)
        {
            if (LoadedImage.Equals(images.Current().image))
            {
                ApplyCurretnImage();
                images.Current().image.DownloadProgress -= msg.UpdateProgressBar;
            }

        }

        private String GetImageUrl(String Html)
        {
            int indexEnd = -1;
            String Html_lower = Html.ToLower();
            String[] extensions = { ".jpg", ".bmp", ".jpeg", ".png", ".tiff", ".tif" };

            for (int i = 0; i < extensions.Length; i++)
            {
                if((indexEnd = Html_lower.IndexOf(extensions[i])) >= -1)
                {
                    indexEnd += extensions[i].Length - 1;
                    break;
                }
            }

            int indexStart = Html.LastIndexOf("http", indexEnd);

            if (indexEnd < 0 || indexStart < 0)
            {
                return "";
            }

            String
                url = Html.Substring(indexStart, indexEnd - indexStart + 1);

            if (url.Contains("i.pximg.net"))
            {
                //Use the proxy to download the image
                url = url.Replace("i.pximg.net", "i.pixiv.cat");

                return url;
            }
            else
            {
                return url;
            }
        }

        //========================image adjust========================

        Point MousePrevious;
        bool operating = false;
        double radiusOrigin_i, angleOrigin_i, rotation_i, radiusMouse_i, angleMouse_i, scale_ix, scale_iy;

        private void ImgArea_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (images.Length() > 0)
            {
                UpdateSize();
                MousePrevious = MousePos();

                //double click to reset
                if (e.ClickCount == 2)
                {
                    images.Current().reset();
                    images.Current().apply(this);
                }

                if (!operating)
                {
                    new Thread(WaitForReleasing).Start();
                }
            }
        }

        private void Adjust_Click(object sender, RoutedEventArgs e)
        {
            if (images.Length() > 0)
            {
                adjusting = true;
                Adjust.Foreground = Brushes.DarkRed;
            }
        }

        private void Flip_Click(object sender, RoutedEventArgs e)
        {
            if (images.Length() > 0)
            {
                images.Current().set(this);
                images.Current().flip();
                images.Current().apply(this);

                Adjust_Click(sender, e);
            }
        }

        private void ResetImage_Click(object sender, RoutedEventArgs e)
        {
            if (images.Length() > 0)
            {
                images.Current().reset();
                images.Current().apply(this);
            }
        }

        private void initializeRoation()
        {
            double
            HalfWidth = Width * 0.5,
            HalfHeight = ( Height - GloVar.BarHeight ) * 0.5,
            dx = translation_img.X - HalfWidth,
            dy = translation_img.Y - HalfHeight;

            radiusOrigin_i = Math.Sqrt(dx * dx + dy * dy);
            angleOrigin_i = Math.Atan2(dy, dx);
            scale_ix = scale_img.ScaleX;
            scale_iy = scale_img.ScaleY;

            rotation_i = rotation_img.Angle;

            dx = MousePrevious.X - HalfWidth - Left;
            dy = MousePrevious.Y - HalfHeight - Top - GloVar.BarHeight;

            radiusMouse_i = Math.Sqrt(dx * dx + dy * dy);

            angleMouse_i = Math.Atan2(dy, dx);
        }

        private void WaitForReleasing()
        {
            double rtd = 57.2957795131;

            Keys lastModifierKeys = Keys.None;
            var lastButton = MouseButtons.None;

            operating = true;
            while (operating)
            {
                if (( Control.ModifierKeys != Keys.None && lastModifierKeys == Keys.None ) ||
                    ( Control.MouseButtons == MouseButtons.Right && lastButton != MouseButtons.Right ))
                {
                    Dispatcher.Invoke(delegate () { initializeRoation(); });
                }

                lastModifierKeys = Control.ModifierKeys;
                lastButton = Control.MouseButtons;

                if (Control.MouseButtons == MouseButtons.Left || Control.MouseButtons == MouseButtons.Right)
                {
                    Dispatcher.Invoke(delegate ()
                    {

                        Point MouseNow = MousePos();

                        if (Control.ModifierKeys != Keys.None || Control.MouseButtons == MouseButtons.Right)    //rotation and scaleing
                        {
                            double
                                HalfWidth = Width * 0.5,
                                HalfHeight = ( Height - GloVar.BarHeight ) * 0.5,
                                dx = MouseNow.X - HalfWidth - Left,
                                dy = MouseNow.Y - HalfHeight - Top - GloVar.BarHeight,
                                angleMouseNow = Math.Atan2(dy, dx),
                                dang = angleMouseNow - angleMouse_i,
                                scaleNow = 1.0;

                            //noncontinuous rotation
                            if (radiusMouse_i < Width / 4.0)
                            {
                                rotation_img.Angle = ( (int)( rotation_i + dang * rtd ) ) / 15 * 15;
                                dang = ( rotation_img.Angle - rotation_i ) / rtd;

                                translation_img.X = radiusOrigin_i * Math.Cos(angleOrigin_i + dang) + HalfWidth;
                                translation_img.Y = radiusOrigin_i * Math.Sin(angleOrigin_i + dang) + HalfHeight;
                            }

                            //continuous rotation with scaling
                            else
                            {
                                rotation_img.Angle = rotation_i + dang * rtd;

                                scaleNow = Math.Sqrt(dx * dx + dy * dy) / radiusMouse_i;

                                scale_img.ScaleX = scale_ix * scaleNow;
                                scale_img.ScaleY = scale_iy * scaleNow;
                                translation_img.X = scaleNow * radiusOrigin_i * Math.Cos(angleOrigin_i + dang) + HalfWidth;
                                translation_img.Y = scaleNow * radiusOrigin_i * Math.Sin(angleOrigin_i + dang) + HalfHeight;
                            }
                        }
                        else   //translation
                        {
                            translation_img.X += MouseNow.X - MousePrevious.X;
                            translation_img.Y += MouseNow.Y - MousePrevious.Y;
                        }
                        MousePrevious = MouseNow;
                    });

                    SpinWait.SpinUntil(() => false, 25);
                }
                else if (Control.MouseButtons == MouseButtons.None)
                {
                    operating = false;
                }
            }
        }

        //===================auto focus===========================

        [DllImport("User32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("User32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        IntPtr ActiveWindowsHWnd, lastHWnd;

        Thread WindowsThread;

        private void UpdateActiveWindow()
        {
            while (GloVar.AutoFocus)
            {
                try
                {
                    lastHWnd = GetForegroundWindow();
                    GetWindowThreadProcessId(lastHWnd, out uint id);

                    if (Process.GetProcessById((int)id).ProcessName != "explorer")
                    {
                        if (id != 0 && id != Process.GetCurrentProcess().Id)
                        {
                            ActiveWindowsHWnd = lastHWnd;
                        }
                    }
                }
                catch { }

                SpinWait.SpinUntil(() => false, 1000);
            }
        }

        private void ActiveTarget()
        {
            if (GloVar.AutoFocus && lastHWnd == ActiveWindowsHWnd)
            {
                SetForegroundWindow(ActiveWindowsHWnd);
            }
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            ActiveTarget();
        }

        //==================Switching Funcs====================

        private void ApplyCurretnImage()
        {
            imgbox.Source = images.Current().image;

            if (images.Length() > 0)
            {
                images.Current().reset();
                images.Current().apply(this);
            }
        }

        private void NextImage()
        {
            images.Current().set(this);

            imgbox.Source = images.Next().image;

            if (images.Length() > 0)
            {
                images.Current().apply(this);
            }

            UpdateIndexBox();
        }

        private void PreviousImage()
        {
            images.Current().set(this);

            imgbox.Source = images.Previous().image;

            if (images.Length() > 0)
            {
                images.Current().apply(this);
            }

            UpdateIndexBox();
        }

        private void UpdateIndexBox()
        {
            IndexBox.Text = images.Index() + 1 + "/" + images.Length();
        }

        //=====================================screenshot==========================================

        private void ScreenShot_Click(object sender, RoutedEventArgs e)
        {
            bool af_ = GloVar.AutoFocus;////////////but unfixed
            GloVar.AutoFocus = false;

            Visibility = Visibility.Hidden;
            new Thread(TakeScreenShot).Start(0);

            GloVar.AutoFocus = af_;
        }

        private void ScreenShot_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                bool af_ = GloVar.AutoFocus;
                GloVar.AutoFocus = false;

                Visibility = Visibility.Hidden;
                if (e.RightButton == MouseButtonState.Pressed)
                {
                    new Thread(TakeScreenShot).Start(1);
                }
                else if (e.MiddleButton == MouseButtonState.Pressed)
                {
                    new Thread(TakeScreenShot).Start(2);
                }

                GloVar.AutoFocus = af_;
            }
        }

        private void TakeScreenShot(object para)
        {
            SpinWait.SpinUntil(() => false, 500);

            Dispatcher.Invoke(delegate ()
            {
                int left = (int)( Left / GloVar.DpiRatio ),
                    top = (int)( Top / GloVar.DpiRatio ),
                    width = (int)( Width / GloVar.DpiRatio ),
                    height = (int)( Height / GloVar.DpiRatio );

                Bitmap bmp = new Bitmap(width, height);
                Graphics graphics = Graphics.FromImage(bmp);
                graphics.CopyFromScreen(left, top, 0, 0, new System.Drawing.Size(width, height));

                String path = Directory.GetCurrentDirectory() + "\\screenshot_" +
                    DateTime.Now.ToShortDateString().Replace('/', '_') +
                    DateTime.Now.ToLongTimeString().Replace(':', '_') + ".jpg";

                if ((int)para == 0)
                {
                    try { Clipboard.SetImage(Convert(bmp)); }
                    catch { SpinWait.SpinUntil(() => false, 100); }
                }
                else if ((int)para == 1)
                {
                    bmp.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
                else if ((int)para == 2)
                {
                    BitmapImage img = new BitmapImage();
                    Stream ms = new MemoryStream();
                    bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

                    img.BeginInit();
                    img.StreamSource = ms;
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.EndInit();

                    images.Insert(img);
                    ApplyCurretnImage();
                    UpdateIndexBox();
                    Adjust_Click(null, null);
                }

                graphics.Dispose();
                bmp.Dispose();

                Visibility = Visibility.Visible;
            });
        }

        [DllImport("gdi32.dll")]
        private static extern void DeleteObject(IntPtr obj);

        private BitmapSource Convert(Bitmap bmp)
        {
            IntPtr hbitmap = bmp.GetHbitmap();
            BitmapSource source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(hbitmap);
            return source;
        }

        //======================Window Draging============================================
        private void ToolBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.ClickCount == 1)
            {
                waiting = false;
                Drag();
            }
        }

        private void Drag(object sender = null, MouseButtonEventArgs e = null)
        {
            GloVar.AutoHiding = false;
            new Thread(DragUpdate).Start(new Rect(MousePos().X, MousePos().Y, Left, Top));
        }

        void DragUpdate(object p_)
        {
            Rect p = (Rect)p_;
            Point mp = new Point(p.X, p.Y),
                WindowsPosition = new Point(p.Width, p.Height);

            double l, t;
            var button = Control.MouseButtons;

            while (Control.MouseButtons == button)
            {
                l = WindowsPosition.X + MousePos().X - mp.X;
                t = WindowsPosition.Y + MousePos().Y - mp.Y;

                Dispatcher.Invoke(delegate ()
                {
                    l = l * 0.7 + 0.3 * Left;
                    t = t * 0.7 + 0.3 * Top;

                    Left = l;
                    Top = t;
                });
                SpinWait.SpinUntil(() => false, 20);
            }
            GloVar.AutoHiding = true;
        }

        //============================================test=============================================

        private void SetOpacity()
        {
            Point mp = MousePos();

            if (mp.Y < bottom_ && mp.Y > top_)
            {
                if (mp.X > right_)
                {
                    double res = ( mp.X - right_ ) / 50.0;
                    res = res > 1.0 ? 1.0 : res;
                    ImgArea.Opacity = res;
                }
                else if (mp.X < left_)
                {
                    double res = ( left_ - mp.X ) / 50.0;
                    res = res > 1.0 ? 1.0 : res;
                    ImgArea.Opacity = res;
                }
            }
            else if (mp.X < left_ && mp.X > right_)
            {
                if (mp.Y > bottom_)
                {
                    double res = ( mp.Y - bottom_ ) / 50.0;
                    res = res > 1.0 ? 1.0 : res;
                    ImgArea.Opacity = res;
                }
                else if (mp.Y < top_)
                {
                    double res = ( top_ - mp.Y ) / 50.0;
                    res = res > 1.0 ? 1.0 : res;
                    ImgArea.Opacity = res;
                }
            }
        }
    }
}
