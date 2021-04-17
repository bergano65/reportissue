using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Xml;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Office.Interop.Outlook;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Collections.Generic;
using Path = System.IO.Path;

using System.Data.Entity.Migrations;

using ReportIssueUtilities;

namespace ReportIssue
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _sortAccending = true;
        private RIDataModelContainer _data;
        private string _sortColName;
        private TelemetryClient _tc;
        private string _tempFolder;
        private Filter _findFilter;
        private Filter _filter;
        private XmlDocument _cfgDoc;
        private bool _isIssueEdited;

        private Issue _currentIssue { get; set; }

        private int _issueNum { get; set; }

        private ObservableCollection<Issue> _issues { get; set; }

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public MainWindow()
        {        
            this._tc = new TelemetryClient();
            this._tc.Context.User.AuthenticatedUserId = "eviten@microsoft.com";
            this._tc.Context.Session.Id = Guid.NewGuid().ToString();
            IOperationHolder<RequestTelemetry> operation = this._tc.StartOperation<RequestTelemetry>("Start");
            this._tc.TrackEvent("Start", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this._data = new RIDataModelContainer();
            this._issues = new ObservableCollection<Issue>(this._data.Issues.ToList<Issue>());
            this._cfgDoc = new XmlDocument();
            this._cfgDoc.Load(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase), "IssueType.xml"));

            if (!this.CheckTemplates())
            {
                this._tc.TrackEvent("Check templates failed", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                this._tc.StopOperation<RequestTelemetry>(operation);
                System.Windows.Application.Current.Shutdown();
            }

            if (this._data.Filters.Count<Filter>() > 0)
            {
               this._filter = this._data.Filters.First<Filter>();
            }
            else
            {
                this._filter = new Filter();
                this._data.Filters.AddOrUpdate<Filter>(new Filter[1]
                {
          this._filter
                });
                this._data.SaveChanges();
            }
            this.InitializeComponent();
            this._sortColName = "UpdateTime";
            this._sortAccending = false;
            this.RedrawList();
            this._tc.StopOperation<RequestTelemetry>(operation);
            this._issueNum = -1;
        }


        private XmlNode GetIssueTemplate(string type)
        {
            foreach (XmlNode selectNode in this._cfgDoc.SelectNodes("//issue"))
            {
                XmlNode xmlNode = selectNode.SelectSingleNode(nameof(type));
                if (xmlNode != null && xmlNode.InnerText == type)
                    return selectNode;
            }
            return (XmlNode)null;
        }

        private void Update()
        {
            this._tc.TrackEvent("Update database", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            foreach (Issue issue in (Collection<Issue>)this._issues)
            {
                if (this.GetIssueTemplate(issue.Template) != null && issue.Template == "VS doc bug template" && issue.Parameter9 == null)
                    issue.Parameter9 = issue.Url;
            }
        }

        private bool CheckTemplates()
        {
            IOperationHolder<RequestTelemetry> operation = this._tc.StartOperation<RequestTelemetry>("Check Templates");
            this._tc.TrackEvent("Check templates", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            string messageBoxText = string.Empty;
            foreach (XmlNode selectNode in this._cfgDoc.SelectNodes("//issue"))
            {
                if (selectNode.SelectSingleNode("type") == null)
                {
                    messageBoxText = "All templates should have type defined";
                    break;
                }
            }
            bool flag;
            if (!string.IsNullOrEmpty(messageBoxText))
            {
                this._tc.TrackEvent(string.Format("Check templates error. {0}", (object)messageBoxText), (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                int num = (int)MessageBox.Show(messageBoxText, "Issue report");
                flag = false;
            }
            else
            {
                this._tc.TrackEvent("Check templates ok", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                flag = true;
            }
            this._tc.StopOperation<RequestTelemetry>(operation);
            return flag;
        }

        private void DuplicateIssue()
        {
            IOperationHolder<RequestTelemetry> operation = this._tc.StartOperation<RequestTelemetry>("Duplicate Issue");
            if (this._issueList.SelectedIndex != -1)
            {
                this._tc.TrackEvent("Duplicate Issue", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                this._currentIssue = new Issue(this._issues[this._issueList.SelectedIndex]);
                this._data.Issues.Add(this._currentIssue);
                this._data.SaveChanges();
            }
            else
                this._tc.TrackEvent("Duplicate Issue canceled", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this.RedrawList();
            this._tc.StopOperation<RequestTelemetry>(operation);
        }

        private void NewIssue()
        {
            try
            {
                IOperationHolder<RequestTelemetry> operation = this._tc.StartOperation<RequestTelemetry>("New Issue");
                if (this._isIssueEdited)
                {
                    return;
                }

                this._tc.TrackEvent("New Issue", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                this._currentIssue = new Issue();
                this._isIssueEdited = true;
                EditIssueDlg editIssueDlg2 = new EditIssueDlg((Window)this, (Issue)null);
                editIssueDlg2.ShowDialog();
                if (editIssueDlg2.Saved)
                {
                    this._currentIssue = editIssueDlg2.CurrentIssue;
                    this._data.Issues.Add(this._currentIssue);
                    this._data.SaveChanges();
                    this.RedrawList();
                    this._tc.TrackEvent("New Issue created", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                }
                else
                    this._tc.TrackEvent("New Issue canceled", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                this.RedrawList();
                this._tc.StopOperation<RequestTelemetry>(operation);
                this._isIssueEdited = false;
            }
            catch (System.Exception e)
            {
                this._isIssueEdited = false;
            }
        } 

        private void RedrawList()
        {
            this._tc.TrackPageView("Redraw Issue List");
            this._tc.TrackEvent("Redraw Issue List", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this.FilterIssueList();
            this.SortIssueList(this._sortColName, new bool?(this._sortAccending));
        }

        private void FilterIssueList()
        {
            this._tc.TrackEvent("Filter Issue List", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this._issues = new ObservableCollection<Issue>(this.GetFilteredIssues(this._data.Issues.AsEnumerable<Issue>(), this._filter));
            this._issueList.ItemsSource = (IEnumerable)this._issues;
            CollectionViewSource.GetDefaultView((object)this._issueList.ItemsSource).Refresh();
        }

        private void setButtonsState()
        {
            if (this._issueList.SelectedIndex != -1)
            {
                this._editIssueBtn.IsEnabled = true;
                this._deleteIssueBtn.IsEnabled = true;
            }
            else
            {
                this._editIssueBtn.IsEnabled = false;
                this._deleteIssueBtn.IsEnabled = false;
            }
        }

        private string GetReportMail(IEnumerable<Issue> issues, ref int pictureCount)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Issue issue in issues)
            {
                XmlDocument document = (this.DataContext as XmlDataProvider).Document;
                XmlNode xmlNode = (XmlNode)null;
                foreach (XmlNode selectNode in document.SelectNodes("//issue"))
                {
                    if (selectNode.SelectSingleNode("type").InnerText == issue.Template)
                    {
                        xmlNode = selectNode;
                        break;
                    }
                }
                if (xmlNode == null)
                {
                    string str = string.Format("No template found of type '{0}'", (object)issue.Template);
                    int num = (int)MessageBox.Show(str, "Issue report");
                    throw new ArgumentException(str);
                }
                stringBuilder.Append("<br/><br/>");
                stringBuilder.Append("<table border='1'>");
                foreach (XmlNode selectNode in xmlNode.SelectNodes("parameters/parameter"))
                {
                    stringBuilder.Append("<tr>");
                    stringBuilder.AppendFormat("<td>{0}</td>", (object)selectNode.Attributes["alias"].Value);
                    string format = (string)null;
                    if (selectNode.Attributes["output"] != null)
                        format = selectNode.Attributes["output"].Value;
                    string name = string.Format("Parameter{0}", (object)selectNode.Attributes["num"].Value);
                    string str = issue.GetType().GetProperty(name).GetValue((object)issue) as string;
                    if (format != null)
                        str = string.Format(format, (object)str);
                    stringBuilder.AppendFormat("<td>{0}</td>", (object)str);
                    stringBuilder.Append("</tr>");
                }

                stringBuilder.Append("<tr>");
                // add pictures
                stringBuilder.Append("<td>Pictures</td><td>");
                issue.Open();
                foreach (Picture p in issue.Pictures)
                {
                    stringBuilder.AppendFormat(" Picture-{0}.png", pictureCount++);
                }
                stringBuilder.Append("<td></tr>");

                stringBuilder.Append("</table>");
            }


            return stringBuilder.ToString();
        }

        public string GetFieldValue(Issue issue, string value)
        {
            if (value.IndexOf("{") == -1)
                return value;
            MatchCollection matchCollection = Regex.Matches(value, "{(\\w+)}");
            if (matchCollection.Count == 0)
                return value;
            string str1 = value;
            foreach (Match match in matchCollection)
            {
                string str2 = match.Groups[1].Value;
                string name = string.Format("Parameter{0}", (object)str2);
                string newValue = issue.GetType().GetProperty(name).GetValue((object)issue) as string;
                str1 = str1.Replace("{" + str2 + "}", newValue);
            }

            return str1;
        }

        private WorkItem CreateBug(Issue issue)
        {
            XmlNode issueTemplate = this.GetIssueTemplate(issue.Template);
            if (issueTemplate == null)
            {
                this._tc.TrackEvent(string.Format("Unable to find template '{0}'", (object)issue.Template), (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                int num = (int)MessageBox.Show(string.Format("Unable to find template '{0}'", (object)issue.Template), "Issue report");
                return (WorkItem)null;
            }
            XmlNode xmlNode1 = issueTemplate.SelectSingleNode("file");
            if (xmlNode1 == null)
            {
                this._tc.TrackEvent(string.Format("Unable to find bug report description for template '{0}'", (object)issue.Template), (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                int num = (int)MessageBox.Show(string.Format("Unable to find bug report description for template '{0}'", (object)issue.Template), "Issue report");
                return (WorkItem)null;
            }

            XmlNode xmlNode2 = xmlNode1.SelectSingleNode("server");
            if (xmlNode2 == null)
            {
                this._tc.TrackEvent(string.Format("Unable to find tfs server for template '{0}'", (object)issue.Template), (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                int num = (int)MessageBox.Show(string.Format("Unable to find tfs server for template '{0}'", (object)issue.Template), "Issue report");
                return (WorkItem)null;
            }

            XmlNode xmlNode3 = xmlNode1.SelectSingleNode("project");
            if (xmlNode3 == null)
            {
                this._tc.TrackEvent(string.Format("Unable to find project for template '{0}'", (object)issue.Template), (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                int num = (int)MessageBox.Show(string.Format("Unable to find project for template '{0}'", (object)issue.Template), "Issue report");
                return (WorkItem)null;
            }


            string innerText1 = xmlNode2.InnerText;
            string innerText2 = xmlNode3.InnerText;
            TfsTeamProjectCollection projectCollection = ReportIssueUtilities.ReportIssueUtilities.GetTfsTeamProjectCollection(innerText1);
            string title = issue.Parameter4;
            string projectName = xmlNode3.InnerText;

            string description = "";
            string area = "";
            int severity = 3;

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            foreach (XmlNode selectNode in xmlNode1.SelectNodes("fields/field"))
            {
                try
                {
                    string index = selectNode.Attributes["name"].Value;
                    if (selectNode.Attributes["alias"] != null)
                    {
                        string str = selectNode.Attributes["alias"].Value;
                        if (str != null)
                            index = str;
                    }

                    if (selectNode.Attributes["value"] == null)
                    {
                        if (selectNode.InnerText.Length <= 0)
                            continue;
                    }

                    string type = "";
                    if (selectNode.Attributes["type"] != null)
                    {
                        type = selectNode.Attributes["type"].Value;
                    }

                    string str1 = selectNode.Attributes["value"] == null ? selectNode.InnerText : selectNode.Attributes["value"].Value;

                    string fieldValue = this.GetFieldValue(issue, str1);

                    if (index == "title")
                        title = fieldValue;
                    else
                    {
                        parameters[index] = fieldValue;
                        if (type == "int")
                        {
                            parameters[index] = (Int32.Parse(fieldValue));
                        }                    
                    }
                }
                catch (System.Exception ex)
                {
                }
            }
            try
            {
                // check item already exist
                WorkItem tfsWorkItem = null; 
                int id = 0;

                if (Int32.TryParse(issue.BugPath, out id))
                {
                    tfsWorkItem = ReportIssueUtilities.ReportIssueUtilities.GetTfsWorkItem(projectCollection, projectName, id);
                }
                
/*
                foreach (Field f in tfsWorkItem.Fields)
                {
                    if (f.Value != null && f.Value.ToString().Contains("rows"))
                    {

                    }
                }
*/

                MessageBoxResult result = MessageBoxResult.No;
                if (tfsWorkItem != null)
                {
                    result = MessageBox.Show("Bug already exist. Yes update parameters, No create another bug or Cancel", "Issu= report", MessageBoxButton.YesNoCancel);
                    if (result == MessageBoxResult.Cancel)
                    {
                        return (WorkItem)null;
                    }
                }

                if (result == MessageBoxResult.No)
                {
                    tfsWorkItem = ReportIssueUtilities.ReportIssueUtilities.CreateTfsWorkItem(projectCollection, innerText2, title, (string)null, (string)null, description, "Bug", parameters);
                }

                if (parameters.ContainsKey("Area"))
                {
                    area = parameters["Area"] as String;
                }

                XmlNode xmlNode4 = xmlNode1.SelectSingleNode("bugpath");
                if (xmlNode4 == null)
                {
                    this._tc.TrackEvent(string.Format("Unable to find bug path description for template '{0}'", (object)issue.Template), (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                    int num = (int)MessageBox.Show(string.Format("Unable to find bug path description for template '{0}'", (object)issue.Template), "Issue report");
                    return (WorkItem)null;
                }

                List<string> pictures = this.GetPictures((IEnumerable<Issue>)new List<Issue>()
                {
                  issue
                });

                foreach (string p in pictures)
                {
                    Microsoft.TeamFoundation.WorkItemTracking.Client.Attachment attachment = new Microsoft.TeamFoundation.WorkItemTracking.Client.Attachment(p, string.Format("Issue Screenshot {0}", pictures.IndexOf(p) + 1));
                    tfsWorkItem.Attachments.Add(attachment);
                }

                tfsWorkItem.AreaPath = area;
                tfsWorkItem.Save();
//                issue.BugPath = string.Format(xmlNode4.InnerText, (object)tfsWorkItem.Id);
                issue.BugPath = issue.Parameter15 = tfsWorkItem.Id.ToString();
                this.DeletePictures(pictures);
                return tfsWorkItem;
            }
            catch (System.Exception ex)
            {
                string msg = string.Format("Bug creation error '{0}'", ex.Message);
                this._tc.TrackEvent(msg);
                MessageBox.Show(msg, "Issue report");
                return null;
            }
        }

        private void createBugButton_Click(object sender, RoutedEventArgs e)
        {
            IOperationHolder<RequestTelemetry> operation = this._tc.StartOperation<RequestTelemetry>("Create Issue Bug");
            if (this._issueList.SelectedIndex == -1)
            {
                this._tc.TrackEvent("No issues collected for bug reporting", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                int num = (int)MessageBox.Show("No issues collected for bug reporting", "Issue report");
                this._tc.StopOperation<RequestTelemetry>(operation);
            } 
            else
            {
                this._tc.TrackEvent("Create Issue Bug", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                Issue issue = this._issues[this._issueList.SelectedIndex];
                bool flag = true;

                this._tc.TrackEvent(string.Format("Create Issue Bug for issue {0}", (object)issue.ID), (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                if (this.CreateBug(issue) != null)
                {
                    this._tc.TrackEvent(string.Format("Create Issue Bug for issue {0}", (object)issue.ID), (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                }
                else
                { 
                    this._tc.TrackEvent(string.Format("Create Issue Bug for issue {0} failed", (object)issue.ID), (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                    flag = false;
                }

                try
                {
                    this._data.SaveChanges();
                }
                catch (System.Exception ex)
                {
                    this._tc.TrackEvent("Save database failed", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                    MessageBox.Show("Can't save database", "Issue report");
                    flag = false;
                }
                finally
                {
                  this._tc.StopOperation<RequestTelemetry>(operation);
                }

                if (flag)
                {
                    MessageBox.Show("Bug created", "Issue report");
                }
                else
                {
                    MessageBox.Show("Bug create failed", "Issue report");
                }
            }
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            IOperationHolder<RequestTelemetry> operation = this._tc.StartOperation<RequestTelemetry>("Report Issues");
            IEnumerable<Issue> issues = this._issues.Where<Issue>((Func<Issue, bool>)(i => i.Selected));
            if (issues.Count<Issue>() == 0)
            {
                int num = (int)MessageBox.Show("No issues collected for sending", "Issue report");
                this._tc.StopOperation<RequestTelemetry>(operation);
            }
            else
            {
                this._tc.TrackEvent("Send Issue Report", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                try
                {
                    this._tc.TrackEvent("Choose report mail", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                    ChooseMailDlg chooseMailDlg = new ChooseMailDlg();
                    chooseMailDlg.ShowDialog();
                    bool? dialogResult = chooseMailDlg.DialogResult;
                    bool flag = true;
                    if ((dialogResult.GetValueOrDefault() == flag ? (dialogResult.HasValue ? 1 : 0) : 0) != 0)
                    {
                        this._tc.TrackEvent("Report mail chosen", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                        this._tc.TrackEvent("Start Outlook", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                        // ISSUE: variable of a compiler-generated type
                        Microsoft.Office.Interop.Outlook.Application instance = (Microsoft.Office.Interop.Outlook.Application)Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("0006F03A-0000-0000-C000-000000000046")));
                        MailItem mailItem =
                                instance.CreateItem(OlItemType.olMailItem);
                        mailItem.Subject = "Document fixes";
                        mailItem.To = chooseMailDlg.Mail;

                        int pictureCount = 1;
                        mailItem.HTMLBody = this.GetReportMail(issues, ref pictureCount);

                        List<string> pictures = this.GetPictures(issues);
                        for (int index = 0; index < pictures.Count; ++index)
                        {
                            // ISSUE: reference to a compiler-generated method
                            mailItem.Attachments.Add((object)pictures[index], (object)OlAttachmentType.olByValue, (object)(index + 1), (object)string.Format("Picture{0}.png", (object)(index + 1)));
                        }
                        this._tc.TrackEvent("Send mail", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                        // ISSUE: reference to a compiler-generated method
                        mailItem.Display((object)false);
                        // ISSUE: reference to a compiler-generated method
//                        mailItem.Send();
                        this.DeletePictures(pictures);
                        int num = (int)MessageBox.Show("Report sent successfully");
                    }
                    else
                        this._tc.TrackEvent("Report mail choose canceled", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                }
                catch (System.Exception ex)
                {
                    this._tc.TrackException(ex, (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                    int num = (int)MessageBox.Show("Failed to run Outlook. Is it already running? Close it in this case");
                }
                finally
                {
                    this._tc.StopOperation<RequestTelemetry>(operation);
                }
            }
        }

        private void DeletePictures(List<string> pics)
        {
            this._tc.TrackEvent("Delete pictures", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            foreach (string pic in pics)
                File.Delete(pic);
            Directory.Delete(this._tempFolder);
        }

        private List<string> GetPictures(IEnumerable<Issue> issues)
        {
            this._tc.TrackEvent("Get pictures", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            int pictureNum = 1;
            List<string> stringList = new List<string>();

            for (int index = 0; index < issues.ToList<Issue>().Count; ++index)
           {
                Issue issue = issues.ToList<Issue>()[index];
                issue.Open();

                foreach (Picture p in issue.Pictures)
                {

                    DrawingVisual drawingVisual = new DrawingVisual();
                    DrawingContext drawingContext = drawingVisual.RenderOpen();
                    BitmapSource sourceFromHbitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(p.Bitmap.GetHbitmap(), (IntPtr)0, new Int32Rect(0, 0, p.Bitmap.Width, p.Bitmap.Height), BitmapSizeOptions.FromWidthAndHeight(805, 750));
                    drawingContext.DrawImage((ImageSource)sourceFromHbitmap, new Rect(0.0, 0.0, (double)805, (double)750));

                    foreach (Marker marker in p.Markers)
                    {
                        drawingContext.DrawRectangle((System.Windows.Media.Brush)null, new System.Windows.Media.Pen((System.Windows.Media.Brush)System.Windows.Media.Brushes.Red, 2.0), new Rect((double)marker.Left, (double)marker.Top, (double)marker.Width, (double)marker.Height));
                    }

                    drawingContext.Close();
                    RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(805, 750, 96.0, 96.0, PixelFormats.Pbgra32);
                    renderTargetBitmap.Render((Visual)drawingVisual);
                    this._tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                    Directory.CreateDirectory(this._tempFolder);
                    string path = Path.Combine(this._tempFolder, string.Format("Picture-{0}.png", pictureNum++));
                    stringList.Add(path);
                    PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
                    pngBitmapEncoder.Frames.Add(BitmapFrame.Create((BitmapSource)renderTargetBitmap));
                    using (Stream stream = (Stream)File.Create(path))
                        pngBitmapEncoder.Save(stream);
                }
            }

            return stringList;
        }

        private void editIssueButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this._isIssueEdited)
                {
                    return;
                }

                IOperationHolder<RequestTelemetry> operation = this._tc.StartOperation<RequestTelemetry>("Edit Issue");
                this._tc.TrackEvent("Edit Issue", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                int selectedIndex = this._issueList.SelectedIndex;
                if (selectedIndex == -1)
                {
                    this._tc.TrackEvent("No Issue to edit found", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                    this._tc.StopOperation<RequestTelemetry>(operation);
                }
                else
                {
                    this._currentIssue = this._issues[selectedIndex];
                    EditIssueDlg editIssueDlg2 = new EditIssueDlg((Window)this, this._currentIssue);
                    editIssueDlg2.ShowDialog();
                    /*
                     *if (editIssueDlg2.Saved)
                                        {
                                            this._currentIssue = editIssueDlg2.CurrentIssue;
                                            this._issues[selectedIndex] = this._currentIssue;
                                            this._data.Issues.AddOrUpdate<Issue>(new Issue[1]
                                            {
                                    this._currentIssue
                                            });
                                            this._data.SaveChanges();
                                            this._tc.TrackEvent("Issue edited", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                                        }
                                        else
                                            this._tc.TrackEvent("Issue edit canceled", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                                        this.RedrawList();
                                        this._tc.StopOperation<RequestTelemetry>(operation);
                                    }
                    */
                }
            }
            catch (System.Exception ee)
            {
            }
        }

        private void newIssueButton_Click(object sender, RoutedEventArgs e)
        {
            this.NewIssue();
        }

        private void dupIssueButton_Click(object sender, RoutedEventArgs e)
        {
            this.DuplicateIssue();
        }

        private void removeIssueButton_Click(object sender, RoutedEventArgs e)
        {
            IOperationHolder<RequestTelemetry> operation = this._tc.StartOperation<RequestTelemetry>("Delete Issue");
            this._tc.TrackEvent("Delete Issue", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            int selectedIndex = this._issueList.SelectedIndex;
            if (selectedIndex == -1)
            {
                this._tc.TrackEvent("Delete Issue skipped, no issue selected", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            }
            else
            {
                if (MessageBox.Show("Are you sure you want delete issue?", "Issue report", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    this._data.Issues.Remove(this._issues[selectedIndex]);
                    this._data.SaveChanges();
                    this.RedrawList();
                    this._tc.TrackEvent("Issue deleted", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                }
                else
                    this._tc.TrackEvent("Delete Issue not confirmed", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                this._tc.StopOperation<RequestTelemetry>(operation);
            }
        }

        private void _issueList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.setButtonsState();
        }

        private IEnumerable<Issue> GetFilteredIssues(
          IEnumerable<Issue> issues,
          Filter filter)
        {
            IEnumerable<Issue> source = issues;
            this._tc.TrackEvent("Get Filtered Issues", (IDictionary<string, string>)null, (IDictionary<string, double>)null);

            if (filter.Status != "Any")
                source = source.Where<Issue>((Func<Issue, bool>)(i => i.Parameter14 == filter.Status));

            if (filter.Submitted != "Any")
                source = !(filter.Submitted == "Yes") ? source.Where<Issue>((Func<Issue, bool>)(i => !i.Submitted)) : source.Where<Issue>((Func<Issue, bool>)(i => i.Submitted));
            if (filter.Fixed != "Any")
                source = !(filter.Fixed == "Yes") ? source.Where<Issue>((Func<Issue, bool>)(i => !i.Fixed)) : source.Where<Issue>((Func<Issue, bool>)(i => i.Fixed));
            if (filter.Product.Length > 0)
                source = source.Where<Issue>((Func<Issue, bool>)(i =>
                {
                    if (i.Parameter2 != null)
                        return i.Parameter2.IndexOf(filter.Product) != -1;
                    return false;
                }));
            if (filter.Issue.Length > 0)
                source = source.Where<Issue>((Func<Issue, bool>)(i =>
                {
                    if (i.Parameter4 != null)
                        return i.Parameter4.IndexOf(filter.Issue) != -1;
                    return false;
                }));
            if (filter.Wrong.Length > 0)
                source = source.Where<Issue>((Func<Issue, bool>)(i =>
                {
                    if (i.Parameter5 != null)
                        return i.Parameter5.IndexOf(filter.Wrong) != -1;
                    return false;
                }));
            if (filter.Right.Length > 0)
                source = source.Where<Issue>((Func<Issue, bool>)(i =>
                {
                    if (i.Parameter6 != null)
                        return i.Parameter6.IndexOf(filter.Right) != -1;
                    return false;
                }));
            return source;
        }

        private void SortIssueList(string colName, bool? ascending)
        {
            this._tc.TrackEvent("Sort Issues", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this._sortAccending = !(colName == this._sortColName) || ascending.HasValue ? ascending.Value : !this._sortAccending;
            this._sortColName = colName;
            if (this._sortColName == "Send")
            {
                this._tc.TrackEvent("Sort by Selected", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                this._issues = !this._sortAccending ? new ObservableCollection<Issue>(this._issues.OrderByDescending<Issue, bool>((Func<Issue, bool>)(e => e.Selected)).ToList<Issue>()) : new ObservableCollection<Issue>(this._issues.OrderBy<Issue, bool>((Func<Issue, bool>)(e => e.Selected)).ToList<Issue>());
            }
            else if (this._sortColName == "Url")
            {
                this._tc.TrackEvent("Sort by Url", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                this._issues = !this._sortAccending ? new ObservableCollection<Issue>(this._issues.OrderByDescending<Issue, string>((Func<Issue, string>)(e => e.Url)).ToList<Issue>()) : new ObservableCollection<Issue>(this._issues.OrderBy<Issue, string>((Func<Issue, string>)(e => e.Url)).ToList<Issue>());
            }
            else if (this._sortColName == "Product")
            {
                this._tc.TrackEvent("Sort by Product", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                this._issues = !this._sortAccending ? new ObservableCollection<Issue>(this._issues.OrderByDescending<Issue, string>((Func<Issue, string>)(e => e.Product)).ToList<Issue>()) : new ObservableCollection<Issue>(this._issues.OrderBy<Issue, string>((Func<Issue, string>)(e => e.Product)).ToList<Issue>());
            }
            else if (this._sortColName == "IssueTxt")
            {
                this._tc.TrackEvent("Sort by Issue", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                this._issues = !this._sortAccending ? new ObservableCollection<Issue>(this._issues.OrderByDescending<Issue, string>((Func<Issue, string>)(e => e.IssueTxt)).ToList<Issue>()) : new ObservableCollection<Issue>(this._issues.OrderBy<Issue, string>((Func<Issue, string>)(e => e.IssueTxt)).ToList<Issue>());
            }
            else if (this._sortColName == "UpdateTime")
            {
                this._tc.TrackEvent("Sort by UpdateTime", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                this._issues = !this._sortAccending ? new ObservableCollection<Issue>(this._issues.OrderByDescending<Issue, DateTime>((Func<Issue, DateTime>)(e => (DateTime)e.UpdateTime)).ToList<Issue>()) : new ObservableCollection<Issue>(this._issues.OrderBy<Issue, DateTime>((Func<Issue, DateTime>)(e => (DateTime)e.UpdateTime)).ToList<Issue>());
            }
            else if (this._sortColName == "Wrong")
            {
                this._tc.TrackEvent("Sort by Wrong", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                this._issues = !this._sortAccending ? new ObservableCollection<Issue>(this._issues.OrderByDescending<Issue, string>((Func<Issue, string>)(e => e.Wrong)).ToList<Issue>()) : new ObservableCollection<Issue>(this._issues.OrderBy<Issue, string>((Func<Issue, string>)(e => e.Wrong)).ToList<Issue>());
            }
            else if (this._sortColName == "Right")
            {
                this._tc.TrackEvent("Sort by Right", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                this._issues = !this._sortAccending ? new ObservableCollection<Issue>(this._issues.OrderByDescending<Issue, string>((Func<Issue, string>)(e => e.Right)).ToList<Issue>()) : new ObservableCollection<Issue>(this._issues.OrderBy<Issue, string>((Func<Issue, string>)(e => e.Right)).ToList<Issue>());
            }
            else if (this._sortColName == "Submitted")
            {
                this._tc.TrackEvent("Sort by Submitted", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                this._issues = !this._sortAccending ? new ObservableCollection<Issue>(this._issues.OrderByDescending<Issue, bool>((Func<Issue, bool>)(e => e.Submitted)).ToList<Issue>()) : new ObservableCollection<Issue>(this._issues.OrderBy<Issue, bool>((Func<Issue, bool>)(e => e.Submitted)).ToList<Issue>());
            }
            else if (this._sortColName == "Fixed")
            {
                this._tc.TrackEvent("Sort by Fixed", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                this._issues = !this._sortAccending ? new ObservableCollection<Issue>(this._issues.OrderByDescending<Issue, bool>((Func<Issue, bool>)(e => e.Fixed)).ToList<Issue>()) : new ObservableCollection<Issue>(this._issues.OrderBy<Issue, bool>((Func<Issue, bool>)(e => e.Fixed)).ToList<Issue>());
            }
            this._issueList.ItemsSource = (IEnumerable)this._issues;
            CollectionViewSource.GetDefaultView((object)this._issueList.ItemsSource).Refresh();
        }

        private void _issueList_Click(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is GridViewColumnHeader))
                return;
            IOperationHolder<RequestTelemetry> operation = this._tc.StartOperation<RequestTelemetry>("Sort Issues");
            this._tc.TrackEvent("Click sort issues", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            string content = ((ContentControl)e.OriginalSource).Content as string;
            if (content == this._sortColName)
                this.SortIssueList(this._sortColName, new bool?());
            else
                this.SortIssueList(content, new bool?(true));
            this._tc.StopOperation<RequestTelemetry>(operation);
        }

        private void _filterIssueBtn_Click(object sender, RoutedEventArgs e)
        {
            IOperationHolder<RequestTelemetry> operation = this._tc.StartOperation<RequestTelemetry>("Filter Issues");
            this._tc.TrackEvent("Filter Issues", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            FilterDlg filterDlg = new FilterDlg(this._filter);
            bool? nullable = filterDlg.ShowDialog();
            bool flag = true;
            if ((nullable.GetValueOrDefault() == flag ? (nullable.HasValue ? 1 : 0) : 0) != 0)
            {
                this._tc.TrackEvent("Filter Issues", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                this._filter = filterDlg.Filter;
                this._data.Filters.AddOrUpdate<Filter>(new Filter[1]
                {
          this._filter
                });
                this._data.SaveChanges();
                this.RedrawList();
            }
            else
                this._tc.TrackEvent("Filter Issues canceled", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this._tc.StopOperation<RequestTelemetry>(operation);
        }

        private void _selectAllIssueBtn_Click(object sender, RoutedEventArgs e)
        {
            this._tc.TrackEvent("Select All Issues", (IDictionary<string, string>)null, (IDictionary<string, double>)null);

            foreach (Issue issue in (Collection<Issue>)this._issues)
            {
                issue.Selected = !issue.Selected;
            }
    
            CollectionViewSource.GetDefaultView((object)this._issueList.ItemsSource).Refresh();
        }

        private void _findIssueBtn_Click(object sender, RoutedEventArgs e)
        {
            IOperationHolder<RequestTelemetry> operation = this._tc.StartOperation<RequestTelemetry>("Find Issue");
            this._tc.TrackEvent("Find Issue", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            Filter filter1 = new Filter();
            Filter filter2;
            if (this._findFilter != null)
            {
                filter2 = this._findFilter;
            }
            else
            {
                FilterDlg filterDlg = new FilterDlg(filter1);
                bool? nullable = filterDlg.ShowDialog();
                bool flag = true;
                if ((nullable.GetValueOrDefault() == flag ? (!nullable.HasValue ? 1 : 0) : 1) != 0)
                {
                    this._tc.TrackEvent("Find Issue canceled", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                    this._tc.StopOperation<RequestTelemetry>(operation);
                    return;
                }
                filter2 = filterDlg.Filter;
            }
            this._findFilter = filter2;
            this._tc.TrackEvent("Find Issue started", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            IEnumerable<Issue> source1 = this.GetFilteredIssues((IEnumerable<Issue>)this._issues, filter2);
            int currentIssueNum = this._issueList.SelectedIndex;
            if (currentIssueNum != -1)
            {
                IEnumerable<Issue> source2 = source1.Where<Issue>((Func<Issue, bool>)(i => this._issues.IndexOf(i) > currentIssueNum));
                if (source2.Count<Issue>() > 0)
                    source1 = source2;
            }
            if (source1.Count<Issue>() > 0)
            {
                this._tc.TrackEvent("Found Issue", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
                this._issueList.SelectedIndex = this._issues.IndexOf(source1.First<Issue>());
            }
            else
            {
                int num = (int)MessageBox.Show("No issues found", "Issue Report");
            }
            this._tc.StopOperation<RequestTelemetry>(operation);
        }

        private void _resetFilterIssueBtn_Click(object sender, RoutedEventArgs e)
        {
            IOperationHolder<RequestTelemetry> operation = this._tc.StartOperation<RequestTelemetry>("Reset Issue Filter");
            this._tc.TrackEvent("Reset Issue Filter", (IDictionary<string, string>)null, (IDictionary<string, double>)null);
            this._filter.Reset();
            this._data.Filters.AddOrUpdate<Filter>(new Filter[1]
            {
        this._filter
            });
            this._data.SaveChanges();
            this.RedrawList();
            this._tc.StopOperation<RequestTelemetry>(operation);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Insert)
                this.newIssueButton_Click((object)null, (RoutedEventArgs)null);
            else if (e.Key == Key.Return)
            {
                this.editIssueButton_Click((object)null, (RoutedEventArgs)null);
            }
            else if (e.Key == Key.Space)
            {
                IEnumerable<Issue> issues = this._issues.Where<Issue>((Func<Issue, bool>)(i => i.Selected));
                foreach (Issue i in issues)
                {
                    i.Selected = !i.Selected;
                }

                this.RedrawList();
            }
            else
            {
                if (e.Key == Key.F3)
                    this._findIssueBtn_Click((object)null, (RoutedEventArgs)null);
                if (!e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) && !e.KeyboardDevice.IsKeyDown(Key.RightCtrl) || e.Key != Key.F)
                    return;
                this._findFilter = (Filter)null;
                this._findIssueBtn_Click((object)null, (RoutedEventArgs)null);
            }
        }

        private void locationHyperlink_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Documents.Hyperlink source = e.Source as System.Windows.Documents.Hyperlink;
            if (source.NavigateUri == (Uri)null)
            {
                int num = (int)MessageBox.Show("No issue web location specified", "Issue report");
            }
            Process.Start(source.NavigateUri.ToString());
        }

        private void bugPathHyperlink_Click(object sender, RoutedEventArgs e)
        {
            Issue issue = (e.Source as System.Windows.Documents.Hyperlink).DataContext as Issue;
            
            // find bug location
            XmlNode issueTemplate = this.GetIssueTemplate(issue.Template);
            if (issueTemplate == null)
            {
                string msg = string.Format("Unable to find template '{0}'", issue.Template);
                this._tc.TrackEvent(msg);
                MessageBox.Show(msg, "Issue report");
                return;
            }

            XmlNode xmlNode1 = issueTemplate.SelectSingleNode("file");
            if (xmlNode1 == null)
            {
                string msg = string.Format("Unable to find template '{0}' bug description", issue.Template);
                this._tc.TrackEvent(msg);
                MessageBox.Show(msg, "Issue report");
                return;
            }

            XmlNode xmlNode3 = xmlNode1.SelectSingleNode("bugpath");
            if (xmlNode3 == null)
            {
                string msg = string.Format("Unable to find template '{0}' bug path", issue.Template);
                this._tc.TrackEvent(msg);
                MessageBox.Show(msg, "Issue report");
                return;
            }

            try
            {
                string bugpath = xmlNode3.InnerText;

                // check ID for validity
                Int32 id;
                if (!Int32.TryParse(issue.Parameter15, out id))
                {
                    MessageBox.Show("No issue  related bug created", "Issue report");
                    return;
                }

                Uri uri = new Uri(string.Format(bugpath, issue.Parameter15));
                Process.Start(uri.ToString());
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("No issue  related bug created", "Issue report");
                return;
            }
        }

        private void _issueHyperlink_Click(object sender, RoutedEventArgs e)
        {
            Issue dataContext = (e.Source as System.Windows.Documents.Hyperlink).DataContext as Issue;
            Process.Start(dataContext.Parameter9);
        }
    }
}

