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
        private bool _isIssueEdited;
        private TelemetryClient _tc;
        private Window _parentWindow;
        private WindowState _parentWindowState;
        private WindowState _windowState;
        public Issue CurrentIssue;

        public bool Saved { get; set; }

        public EditIssueDlg2(Window parent, Issue issue)
        {
            this._tc = new TelemetryClient();
            this._tc.TrackPageView("Edit Issue");
            this._tc.TrackEvent("Start edit issue", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this._parentWindow = parent;
            this.InitializeComponent();
            this.CurrentIssue = issue;
        }

        private void NewIssue()
        {
            this._tc.TrackEvent("New Issue", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this._isIssueEdited = true;
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

        private void removePictureButton_Click(object sender, RoutedEventArgs e)
        {
            if (_pictureTabControl.SelectedIndex != -1)
            {
                _pictureTabControl.Items.RemoveAt(_pictureTabControl.SelectedIndex);
            }

            for (int i = 0; i < _pictureTabControl.Items.Count; i++)
            {
                (_pictureTabControl.Items[i] as TabItem).Header = string.Format("Picture {0}", i + 1);
            }
        }

        private void addPictureButton_Click(object sender, RoutedEventArgs e)
        {
            TabItem item = new TabItem();
            item.Content = new ShotPanel();
            item.Header = string.Format("Picture {0}", _pictureTabControl.Items.Count + 1);
            _pictureTabControl.Items.Add(item);
            _pictureTabControl.SelectedIndex = _pictureTabControl.Items.Count - 1;
        }

        private void shotButton_Click(object sender, RoutedEventArgs e)
        {
            if (_pictureTabControl.Items.Count == 0 || 
                (_pictureTabControl.SelectedIndex == -1 && _pictureTabControl.Items.Count > 1))
            { 
                return;
            }
           
            this._tc.TrackEvent("Take Screenshot", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            int selIndex =
                _pictureTabControl.SelectedIndex != -1 ?
                _pictureTabControl.SelectedIndex : 0;

            ShotPanel shotPanel = (_pictureTabControl.Items[selIndex] as TabItem).Content as ShotPanel;

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

            shotPanel.SetPicture(bitmap);

            this._parentWindow.WindowState = this._parentWindowState;
            this.WindowState = this._windowState;

    }

        private void removeMarkButton_Click(object sender, RoutedEventArgs e)
        {
            /*
                        this._tc.TrackEvent("Remove markers", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                        if (!this._isImgDrawn)
                            return;
                        this.Markers.Clear();
                        for (int index = this._imgCanvas.Children.Count - 1; index > 0; --index)
                            this._imgCanvas.Children.RemoveAt(index);
                        this._isIssueEdited = true;
            */
        }

        private void ShowIssue()
        {
            /*
            this._tc.TrackEvent("Show Issue", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this._isIssueEdited = false;
            this.CurrentIssue.Open();
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
            */
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
            return true;
            /*
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
            */
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
