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
    public partial class AutomationDlg : Window
    {
        private TelemetryClient _tc;

        public string Output { get; set; }

        public AutomationDlg()
        {
            this._tc = new TelemetryClient();
            this.InitializeComponent();
            this.DataContext = (object)this;
        }

        private void _okBtn_Click(object sender, RoutedEventArgs e)
        {
            this._tc.TrackEvent("Mail report choose submit", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
        }

        private void _cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
