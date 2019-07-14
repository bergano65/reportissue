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
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Data.Entity.Migrations;
using System.Data.Entity.Validation;

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
            Picture picture = new Picture();
            this.CurrentIssue.Pictures.Add(picture);
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
           
            EnsurePictureSelection();
      
            this._tc.TrackEvent("Take Screenshot", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            int selIndex =
                _pictureTabControl.SelectedIndex != -1 ?
                _pictureTabControl.SelectedIndex : 0;

            ShotPanel shotPanel = (_pictureTabControl.Items[selIndex] as TabItem).Content as ShotPanel;

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

            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics graphics = Graphics.FromImage((System.Drawing.Image)bitmap);
            graphics.CopyFromScreen(x, y, 0, 0, size, CopyPixelOperation.SourceCopy);

            shotPanel.SetPicture(bitmap);

            if (this._parentWindow != null)
            {
                this._parentWindow.WindowState = this._parentWindowState;
            }

            this.WindowState = this._windowState;
            this.Activate();

    }

        private void removeMarkButton_Click(object sender, RoutedEventArgs e)
        {
            this._tc.TrackEvent("Remove markers", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            if (_pictureTabControl.Items.Count == 0)
            {
                return;            }
            if (_pictureTabControl.Items.Count == 1)

            {
                _pictureTabControl.SelectedIndex = 0;
            }

            ShotPanel panel = (_pictureTabControl.SelectedItem as TabItem).Content as ShotPanel;
            panel.RemoveMarkers();

            this._isIssueEdited = true;
        }

        private void ShowIssue()
        {
            this._tc.TrackEvent("Show Issue", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this._isIssueEdited = false;
            this.CurrentIssue.Open();
            this._issuePropsTabControl.GetIssueProperties(this.CurrentIssue);
            ShowPictures();
        }

       private void ShowPictures()
       {
            int picNum = 1;
            RIDataModelContainer d = new RIDataModelContainer();
            string[] pictureIds = this.CurrentIssue.PictureString.Split(',');
            foreach(string id in pictureIds)
            {
                Picture p = d.Pictures.Find(id);
                ShotPanel shotPanel = new ShotPanel();

                TabItem tabItem = new TabItem();
                tabItem.Content = shotPanel;
                _pictureTabControl.Items.Add(tabItem);
                _pictureTabControl.SelectedIndex = _pictureTabControl.Items.Count - 1;
                _pictureTabControl.SelectionChanged += _pictureTabControl_SelectionChanged;
                tabItem.Header = string.Format("Picture {0}", picNum++);
                shotPanel.Picture = p;
                shotPanel.Open();
            } 
       }

        private void _pictureTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem tabItem = _pictureTabControl.SelectedItem as TabItem;
            ShotPanel panel = tabItem.Content as ShotPanel;
            panel.Open();
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
            try
            {

                this._tc.TrackEvent("Save Issue", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                if (!this.CheckCurrentIssueProperties())
                {
                    int num = (int)System.Windows.MessageBox.Show("All values should be not empty", "Issue report");
                    return false;
                }

                if (_pictureTabControl.Items.Count == 0)
                {
                    int num = (int)System.Windows.MessageBox.Show("Add at least one picture", "Issue report");
                    return false;
                }


                foreach (TabItem i in _pictureTabControl.Items)
                {
                    ShotPanel shotPanel = i.Content as ShotPanel;
                    string msg;
                    if (!shotPanel.IsValid(out msg))
                    {
                        System.Windows.MessageBox.Show(msg, "ReportIssue", MessageBoxButton.OK);
                        return false;
                    }
                }

                this.PutCurrentIssueProperties();

                this.CurrentIssue.Pictures.Clear();
                StringBuilder stringBuilder = new StringBuilder();
                foreach (TabItem i in _pictureTabControl.Items)
                {
                    ShotPanel shotPanel = i.Content as ShotPanel;
                    shotPanel.Save();
                    this.CurrentIssue.Pictures.Add(shotPanel.Picture);

                    if (_pictureTabControl.Items.IndexOf(i) > 0)
                    {
                        stringBuilder.AppendFormat(",");
                    }

                    stringBuilder.AppendFormat("{0}", shotPanel.Picture.ID);
                }

                this.CurrentIssue.PictureString = stringBuilder.ToString();
                this.CurrentIssue.Save();

                RIDataModelContainer d = new RIDataModelContainer();
                foreach (Picture p in this.CurrentIssue.Pictures)
                {
                    d.Pictures.AddOrUpdate(p);
                }

                d.Issues.AddOrUpdate(this.CurrentIssue);
                d.SaveChanges();
                this._isIssueEdited = false;
                return true;
            }
            catch (DbEntityValidationException e)
            {
                foreach (DbEntityValidationResult r in e.EntityValidationErrors)
                {   
                    foreach (var err in r.ValidationErrors)
                    {
                        Console.WriteLine(err.PropertyName);
                        Console.WriteLine(err.ErrorMessage);
                    }
                }
            }

            return false;

        }

        private void EnsurePictureSelection()
        {     
            foreach (TabItem i in _pictureTabControl.Items)
  {

                ShotPanel panel = i.Content as ShotPanel;
                if (panel.IsOpened())
                {
                    _pictureTabControl.SelectedItem = i;
                }
            }
        } 

        private void _okBtn_Click(object sender, RoutedEventArgs e)
        {
            EnsurePictureSelection();
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
