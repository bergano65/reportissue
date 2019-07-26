using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ReportIssue
{

    /// <summary>
    /// Interaction logic for EditIssueDlg.xaml
    /// </summary>
    public partial class EditIssueDlg : Window
    {
        private bool _isIssueEdited;
        private bool _isPictureEdited;
        private TelemetryClient _tc;
        private Window _parentWindow;
        private WindowState _parentWindowState;
        private WindowState _windowState;
        public Issue CurrentIssue;
        private System.Windows.Controls.Image _img;
        private bool _isImgDrawn;
        private System.Windows.Forms.Timer _timer;
        private bool _isMarkerDrawn;
        private IOperationHolder<RequestTelemetry> _markerOp;
        private System.Windows.Point _markerStartPosition;
        private System.Windows.Shapes.Rectangle _markerRect;
        private Picture _currentPicture;
        private List<System.Drawing.Rectangle> Markers;
        private Bitmap Bitmap;

        public bool Saved { get; set; }
        
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public EditIssueDlg(Window parent, Issue issue)
        {
            this.InitializeComponent();
            this._tc = new TelemetryClient();
            this._tc.TrackPageView("Edit Issue");
            this._tc.TrackEvent("Start edit issue", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this._parentWindow = parent;
            this.CurrentIssue = issue;
            this._timer = new System.Windows.Forms.Timer();
            this._timer.Tick += _timer_Tick;
            this._timer.Start();
            this.Markers = new List<System.Drawing.Rectangle>();
            this.CurrentIssue = issue;
            if (issue != null)
            {
                ShowIssue();
            }
            else
            {
                NewIssue();
            }
        }

        private void ShowIssue()
        {
        }

        private void NewIssue()
        {
            this._tc.TrackEvent("New Issue", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this._isIssueEdited = true;
            this.Saved = false;
            this.CurrentIssue = new Issue();
        }

        private void addPictureButton_Click(object sender, RoutedEventArgs e)
        {
            // save currento picture if exists
            if (this._currentPicture != null)
            {
                if (!SavePicture(this._currentPicture))
                {
                    return;
                }

            }

            RIDataModelContainer d = new RIDataModelContainer();

            this._currentPicture = new Picture();
            Picture p = this._currentPicture;

            System.Windows.Controls.ListBoxItem item = new System.Windows.Controls.ListBoxItem();
            item.Content = string.Format("Picture - {0}", _pictureList.Items.Count + 1);
            item.DataContext = this._currentPicture;
            _pictureList.Items.Add(item);
        
        }

        private void removePictureButton_Click(object sender, RoutedEventArgs e)
        {
            if (this._currentPicture == null)
            {
                return;

            }
                
            if (_pictureList.Items.Count == 0)
            {
                return;
            }

            if (_pictureList.SelectedIndex != -1)
            {
                _pictureList.Items.RemoveAt(_pictureList.SelectedIndex);
            }

            if (_pictureList.Items.Count > 0)
            {
                _pictureList.SelectedIndex = _pictureList.Items.Count - 1;
            }
        }

        private System.Drawing.Rectangle ScaleMarker(System.Drawing.Rectangle from, bool toBitmap)
        {
            double dX = (double)this.Bitmap.Width/(double)ImgCanvas.RenderSize.Width;
            double dY = (double)this.Bitmap.Height / (double)ImgCanvas.RenderSize.Height;
            if (toBitmap)
            {
                return new System.Drawing.Rectangle((int)(from.X * dX), (int)(from.Y * dY), (int)(from.Width * dX), (int)(from.Height * dY));
            }
            else
            {
                return new System.Drawing.Rectangle((int)(from.X / dX), (int)(from.Y / dY), (int)(from.Width / dX), (int)(from.Height / dY));
            }
        } 

        private bool SavePicture(Picture picture)
        {
            if (this._currentPicture == null)
            {
                return true;
            }

            if (!this._isImgDrawn)
            {
                this._pictureList.SelectedIndex = FindPicture(picture);
                System.Windows.MessageBox.Show("No screen shot defined", "Report Issue");
                return false;
            }

            if (this.Markers.Count == 0)
            {
                this._pictureList.SelectedIndex = FindPicture(picture);
                System.Windows.MessageBox.Show("No markers defined", "Report Issue");
                return false;
            }

            this._currentPicture.Markers.Clear();
            foreach (System.Drawing.Rectangle r in this.Markers)
            {
                this._currentPicture.Markers.Add(ScaleMarker(r, true));
            }

            this._currentPicture.Bitmap = this.Bitmap;
            this._currentPicture.Save();

            this._isPictureEdited = false;

            return true;
        }

        private int FindPicture(Picture picture)
        {
            int num = 0;
            foreach (System.Windows.Controls.ListBoxItem item in 
                (this._pictureList.Items))
            {
                Picture itemPicture = item.DataContext as Picture;
                if (itemPicture == picture)
                {
                    return num;
                }

                num++;
            }

            return -1;
        }

        private void shotButton_Click(object sender, RoutedEventArgs e)
        {
            this._tc.TrackEvent("Take Screenshot", (IDictionary<string, string>)null, (IDictionary<string, double>)null);

            if (this._currentPicture == null)
            {
                return;
            }

            this._windowState = this.WindowState;
            if (this._parentWindow != null)
            {
                this._parentWindowState = this._parentWindow.WindowState;
                this._parentWindow.WindowState = WindowState.Minimized;
            }

            this.WindowState = WindowState.Minimized;

            System.Drawing.Rectangle bounds = Screen.PrimaryScreen.Bounds;
            int width = bounds.Width;
            int height = bounds.Height;
            int x = bounds.X;
            int y = bounds.Y;
            System.Drawing.Size size = bounds.Size;

            this.Bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics graphics = Graphics.FromImage((System.Drawing.Image)Bitmap);
            graphics.CopyFromScreen(x, y, 0, 0, size, CopyPixelOperation.SourceCopy);

            IntPtr hbitmap = this.Bitmap.GetHbitmap();
            IntPtr palette = (IntPtr)0;
            Int32Rect sourceRect = new Int32Rect(0, 0, this.Bitmap.Width, this.Bitmap.Height);
            //            BitmapSizeOptions sizeOptions = BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height);

            System.Windows.Size renderSize = ImgCanvas.RenderSize;
            int width2 = (int)renderSize.Width;
            int height2 = (int)renderSize.Height;
            BitmapSizeOptions sizeOptions = BitmapSizeOptions.FromWidthAndHeight(width2, height2);
            BitmapSource sourceFromHbitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, palette, sourceRect, sizeOptions);


            this._img = new System.Windows.Controls.Image();
            this._img.Source = (ImageSource)sourceFromHbitmap;

            //            this._img.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, palette, sourceRect, sizeOptions);
            //            this._img.Stretch = Stretch.Fill;

            this._isImgDrawn = true;
            this._isPictureEdited = true;

            if (this.ImgCanvas.Children.Count > 0 && this.ImgCanvas.Children[0] is System.Windows.Controls.Image)
            {
                this.ImgCanvas.Children.RemoveAt(0);
            }

 
            this.ImgCanvas.Children.Insert(0, (UIElement)this._img);
            Canvas.SetLeft((UIElement)this._img, 0.0);
            Canvas.SetRight((UIElement)this._img, 0.0);
            DeleteObject(hbitmap);
            this._isImgDrawn = true;


            if (this._parentWindow != null)
            {
                this._parentWindow.WindowState = this._parentWindowState;
            }
             
            this.WindowState = this._windowState;
            this.Activate();

        }

       private void _timer_Tick(object sender, EventArgs e)
        {

            if (this._currentPicture == null)
            {
                return;
            }
            if (!this._isImgDrawn)
                return;
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (!this._isMarkerDrawn)
                {
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
                else
                    this.DrawMarker(Mouse.GetPosition((IInputElement)this._img));
            }
            else
            {
                if (!this._isMarkerDrawn)
                    return;
                this._tc.TrackEvent("Finish marker", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                this._tc.StopOperation<RequestTelemetry>(this._markerOp);
                System.Drawing.Rectangle markerRect = this.GetMarkerRect(this._markerStartPosition, Mouse.GetPosition((IInputElement)this._img));
                this.Markers.Add(markerRect);
                this._markerRect = (System.Windows.Shapes.Rectangle)null;
                this._isIssueEdited = true;
                this._isMarkerDrawn = false;
                this._isPictureEdited = true; 
            }
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


        private void _img_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void _img_MouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        public EditIssueDlg()
        {

        }

        private void removeMarkButton_Click(object sender, RoutedEventArgs e)
        {
            if (this._currentPicture == null)
            {
                return;
            }

            this.Markers.Clear();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void _okBtn_Click(object sender, RoutedEventArgs e)
        {
             
            RIDataModelContainer d = new RIDataModelContainer();

            for (int i = 0; i < this._pictureList.Items.Count; i++)
            {
                 var item = this._pictureList.Items[i];
                 PictureHolder pictureHolder = (PictureHolder)item;
                Picture picture = pictureHolder.Data as Picture;
               
                if (!SavePicture(picture))
                {
                    return;
                }

            }

            d.SaveChanges();
        }

        private void _cancelBtn_Click(object sender, RoutedEventArgs e)
        {
        }

        private void _pictureList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { 
            // if selected item is same as current not check
            if (e.AddedItems.Count > 0)
            {
                ListBoxItem i = e.AddedItems[0] as ListBoxItem;
                if ((i.DataContext as Picture) == this._currentPicture)
                {
                    return;
                }
            }

            if (!SavePicture(this._currentPicture))
            {
                return;
            }

            ListBoxItem item = _pictureList.Items[_pictureList.SelectedIndex] as ListBoxItem;
            Picture picture = item.DataContext as Picture;
            ShowPicture(picture);

            this._isPictureEdited = false;
        }

        private void ShowPicture(Picture picture)
        {
            this.ImgCanvas.Children.Clear();
            picture.Open();

            IntPtr hbitmap = picture.Bitmap.GetHbitmap();
            IntPtr palette = (IntPtr)0;
            Int32Rect sourceRect = new Int32Rect(0, 0, this.Bitmap.Width, this.Bitmap.Height);
            //            BitmapSizeOptions sizeOptions = BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height);

            System.Windows.Size renderSize = ImgCanvas.RenderSize;
            int width2 = (int)renderSize.Width;
            int height2 = (int)renderSize.Height;
            BitmapSizeOptions sizeOptions = BitmapSizeOptions.FromWidthAndHeight(width2, height2);
            BitmapSource sourceFromHbitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, palette, sourceRect, sizeOptions);


            this._img = new System.Windows.Controls.Image();
            this._img.Source = (ImageSource)sourceFromHbitmap;
            this.ImgCanvas.Children.Add(this._img);

            foreach (System.Drawing.Rectangle r in picture.Markers)
            {
                System.Drawing.Rectangle sr = ScaleMarker(r, false);
                System.Windows.Shapes.Rectangle markerRect = new System.Windows.Shapes.Rectangle();
                this._markerRect.Stroke = (System.Windows.Media.Brush)new SolidColorBrush(Colors.Red);
                this._markerRect.StrokeThickness = 2.0;
                Canvas.SetLeft((UIElement)markerRect, sr.X);
                Canvas.SetTop((UIElement)markerRect, sr.Y);
                this.ImgCanvas.Children.Add((UIElement)markerRect);
                this._isMarkerDrawn = true;

            }

            DeleteObject(hbitmap);
            this._isImgDrawn = true;
            this._isPictureEdited = true;


        }
    }
}
