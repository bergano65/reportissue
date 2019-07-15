using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace ReportIssue
{
    /// <summary>
    /// Interaction logic for ShotPanel.xaml
    /// </summary>
    public partial class ShotPanel : UserControl
    {
        private bool _isImgDrawn;
        private bool _isMarkerDrawn;
        private TelemetryClient _tc;
        private MemoryStream _openStream;

        public Picture Picture { get; set; }
        private Bitmap Bitmap { get; set; }

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        private System.Windows.Controls.Image _img;
        private System.Windows.Forms.Timer _timer;
        public List<System.Drawing.Rectangle> Markers { get; set; }

        private IOperationHolder<RequestTelemetry> _markerOp;
        private System.Windows.Point _markerStartPosition;
        private System.Windows.Shapes.Rectangle _markerRect;
        private System.Drawing.Rectangle sr;

        public ShotPanel()
        {
            InitializeComponent();
            this._tc = new TelemetryClient();

            this.Picture = new Picture();
            this.Markers = new List<System.Drawing.Rectangle>();
            this._timer = new System.Windows.Forms.Timer();
            this._timer.Interval = 50;
            this._timer.Tick += new EventHandler(this._timer_Tick);
            this._timer.Start();
            this._isMarkerDrawn = false;

            this.ImgCanvas.Children.Clear();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (!this._isImgDrawn)
                return;
            if (!this._isMarkerDrawn)
                return;
            this.DrawMarker(Mouse.GetPosition((IInputElement)this._img));
        }

       private void DrawMarker(System.Windows.Point mousePosition)
        {
            this._tc.TrackEvent("Draw Marker", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            System.Drawing.Rectangle markerRect = this.GetMarkerRect(this._markerStartPosition, mousePosition);
            Canvas.SetLeft((UIElement)this._markerRect, markerRect.X);
            Canvas.SetTop((UIElement)this._markerRect, markerRect.Y);
            this._markerRect.Width = markerRect.Width;
            this._markerRect.Height = markerRect.Height;
        }

        public void SetPicture(Bitmap bitmap)
        {
            this.Bitmap = bitmap;
            IntPtr hbitmap = bitmap.GetHbitmap();
            IntPtr palette = (IntPtr)0;
            Int32Rect sourceRect = new Int32Rect(0, 0, bitmap.Width, bitmap.Height);
//            BitmapSizeOptions sizeOptions = BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height);

             System.Windows.Size renderSize = new System.Windows.Size(900, 800);
                int width2 = (int)renderSize.Width;
                int height2 = (int)renderSize.Height;
                BitmapSizeOptions sizeOptions = BitmapSizeOptions.FromWidthAndHeight(width2, height2);
                BitmapSource sourceFromHbitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, palette, sourceRect, sizeOptions);


            this._img = new System.Windows.Controls.Image();
            this._img.Source = (ImageSource)sourceFromHbitmap;

//            this._img.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, palette, sourceRect, sizeOptions);
//            this._img.Stretch = Stretch.Fill;

            this._isImgDrawn = true;

            if (this.ImgCanvas.Children.Count > 0 && this.ImgCanvas.Children[0] is System.Windows.Controls.Image)
            {
                this.ImgCanvas.Children.RemoveAt(0);
            }

            this._img.MouseDown += _img_MouseDown;
            this._img.MouseUp += _img_MouseUp;

            this.ImgCanvas.Children.Insert(0, (UIElement)this._img);
            Canvas.SetLeft((UIElement)this._img, 0.0);
            Canvas.SetRight((UIElement)this._img, 0.0);
            DeleteObject(hbitmap);
            this._isImgDrawn = true;
        }

        private void _img_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!this._isImgDrawn)
                return;
            if (!this._isMarkerDrawn)   
                return;

            this._tc.TrackEvent("Finish marker", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this._tc.StopOperation<RequestTelemetry>(this._markerOp);
            System.Drawing.Rectangle markerRect = this.GetMarkerRect(this._markerStartPosition, Mouse.GetPosition((IInputElement)this._img));
            Canvas.SetRight(this._markerRect, markerRect.Right);
            Canvas.SetBottom(this._markerRect, markerRect.Bottom);
            this.Markers.Add(markerRect);
            this._markerRect = (System.Windows.Shapes.Rectangle)null;
            this._isMarkerDrawn = false;
        }


        private void _img_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!this._isImgDrawn)
                return;
            if (this._isMarkerDrawn)
            {
                return;
            }

            this._tc.TrackEvent("Start marker", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this._markerOp = this._tc.StartOperation<RequestTelemetry>(new RequestTelemetry("Place marker", (DateTimeOffset)DateTime.Now, new TimeSpan(0, 0, 40), "200", true));
            this._markerStartPosition = Mouse.GetPosition((IInputElement)this._img);
            this._markerRect = new System.Windows.Shapes.Rectangle();
            this._markerRect.Stroke = (System.Windows.Media.Brush)new SolidColorBrush(Colors.Red);
            this._markerRect.StrokeThickness = 2.0;
            Canvas.SetLeft((UIElement)this._markerRect, this._markerStartPosition.X);
            Canvas.SetTop((UIElement)this._markerRect, this._markerStartPosition.Y);
            this.ImgCanvas.Children.Add((UIElement)this._markerRect);
            this._isMarkerDrawn = true;
         }

        public void Open()
        {
            if (this.Picture == null)
            {
                return;
            }

            if (this.Picture.Bytes == null)
            {
                return;
            }

            this._openStream = new MemoryStream(this.Picture.Bytes);

            this.Bitmap = new Bitmap((Stream)this._openStream);

            this.ImgCanvas.Children.Clear();

            if (this.Picture.MarkerString == null)
            {
                this.Picture.MarkerString = "";
            }

            SetPicture(this.Bitmap);


            try
            {
                string[] markArray = this.Picture.MarkerString.Split(':');
                foreach (string m in markArray)
                {
                    string[] strArray = m.Split(':');
                    int num = strArray.Length / 4;
                    for (int index = 0; index < num; ++index)
                    {
                        System.Drawing.Rectangle sr =
                            ScaleRectangle(
                                new System.Drawing.Rectangle(
                                    int.Parse(strArray[index * 4]),
                                    int.Parse(strArray[(index * 4) + 1]),
                                    int.Parse(strArray[(index * 4) + 2]),
                                    int.Parse(strArray[(index * 4) + 3])),
                                    false);
                        this.Markers.Add(sr);
                        System.Windows.Shapes.Rectangle dr = new System.Windows.Shapes.Rectangle();
                        Canvas.SetLeft((UIElement)dr, sr.X);
                        Canvas.SetTop((UIElement)dr, sr.Y);
                        this.ImgCanvas.Children.Add((UIElement)dr);
                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        private System.Drawing.Rectangle GetMarkerRect(System.Windows.Point markerStartPosition, System.Windows.Point mousePosition)
        {
            return new System.Drawing.Rectangle()
            {
                X = (int)Math.Min(markerStartPosition.X, mousePosition.X),
                Y = (int)Math.Min(markerStartPosition.Y, mousePosition.Y),
                Width = (int)Math.Abs(markerStartPosition.X - mousePosition.X),
                Height = (int)Math.Abs(markerStartPosition.Y - mousePosition.Y)
            };
        }

        public void RemoveMarkers()
        {
            if (!this._isImgDrawn)
            {
                this.ImgCanvas.Children.Clear();
            }
            else
            {
                while (this.ImgCanvas.Children.Count > 1)
                {
                    this.ImgCanvas.Children.RemoveAt(this.ImgCanvas.Children.Count - 1);
                }
            }
        }

        public bool IsValid(out string msg)
        {
            if (!this._isImgDrawn)
            {
                msg = "Image isn't drawn";
                return false;
            }

            if (this.Markers.Count == 0)
            {
                msg = "No markers defined";
                return false;
            }

            msg = string.Empty;
            return true;
        }

        public void Save()
        {
            StringBuilder markers = new StringBuilder();
            foreach(System.Drawing.Rectangle m in this.Markers)
            {
                if (this.Markers.IndexOf(m) > 0)
                {
                    markers.Append(":");
                }

                System.Drawing.Rectangle sr = ScaleRectangle(m, true);
                markers.AppendFormat("{0},{1},{2},{3}",
                    sr.Top, sr.Left, sr.Right, sr.Bottom);
            }

            this.Picture.MarkerString = markers.ToString();
            MemoryStream memoryStream = new MemoryStream();
            this.Bitmap.Save((Stream)memoryStream, ImageFormat.Png);
            this.Picture.Bytes = memoryStream.ToArray();
        }

        private System.Drawing.Rectangle ScaleRectangle(System.Drawing.Rectangle m, bool toScreeen)
        {
            int scaleY = (int)((double)this.Bitmap.Height / 700);
            int scaleX = (int)((double)this.Bitmap.Width / 800);
            if (toScreeen)
            { 
                return new System.Drawing.Rectangle(m.X * scaleX, m.Y * scaleY, m.Width * scaleX, m.Height * scaleY); 
            }   
            else
            {
                return new System.Drawing.Rectangle(m.X / scaleX, m.Y / scaleY, m.Width / scaleX, m.Height / scaleY);

            }
        }

        public bool IsOpened()
        {
            if (this.ImgCanvas.Visibility == Visibility.Visible)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}     