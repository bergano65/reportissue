using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Xml;

namespace ReportIssue
{
    /// <summary>
    /// Interaction logic for IssuePropertiesList.xaml
    /// </summary>
    public partial class AutomationPropertiesList : ListView
    {
        public AutomationPropertiesList()
        {
            InitializeComponent();
        }


        public void BuildUU(XmlNode cfgNode)
        {
            this.Items.Clear();
            XmlDocument ownerDocument = cfgNode.OwnerDocument;
            XmlNodeList parameters = cfgNode.SelectNodes("parameters/parameter");
            foreach (XmlNode par in parameters)
            {

                this.Items.Add(par);
                string type = par.Attributes["type"].Value;
                if (type == "combo")
                {
                    XmlNode element = (XmlNode)ownerDocument.CreateElement("selected");
                    element.InnerText = "0";
                    par.AppendChild(element);
                }
                else
                {
                    XmlNode element = (XmlNode)ownerDocument.CreateElement("value");
                    par.AppendChild(element);
                }
            }
        }

    }
}