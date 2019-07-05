using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ReportIssue
{
    public partial class EditIssueDlg2 : Window
    {
        private bool _isImgDrawn;
        private bool _isIssueEdited;
        private bool _isMarkerDrawn;
        private IOperationHolder<RequestTelemetry> _markerOp;
        private System.Windows.Point _markerStartPosition;
        private System.Windows.Shapes.Rectangle _markerRect;
        private TelemetryClient _tc;
        private Window _parentWindow;
        private WindowState _parentWindowState;
        private WindowState _windowState;
        public Issue CurrentIssue;
        private System.Windows.Controls.Image _img;
        private System.Windows.Forms.Timer _timer;


        public List<System.Drawing.Rectangle> Markers { get; set; }

        public bool Saved { get; set; }

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public EditIssueDlg2(Window parent, Issue issue)
        {
            this._tc = new TelemetryClient();
            this._tc.TrackPageView("Edit Issue");
            this._tc.TrackEvent("Start edit issue", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this._parentWindow = parent;
            this.Markers = new List<System.Drawing.Rectangle>();
            this.InitializeComponent();
            this.CurrentIssue = issue;
            this._timer = new System.Windows.Forms.Timer();
            this._timer.Interval = 50;
            this._timer.Tick += new EventHandler(this._timer_Tick);
            this._timer.Start();
            this._isMarkerDrawn = false;
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (!this._isImgDrawn)
                return;
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (!this._isMarkerDrawn)
                {
                    System.Windows.Point p  = Mouse.GetPosition((IInputElement)this._img);
                    if (p.X >= 0 && p.X <= this._img.Width && p.Y >= 0 && p.Y <= this._img.Height) 
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
                    this._imgCanvas.Children.Add((UIElement)this._markerRect);
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
                Rect markerRect = this.GetMarkerRect(this._markerStartPosition, Mouse.GetPosition((IInputElement)this._img));
                this.Markers.Add(new System.Drawing.Rectangle((int)markerRect.X, (int)markerRect.Y, (int)markerRect.Width, (int)markerRect.Height));
                this._markerRect = (System.Windows.Shapes.Rectangle)null;
                this._isIssueEdited = true;
                this._isMarkerDrawn = false;
            }
        }

        private Rect GetMarkerRect(System.Windows.Point markerStartPosition, System.Windows.Point mousePosition)
        {
            return new Rect()
            {
                X = Math.Min(markerStartPosition.X, mousePosition.X),
                Y = Math.Min(markerStartPosition.Y, mousePosition.Y),
                Width = Math.Abs(markerStartPosition.X - mousePosition.X),
                Height = Math.Abs(markerStartPosition.Y - mousePosition.Y)
            };
        }

        private void NewIssue()
        {
            this._tc.TrackEvent("New Issue", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this._imgCanvas.Children.Clear();
            this._isIssueEdited = true;
            this._isMarkerDrawn = false;
            this.Saved = false;
        }

        private void PutCurrentIssueProperties()
        {
            this._issuePropsTabControl.PutIssueProperties(ref this.CurrentIssue);
        }

        private bool CheckCurrentIssueProperties()
        {
            return this._issuePropsTabControl.CheckIssueProperties();
        }

        private void shotButton_Click(object sender, RoutedEventArgs e)
        {
            this._tc.TrackEvent("Take Screenshot", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this._windowState = this.WindowState;
            this._parentWindowState = this._parentWindow.WindowState;
            this.WindowState = WindowState.Minimized;
            this._parentWindow.WindowState = WindowState.Minimized;
            System.Drawing.Rectangle bounds1 = Screen.PrimaryScreen.Bounds;
            int width1 = bounds1.Width;
            bounds1 = Screen.PrimaryScreen.Bounds;
            int height1 = bounds1.Height;
            Bitmap bitmap = new Bitmap(width1, height1, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics graphics = Graphics.FromImage((System.Drawing.Image)bitmap);
            System.Drawing.Rectangle bounds2 = Screen.PrimaryScreen.Bounds;
            int x = bounds2.X;
            bounds2 = Screen.PrimaryScreen.Bounds;
            int y = bounds2.Y;
            bounds2 = Screen.PrimaryScreen.Bounds;
            System.Drawing.Size size = bounds2.Size;
            graphics.CopyFromScreen(x, y, 0, 0, size, CopyPixelOperation.SourceCopy);
            IntPtr hbitmap = bitmap.GetHbitmap();
            IntPtr palette = (IntPtr)0;
            Int32Rect sourceRect = new Int32Rect(0, 0, bitmap.Width, bitmap.Height);
            System.Windows.Size renderSize = this._imgCanvas.RenderSize;
            int width2 = (int)renderSize.Width;
            renderSize = this._imgCanvas.RenderSize;
            int height2 = (int)renderSize.Height;
            BitmapSizeOptions sizeOptions = BitmapSizeOptions.FromWidthAndHeight(width2, height2);
            BitmapSource sourceFromHbitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, palette, sourceRect, sizeOptions);
            this._img = new System.Windows.Controls.Image();
            this._img.Source = (ImageSource)sourceFromHbitmap;
            this._isImgDrawn = true;
            if (this._imgCanvas.Children.Count > 0 && this._imgCanvas.Children[0] is System.Windows.Controls.Image)
                this._imgCanvas.Children.RemoveAt(0);
            this._imgCanvas.Children.Insert(0, (UIElement)this._img);
            Canvas.SetLeft((UIElement)this._img, 0.0);
            Canvas.SetRight((UIElement)this._img, 0.0);
            EditIssueDlg2.DeleteObject(hbitmap);
            this._isImgDrawn = true;
            this.CurrentIssue.Picture = bitmap;
            this._parentWindow.WindowState = this._parentWindowState;
            this.WindowState = this._windowState;
        }

        private void removeMarkButton_Click(object sender, RoutedEventArgs e)
        {
            this._tc.TrackEvent("Remove markers", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            if (!this._isImgDrawn)
                return;
            this.Markers.Clear();
            for (int index = this._imgCanvas.Children.Count - 1; index > 0; --index)
                this._imgCanvas.Children.RemoveAt(index);
            this._isIssueEdited = true;
        }

        private void ShowIssue()
        {
            this._tc.TrackEvent("Show Issue", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this._isIssueEdited = false;
            this.CurrentIssue.Open();
            this._imgCanvas.Children.Clear();
            Bitmap picture = this.CurrentIssue.Picture;
            IntPtr hbitmap = picture.GetHbitmap();
            IntPtr palette = (IntPtr)0;
            Int32Rect sourceRect = new Int32Rect(0, 0, picture.Width, picture.Height);
            System.Windows.Size renderSize = this._imgCanvas.RenderSize;
            int width = (int)renderSize.Width;
            renderSize = this._imgCanvas.RenderSize;
            int height = (int)renderSize.Height;
            BitmapSizeOptions sizeOptions = BitmapSizeOptions.FromWidthAndHeight(width, height);
            BitmapSource sourceFromHbitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, palette, sourceRect, sizeOptions);
            this._img = new System.Windows.Controls.Image();
            this._img.Source = (ImageSource)sourceFromHbitmap;
            this._imgCanvas.Children.Insert(0, (UIElement)this._img);
            Canvas.SetLeft((UIElement)this._img, 0.0);
            Canvas.SetRight((UIElement)this._img, 0.0);
            EditIssueDlg2.DeleteObject(hbitmap);
            this._isMarkerDrawn = false;
            this._isImgDrawn = true;
            foreach (System.Drawing.Rectangle marker in this.CurrentIssue.Markers)
            {
                double num1 = this._imgCanvas.RenderSize.Width / (double)picture.Width;
                double num2 = this._imgCanvas.RenderSize.Height / (double)picture.Height;
                System.Drawing.Rectangle rectangle1 = new System.Drawing.Rectangle((int)((double)marker.X * num1), (int)((double)marker.Y * num2), (int)((double)marker.Width * num1), (int)((double)marker.Height * num2));
                this.Markers.Add(rectangle1);
                System.Windows.Shapes.Rectangle rectangle2 = new System.Windows.Shapes.Rectangle();
                rectangle2.Stroke = (System.Windows.Media.Brush)new SolidColorBrush(Colors.Red);
                rectangle2.Fill = (System.Windows.Media.Brush)System.Windows.Media.Brushes.Transparent;
                rectangle2.StrokeThickness = 2.0;
                Canvas.SetLeft((UIElement)rectangle2, (double)rectangle1.X);
                Canvas.SetTop((UIElement)rectangle2, (double)rectangle1.Y);
                rectangle2.Width = (double)rectangle1.Width;
                rectangle2.Height = (double)rectangle1.Height;
                this._imgCanvas.Children.Add((UIElement)rectangle2);
            }
            this._issuePropsTabControl.GetIssueProperties(this.CurrentIssue);
        }

        private void SelectComboBoxItem(System.Windows.Controls.ComboBox comboBox, string value)
        {
            this._tc.TrackEvent("Select combo box item", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            foreach (ComboBoxItem comboBoxItem in (IEnumerable)comboBox.Items)
            {
                if ((string)comboBoxItem.Content == value)
                {
                    comboBoxItem.IsSelected = true;
                    break;
                }
            }
        }

        private bool SaveIssue()
        {
            this._tc.TrackEvent("Save Issue", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            if (!this.CheckCurrentIssueProperties())
            {
                int num = (int)System.Windows.MessageBox.Show("All values should be not empty", "Issue report");
                return false;
            }
            if (!this._isImgDrawn)
            {
                int num = (int)System.Windows.MessageBox.Show("Picture should be not empty", "Issue report");
                return false;
            }
            if (this.Markers.Count == 0)
            {
                int num = (int)System.Windows.MessageBox.Show("At least one marker should be present", "Issue report");
                return false;
            }
            this.PutCurrentIssueProperties();
            this.CurrentIssue.Markers.Clear();
            foreach (System.Drawing.Rectangle marker in this.Markers)
            {
                Bitmap picture = this.CurrentIssue.Picture;
                double width1 = (double)picture.Width;
                System.Windows.Size renderSize = this._imgCanvas.RenderSize;
                double width2 = renderSize.Width;
                double num1 = width1 / width2;
                double height1 = (double)picture.Height;
                renderSize = this._imgCanvas.RenderSize;
                double height2 = renderSize.Height;
                double num2 = height1 / height2;
                this.CurrentIssue.Markers.Add(new System.Drawing.Rectangle((int)((double)marker.X * num1), (int)((double)marker.Y * num2), (int)((double)marker.Width * num1), (int)((double)marker.Height * num2)));
            }
            this.CurrentIssue.Save();
            this._isMarkerDrawn = false;
            this._isIssueEdited = false;
            return true;
        }

        private void DrawMarker(System.Windows.Point mousePosition)
        {
            this._tc.TrackEvent("Draw Marker", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            Rect markerRect = this.GetMarkerRect(this._markerStartPosition, mousePosition);
            Canvas.SetLeft((UIElement)this._markerRect, markerRect.X);
            Canvas.SetTop((UIElement)this._markerRect, markerRect.Y);
            this._markerRect.Width = markerRect.Width;
            this._markerRect.Height = markerRect.Height;
        }

        private void _okBtn_Click(object sender, RoutedEventArgs e)
        {
            this._tc.TrackEvent("Save Issue", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            if (!this.SaveIssue())
                return;
            this.Saved = true;
            this.Close();
        }

        private void _cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this._tc.TrackEvent("Cancel Issue", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this.Saved = false;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.CurrentIssue != null)
            {
                this._issuePropsTabControl.EditIssue = this.CurrentIssue;
                this.ShowIssue();
            }
            else
            {
                this.CurrentIssue = new Issue();
                this._issuePropsTabControl.EditIssue = this.CurrentIssue;
                this.NewIssue();
            }
        }

    }
}
