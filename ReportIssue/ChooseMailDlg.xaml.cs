using Microsoft.ApplicationInsights;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace ReportIssue
{
    public partial class ChooseMailDlg : Window
    {
        private TelemetryClient _tc;

        public string Mail { get; set; }

        public ChooseMailDlg()
        {
            this._tc = new TelemetryClient();
            this.Mail = "";
            this._tc.TrackPageView("Choose mail for report");
            this._tc.TrackEvent("Choose mail for report", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this.InitializeComponent();
            this.DataContext = (object)this;
        }

        private void _okBtn_Click(object sender, RoutedEventArgs e)
        {
            this._tc.TrackEvent("Mail report choose submit", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            if (this.Mail == "" || !this.Mail.Contains("@"))
            {
                int num = (int)MessageBox.Show("Mail can't be empty and should contain '@'", "Issue report");
            }
            else
            {
                this.DialogResult = new bool?(true);
                this.Close();
            }
        }

        private void _cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this._tc.TrackEvent("Mail report choose cancel", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this.DialogResult = new bool?(false);
            this.Close();
        }
    }
}
