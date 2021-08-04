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
    public partial class AutomationPropertiesList : UserControl
    {
        public AutomationPropertiesList()
        {
            InitializeComponent();
        }

        public void PutIssueParameter(XmlNode parameterNode, ref Issue issue)
        {
            string name = string.Format("Parameter{0}", (object)parameterNode.Attributes["num"].Value.ToString());
            string innerText;
            if (parameterNode.Attributes["type"].Value.ToString() == "combo")
            {
                int index = int.Parse(parameterNode.SelectSingleNode("selected").InnerText);
                innerText = parameterNode.SelectNodes("value")[index].Attributes["name"].Value;
            }
            else
                innerText = parameterNode.SelectSingleNode("value").InnerText;
            issue.GetType().GetProperty(name).SetValue((object)issue, (object)innerText);
        }

        public void GetIssueParameter(XmlNode parameterNode, int parameterNum, string parameterValue)
        {
            string str = parameterNode.Attributes["type"].Value;
            XmlDocument ownerDocument = (this.DataContext as XmlElement).OwnerDocument;
            if (str == "combo")
            {
                XmlNode xmlNode = parameterNode.SelectSingleNode("selected");
                if (xmlNode == null)
                {
                    XmlElement element = ownerDocument.CreateElement("selected");
                    parameterNode.AppendChild((XmlNode)element);
                    xmlNode = (XmlNode)element;
                }
                int num = 0;
                foreach (XmlNode selectNode in parameterNode.SelectNodes("value"))
                {
                    if (selectNode.Attributes["name"].Value.ToString() == parameterValue)
                        xmlNode.InnerText = num.ToString();
                    ++num;
                }
            }
            else
            {
                parameterNode.InnerXml = string.Empty;
                XmlElement element = ownerDocument.CreateElement("value");
                XmlCDataSection cdataSection = ownerDocument.CreateCDataSection("");
                cdataSection.InnerText = parameterValue;
                element.AppendChild((XmlNode)cdataSection);
                parameterNode.AppendChild((XmlNode)element);
            }
        }

        public bool CheckIssueProperties()
        {
            int count = this._propertyList.Items.Count;
            for (int index = 0; index < count; ++index)
            {
                XmlNode xmlNode = this._propertyList.Items[index] as XmlNode;
                if (xmlNode.Attributes["type"].Value == "combo")
                {
                    if (xmlNode.SelectSingleNode("selected").InnerText == "")
                        return false;
                }
                else
                {
                    string innerText = xmlNode.SelectSingleNode("value").InnerText;
                    if (innerText == "" || innerText == "-1")
                        return false;
                }
            }
            return true;
        }

        public void AddItem(XmlNode itemNode)
        {
            this._propertyList.Items.Add((object)itemNode);
        }

    }
}
