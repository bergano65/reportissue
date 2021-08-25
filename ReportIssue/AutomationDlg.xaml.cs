using Microsoft.ApplicationInsights;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Management.Automation;
using System.Reflection;
using System.IO;
using System.Xml;

namespace ReportIssue
{
    public partial class AutomationDlg : Window
    {
        private TelemetryClient _tc;

        public string Output { get; set; }

        public AutomationDlg(System.Xml.XmlNode cfgNode)
        {
            this._tc = new TelemetryClient();
            this.InitializeComponent();
            this.DataContext = cfgNode;

            this.InitializeComponent();
            _PropertiesList.DataContext = cfgNode;
            _PropertiesList.BuildUU(cfgNode);
        }

        private void _okBtn_Click(object sender, RoutedEventArgs e)
        {
            this._tc.TrackEvent("Mail report choose submit", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                
            RunScript();
        }

        private void RunScript()
        {
            PowerShell powerShell = PowerShell.Create();

            Assembly assembly = Assembly.GetExecutingAssembly();
            string loc = assembly.Location;
            string directory = Path.GetDirectoryName(loc);
            XmlNode scriptNode = this.DataContext as XmlNode;

            string scriptLoc  = scriptNode.Attributes["src"].Value;
            string script = Path.Combine(directory, scriptLoc);
            string cmd = File.ReadAllText(script);

            powerShell.AddScript(cmd);
            var output = powerShell.Invoke();

            Output = output[0].ToString();
            
      }

        private void _cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
