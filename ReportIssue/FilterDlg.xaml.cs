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
    public partial class FilterDlg : Window
    {
        private TelemetryClient _tc;

        public Filter Filter { get; set; }

        public FilterDlg(Filter filter)
        {
            this._tc = new TelemetryClient();
            this._tc.TrackPageView("Filter Edit");
            this._tc.TrackEvent("Filter Edit", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this.Filter = filter;
            this.InitializeComponent();
            ((ListBoxItem)this._submittedComboBox.FindName(this.Filter.Submitted)).IsSelected = true;
            ((ListBoxItem)this._submittedComboBox.FindName(this.Filter.Fixed + "2")).IsSelected = true;
            this._productTextBox.Text = this.Filter.Product;
            this._issueTextBox.Text = this.Filter.Issue;
            this._wrongTextBox.Text = this.Filter.Wrong;
            this._rightTextBox.Text = this.Filter.Right;
        }

        private void _okBtn_Click(object sender, RoutedEventArgs e)
        {
            this._tc.TrackEvent("Filter Edit Submit", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this.Filter.Submitted = (string)(this._submittedComboBox.SelectedValue as ComboBoxItem).Content;
            this.Filter.Fixed = (string)(this._fixedComboBox.SelectedValue as ComboBoxItem).Content;
            this.Filter.Product = this._productTextBox.Text;
            this.Filter.Issue = this._issueTextBox.Text;
            this.Filter.Wrong = this._wrongTextBox.Text;
            this.Filter.Right = this._rightTextBox.Text;
            this.DialogResult = new bool?(true);
            this.Close();
        }

        private void _cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this._tc.TrackEvent("Filter Edit Cancel", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this.DialogResult = new bool?(false);
            this.Close();
        }
    }
}
