using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace screen_recorder
{
    public class SelectRegion
    {
        public Window window;
        public Point pt1, pt2;
        public Rectangle rectangle;
        public Grid grid = new Grid();
        public Image image = new Image();
        public Canvas canvas = new Canvas();
        public TextBlock text = new TextBlock() { Background = Brushes.White };
        public Line VerticalLine = new Line() { StrokeThickness = 3, Stroke = Brushes.Red, Y2 = SystemParameters.PrimaryScreenHeight, };
        public Line HorizontalLine = new Line() { StrokeThickness = 3, Stroke = Brushes.Red, X2 = SystemParameters.PrimaryScreenWidth, };
        public SelectRegion()
        {
            _ = grid.Children.Add(image);
            _ = grid.Children.Add(canvas);
            _ = canvas.Children.Add(text);

            canvas.MouseDown += Canvas_MouseDown;
            canvas.MouseMove += Canvas_MouseMove;
            _ = canvas.Children.Add(VerticalLine);
            _ = canvas.Children.Add(HorizontalLine);
        }
        public async Task Start()
        {
            window = new Window()
            {
                Top = 0,
                Left = 0,
                Topmost = true,
                Content = grid,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                WindowState = WindowState.Maximized,
                Width = SystemParameters.PrimaryScreenWidth,
                Height = SystemParameters.PrimaryScreenHeight,
            };
            window.PreviewKeyDown += PreviewKeyDown;
            CaptureScreen();
            window.Show();

            canvas.Children.Remove(rectangle);
            _ = canvas.Children.Add(rectangle = new Rectangle()
            {
                StrokeThickness = 3,
                Stroke = Brushes.Green,
            });

            pt1.X = pt1.Y = -1;
            _ = canvas.CaptureMouse();
            await Task.Run(() =>
            {
                bool isCaptured = true;
                while (isCaptured)
                {
                    _ = Thread.CurrentThread.Join(100);
                    window.Dispatcher.Invoke(() => { isCaptured = canvas.IsMouseCaptured; });
                }
            });
            window.Close();
        }
        public Rect GetSelectedRegion()
        {
            return new Rect(
                Math.Min(pt1.X, pt2.X),
                Math.Min(pt1.Y, pt2.Y),
                Math.Abs(pt1.X - pt2.X),
                Math.Abs(pt1.Y - pt2.Y)
            );
        }
        private void PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.GetKeyStates(Key.Escape) & KeyStates.Down) == KeyStates.Down)
            {
                canvas.ReleaseMouseCapture();
            }
        }
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            // Get Cursor Points
            Point pt = e.GetPosition(window);
            Point scrpt = window.PointToScreen(pt);

            // Set Text
            if (scrpt.Y >= SystemParameters.PrimaryScreenHeight / 2)
            {
                Canvas.SetTop(text, scrpt.Y - text.ActualHeight - 10);
            }
            else
            {
                Canvas.SetTop(text, scrpt.Y + 15);
            }
            if (scrpt.X >= SystemParameters.PrimaryScreenWidth / 2)
            {
                Canvas.SetLeft(text, scrpt.X - text.ActualWidth - 10);
            }
            else
            {
                Canvas.SetLeft(text, scrpt.X + 15);
            }
            text.Text = $"({scrpt.X},{scrpt.Y})";

            App.selectRegion.VerticalLine.X1 =
                App.selectRegion.VerticalLine.X2 = scrpt.X;
            App.selectRegion.HorizontalLine.Y1 =
                App.selectRegion.HorizontalLine.Y2 = scrpt.Y;

            if (pt1.X != -1)
            {
                pt2.X = scrpt.X;
                pt2.Y = scrpt.Y;
                rectangle.Width = Math.Abs(pt1.X - pt2.X);
                rectangle.Height = Math.Abs(pt1.Y - pt2.Y);
                Canvas.SetTop(rectangle, Math.Min(pt1.Y, pt2.Y));
                Canvas.SetLeft(rectangle, Math.Min(pt1.X, pt2.X));
            }
        }
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(window);
            Point scrpt = window.PointToScreen(pt);

            if (pt1.X == -1)
            {
                pt1.X = scrpt.X;
                pt1.Y = scrpt.Y;
            }
            else
            {
                pt2.X = scrpt.X;
                pt2.Y = scrpt.Y;
                canvas.ReleaseMouseCapture();
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr obj);
        private void CaptureScreen()
        {
            System.Drawing.Bitmap bitmap;
            bitmap = new System.Drawing.Bitmap(
                (int)SystemParameters.PrimaryScreenWidth,
                (int)SystemParameters.PrimaryScreenHeight,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
            }
            IntPtr handle = IntPtr.Zero;
            try
            {
                handle = bitmap.GetHbitmap();
                image.Source = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception)
            {
            }
            finally
            {
                _ = DeleteObject(handle);
            }
        }
    }
}
