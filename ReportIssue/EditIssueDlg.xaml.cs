using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

using ReportIssueUtilities;

namespace ReportIssue
{

    /// <summary>2
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
        private TextBlock _markerTxtBlock;

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
            this.CurrentIssue = issue;
            
           _errorGrid.ItemsSource = new ObservableCollection<Error>();

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
            this._tc.TrackEvent("Show Issue", (IDictionary<string, string>)null, (IDictionary<string, double>)null);

            this.ImgCanvas.Children.Clear();
            this._pictureListView.Items.Clear();
//            this._pictureList.Items.Clear();

            this._isIssueEdited = false;
            this._issuePropsTabControl.GetIssueProperties(this.CurrentIssue);

            this.CurrentIssue.Open();
            foreach (Picture p in this.CurrentIssue.Pictures)
            {
                System.Windows.Controls.ListViewItem item = new System.Windows.Controls.ListViewItem();
//                ListBoxItem item = new ListBoxItem();
                item.Content = p.Name;
                item.DataContext = p;
                this._pictureListView.Items.Add(item);
//                this._pictureList.Items.Add(item);
            }

//            this._pictureList.SelectedIndex = 0;
            this._pictureListView.SelectedIndex = 0;

            _errorGrid.ItemsSource = ReportIssueUtilities.ReportIssueUtilities.DecodeErrors(this.CurrentIssue.Wrong);
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
            if (this._currentPicture != null)
          {
                if (!SavePicture(this._currentPicture))
                {
                    return;
                }

            }

            RIDataModelContainer d = new RIDataModelContainer();

            Picture p = new Picture();
            p.IsOpened = true;

            System.Windows.Controls.ListViewItem item = new System.Windows.Controls.ListViewItem();
            /*
            System.Windows.Controls.ListBoxItem item = new System.Windows.Controls.ListBoxItem();
             *p.Name = string.Format("Picture - {0}", _pictureList.Items.Count + 1);
                        item.Content = p.Name;
                        item.DataContext = p;
                        _pictureList.Items.Add(item);
                        _pictureList.SelectedIndex = _pictureList.Items.Count - 1;
              */
            p.Name = string.Format("Picture - {0}", _pictureListView.Items.Count + 1);
            item.Content = p.Name;
            item.DataContext = p;
            _pictureListView.Items.Add(item);
            _pictureListView.SelectedIndex = _pictureListView.Items.Count - 1;

        }

        private void removePictureButton_Click(object sender, RoutedEventArgs e)
        {
            if (this._currentPicture == null)
            {
                return;

            }

            /*
             *if (_pictureList.Items.Count == 0)
                        {
                            return;
                        }

                         if (_pictureList.SelectedIndex != -1)
                         {
                            ListBoxItem i = (ListBoxItem)_pictureList.Items[_pictureList.SelectedIndex] as ListBoxItem;
                            _pictureList.Items.Remove(i);
                         }

                        if (_pictureList.Items.Count > 0)
                        {
                            _pictureList.SelectedIndex = 0;
                        }
                        else
                        {
                            this._currentPicture = null;
                            this._pictureList.SelectedIndex = -1;
                        }
              */

            if (_pictureListView.Items.Count == 0)
            {
                return;
            }

            if (_pictureListView.SelectedIndex != -1)
            {
                System.Windows.Controls.ListViewItem i = (ListBoxItem)_pictureListView.Items[_pictureListView.SelectedIndex] as System.Windows.Controls.ListViewItem;
//                ListBoxItem i = (ListBoxItem)_pictureListView.Items[_pictureListView.SelectedIndex] as ListBoxItem;
                _pictureListView.Items.Remove(i);
            }

            if (_pictureListView.Items.Count > 0)
            {
                _pictureListView.SelectedIndex = 0;
            }
            else
            {
                this._currentPicture = null;
                this._pictureListView.SelectedIndex = -1;
            }

        }

        private System.Drawing.Rectangle ScaleMarker(Bitmap bitmap, System.Drawing.Rectangle from, bool toBitmap)
        {
            double dX = (double)bitmap.Width/(double)805;
            double dY = (double)bitmap.Height / (double) 750;
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
            if (picture == null)
            {
                return true;
            }

            if (picture.Bitmap == null)
            {
//                this._pictureList.SelectedIndex = FindPicture(picture);
                this._pictureListView.SelectedIndex = FindPicture(picture);
                System.Windows.MessageBox.Show("No screen shot defined", "Report Issue");
                return false;
            }


            if (picture.Markers.Count == 0)
            {
//                this._pictureList.SelectedIndex = FindPicture(picture);
                this._pictureListView.SelectedIndex = FindPicture(picture);
                System.Windows.MessageBox.Show("No markers defined", "Report Issue");
                return false;
            }

            picture.Save();

            return true;
        }

        private int FindPicture(Picture picture)
        {
            int num = 0;
            foreach (System.Windows.Controls.ListBoxItem item in
                (this._pictureListView.Items))
//                (this._pictureList.Items))
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

            this._currentPicture.Bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics graphics = Graphics.FromImage((System.Drawing.Image)this._currentPicture.Bitmap);
            graphics.CopyFromScreen(x, y, 0, 0, size, CopyPixelOperation.SourceCopy);

            IntPtr hbitmap = this._currentPicture.Bitmap.GetHbitmap();
            IntPtr palette = (IntPtr)0;
            Int32Rect sourceRect = new Int32Rect(0, 0, this._currentPicture.Bitmap.Width, this._currentPicture.Bitmap.Height);
            //            BitmapSizeOptions sizeOptions = BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height);

            System.Windows.Size renderSize = ImgCanvas.RenderSize;
            int width2 = 805; // (int)renderSize.Width;
            int height2 = 750; // (int)renderSize.Height;
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
            {
                return;
            }

            System.Windows.Point pos = Mouse.GetPosition(relativeTo: this.ImgCanvas);
            _mousePositionTxtBlock.Text = string.Format("{0}:{1}", pos.X, pos.Y);

            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (pos.X <= 0 || pos.X > 805 || pos.Y <= 0 || pos.Y > 750 )
                {   
                    return;
                }

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

                    _markerTxtBlock = new TextBlock();
                    _markerTxtBlock.Foreground = System.Windows.Media.Brushes.Green;
                    Canvas.SetLeft((UIElement)this._markerTxtBlock, this._markerStartPosition.X + 5);
                    Canvas.SetTop((UIElement)this._markerTxtBlock, this._markerStartPosition.Y + 5);
                        ImgCanvas.Children.Add(this._markerTxtBlock);

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
                Marker marker = this.GetMarker(this._markerStartPosition, Mouse.GetPosition((IInputElement)this._img));
//               this._currentPicture.Markers.Add(ScaleMarker(this._currentPicture.Bitmap, markerRect, true));
           this._currentPicture.Markers.Add(marker);
               this._markerRect = (System.Windows.Shapes.Rectangle)null;
                this._isIssueEdited = true;
                this._isMarkerDrawn = false;
                this._isPictureEdited = true; 
            }
        }

    private void DrawMarker(System.Windows.Point mousePosition)
    {
        this._tc.TrackEvent("Draw Marker", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
        Marker marker = this.GetMarker(this._markerStartPosition, mousePosition);
        Canvas.SetLeft((UIElement)this._markerRect, marker.Left);
        Canvas.SetTop((UIElement)this._markerRect, marker.Top);
        this._markerRect.Width = marker.Width;
        this._markerRect.Height = marker.Height;

            // draw text
            _markerTxtBlock.Text = string.Format("{0},{1}:{2},{3}", marker.Top, marker.Left, marker.Width, marker.Height);

        }

        private Marker GetMarker(System.Windows.Point markerStartPosition, System.Windows.Point mousePosition)
        {
            return new Marker()
            {
                Left = (int)Math.Min(markerStartPosition.X, mousePosition.X),
                Top = (int)Math.Min(markerStartPosition.Y, mousePosition.Y),
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

            this._currentPicture.Markers.Clear();
            List<object> list = new List<object>();
            foreach (var c in ImgCanvas.Children)
            {
                if (c is TextBlock || c is System.Windows.Shapes.Rectangle)
                {
                    list.Add(c);
                }
            }

            foreach (var c in list)
            {
                this.ImgCanvas.Children.Remove((UIElement)c);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void _okBtn_Click(object sender, RoutedEventArgs e)
        {
            this._tc.TrackEvent("Save Issue", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            if (!this.SaveIssue())
                return;
            this.Saved = true;
            Close();
        }

        private void PutCurrentIssueProperties()
        {
            this._issuePropsTabControl.PutIssueProperties(ref this.CurrentIssue);
        }

        private bool CheckCurrentIssueProperties()
        {
            return this._issuePropsTabControl.CheckIssueProperties();
        }

        private bool SaveIssue()
        {
            this._tc.TrackEvent("Save Issue", (IDictionary<string, string>)null, (IDictionary<string, double>)null);

            if (!this.CheckCurrentIssueProperties())
            {
                int num = (int)System.Windows.MessageBox.Show("All values should be not empty", "Issue report");
                return false;
            }

            this.PutCurrentIssueProperties();
            RIDataModelContainer d = new RIDataModelContainer();

            this.CurrentIssue.Wrong = ReportIssueUtilities.ReportIssueUtilities.EncodeErrors(_errorGrid.ItemsSource as ObservableCollection<Error>);

            this.CurrentIssue.Pictures.Clear();
            for (int i = 0; i < this._pictureListView.Items.Count; i++)
            {
                System.Windows.Controls.ListViewItem item = _pictureListView.Items[i] as System.Windows.Controls.ListViewItem;
                Picture picture = (Picture)item.DataContext;

                this.CurrentIssue.Pictures.Add(picture);
                if (!SavePicture(picture))
                {
                    return false;
                }

                d.Pictures.AddOrUpdate(picture);
                foreach (Marker m in picture.Markers)
                {
                    d.Markers.AddOrUpdate(m);
                }
            }

            /*
             *for (int i = 0; i < this._pictureList.Items.Count; i++)
                        {
                             ListBoxItem item = this._pictureList.Items[i] as ListBoxItem;
                             Picture picture = (Picture)item.DataContext;

                             this.CurrentIssue.Pictures.Add(picture);
                             d.Pictures.AddOrUpdate(picture);
                             if (!SavePicture(picture))
                             {
                                return false;
                             }

                        }
            */
            this.CurrentIssue.Save();
            d.Issues.AddOrUpdate(this.CurrentIssue);

            d.SaveChanges();
            return true;
        }

        private void _cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void _pictureList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { 
            // if selected item is same as current not check
            if (e.AddedItems.Count > 0)
            {
                System.Windows.Controls.ListViewItem i = e.AddedItems[0] as System.Windows.Controls.ListViewItem;
//                ListBoxItem i = e.AddedItems[0] as ListBoxItem;
                if ((i.DataContext as Picture) == this._currentPicture)
                {
                    return;
                }
            }

            if (!SavePicture(this._currentPicture))
            {
                return;
            }

            System.Windows.Controls.ListViewItem item = _pictureListView.Items[_pictureListView.SelectedIndex] as System.Windows.Controls.ListViewItem;
//            ListBoxItem item = _pictureList.Items[_pictureList.SelectedIndex] as ListBoxItem;
            Picture picture = item.DataContext as Picture;
            this._currentPicture = picture;

            ShowPicture(picture);

            this._isPictureEdited = false;
        }

        private void ShowPicture(Picture picture)
        {
//            this._currentPicture = picture;

            this.ImgCanvas.Children.Clear();

            if (picture == null)
            {
                return;
            }

            picture.Open();

            if (picture.Bitmap == null)
            {
              return;
            }

            TextBlock t = new TextBlock();
            t.Foreground = System.Windows.Media.Brushes.Green;
            Canvas.SetLeft((UIElement)t, 0);
            Canvas.SetTop((UIElement)t, 0);
            ImgCanvas.Children.Add(t);
            t.Text = "~";


            IntPtr hbitmap = picture.Bitmap.GetHbitmap();
            IntPtr palette = (IntPtr)0;
            Int32Rect sourceRect = new Int32Rect(0, 0, picture.Bitmap.Width, picture.Bitmap.Height);
            //            BitmapSizeOptions sizeOptions = BitmapSizeOptions.FromWidthAndHeight(bitmap.Width, bitmap.Height);

            System.Windows.Size renderSize = ImgCanvas.RenderSize;
            int width2 = 805; // (int)renderSize.Width;
            int height2 = 750; //  (int)renderSize.Height;
            BitmapSizeOptions sizeOptions = BitmapSizeOptions.FromWidthAndHeight(width2, height2);
            BitmapSource sourceFromHbitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hbitmap, palette, sourceRect, sizeOptions);


            this._img = new System.Windows.Controls.Image();
            this._img.Source = (ImageSource)sourceFromHbitmap;
            this.ImgCanvas.Children.Add(this._img);

            foreach (Marker m in picture.Markers)
            {
//                System.Drawing.Rectangle sr = ScaleMarker(picture.Bitmap, r, false);
                System.Windows.Shapes.Rectangle markerRect = new System.Windows.Shapes.Rectangle();
                markerRect.Stroke = (System.Windows.Media.Brush)new SolidColorBrush(Colors.Red);
                markerRect.StrokeThickness = 2.0;
                markerRect.Width = m.Width;
                markerRect.Height = m.Height;
                Canvas.SetLeft((UIElement)markerRect, m.Left);
                Canvas.SetTop((UIElement)markerRect, m.Top);
                Canvas.SetRight((UIElement)markerRect, m.Left + m.Width);
                Canvas.SetBottom((UIElement)markerRect, m.Top + m.Height);
                this.ImgCanvas.Children.Add((UIElement)markerRect);

                TextBlock markerTxtBlock = new TextBlock();
                markerTxtBlock.Foreground = System.Windows.Media.Brushes.Green;
                Canvas.SetLeft((UIElement)markerTxtBlock, m.Left + 5);
                Canvas.SetTop((UIElement)markerTxtBlock, m.Top + 5);
                markerTxtBlock.Text = string.Format("{0},{1}:{2},{3}", m.Left, m.Top, m.Width, m.Height);
                ImgCanvas.Children.Add(markerTxtBlock);
            }

            DeleteObject(hbitmap);
            this._isImgDrawn = true;
            this._isPictureEdited = true;


        }
    }
}
 