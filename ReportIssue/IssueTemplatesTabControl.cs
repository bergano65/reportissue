using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace ReportIssue
{
    public class IssueTemplatesTabControl : TabControl
    {
        private XmlDocument _templateDoc;
        public Issue EditIssue;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.BuildTemplates();
        }

        public void PutIssueProperties(ref Issue issue)
        {
            IssuePropertiesList selectedContent = this.SelectedContent as IssuePropertiesList;
            issue.Template = (selectedContent.DataContext as XmlElement).SelectSingleNode("type").InnerText;
            selectedContent.PutIssueProperties(ref issue);
        }

        public void GetIssueProperties(Issue issue)
        {
            bool flag = false;
            if (issue.Template == "")
            {
                this.SelectedIndex = 0;
            }
            else
            {
                this.SelectedIndex = 0;
                for (int index = 0; index < this.Items.Count; ++index)
                {
                    if (((this.Items[index] as TabItem).DataContext as XmlNode).SelectSingleNode("type").InnerText == issue.Template)
                    {
                        flag = true;
                        this.SelectedIndex = index;
                        break;
                    }
                }
            }
            if (flag)
            {
                (this.SelectedContent as IssuePropertiesList).GetIssueProperties(issue);
            }
            else
            {
                int num = (int)MessageBox.Show(string.Format("No template '{0}' found", (object)issue.Template));
            }
        }

        public bool CheckIssueProperties()
        {
            return (this.SelectedContent as IssuePropertiesList).CheckIssueProperties();
        }

        private void BuildTemplates()
        {
            string filename = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "IssueType.xml");
            this._templateDoc = new XmlDocument();
            this._templateDoc.Load(filename);
            foreach (XmlNode selectNode1 in this._templateDoc.SelectNodes("//issue"))
            {
                TabItem tabItem = new TabItem();
                tabItem.Header = (object)selectNode1.SelectSingleNode("alias").InnerText;
                tabItem.DataContext = (object)selectNode1;
                this.AddChild((object)tabItem);
                IssuePropertiesList issuePropertiesList = new IssuePropertiesList();
                tabItem.Content = (object)issuePropertiesList;
                foreach (XmlNode selectNode2 in selectNode1.SelectNodes("parameters/parameter"))
                {
                    issuePropertiesList.AddItem(selectNode2);
                    XmlDocument ownerDocument = selectNode2.OwnerDocument;
                    if (selectNode2.Attributes["type"].Value == "combo")
                    {
                        XmlNode element = (XmlNode)ownerDocument.CreateElement("selected");
                        element.InnerText = "0";
                        selectNode2.AppendChild(element);
                    }
                    else
                    {
                        XmlNode element = (XmlNode)ownerDocument.CreateElement("value");
                        selectNode2.AppendChild(element);
                    }
                }
            }
        }

        public XmlNode GetCurrentPropertyConfigurationNode()
        {
            //            return _p
            return null;
        }
    }
}
