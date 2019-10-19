// Decompiled with JetBrains decompilerd
// Type: ReportIssueUtilities.ReportIssueUtilities
// Assembly: ReportIssueUtilities, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BCC50BC3-68E9-403C-A04A-109EA9FA2C74
// Assembly location: C:\Users\Admin\Downloads\Dropbox\ri\Release\Release\ReportIssueUtilities.dll

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ReportIssueUtilities
{
    public static class ReportIssueUtilities
    {
        public static bool GetField(string from, ref string field, ref int ind)
        {
            ind = from.IndexOf("[[field]]", ind);
                
            if (ind == -1)
            {
                return false;
            }
            
            int indEnd = from.IndexOf("[[field_end]]", ind);
            
            if (indEnd == -1)
            {
                return false;
            }

            field = from.Substring(ind, indEnd - ind + 13);
            ind = indEnd + 13;

            return true;
        }

        public static bool GetError(string from, ref string err, ref int ind)
        {
            ind = from.IndexOf("[[error]]", ind);
                
            if (ind == -1)
            {
                return false;
            }

            int indEnd = from.IndexOf("[[error_end]]", ind);


            if (indEnd == -1)
            {
                return false;
            }

            err = from.Substring(ind, indEnd - ind + 11);
            ind = indEnd + 11;
            
            return true;
      }
 
        public static string EncodeField(string f)
        {
            StringBuilder result = new StringBuilder();
            result.Append("[[field]]");
            if (!string.IsNullOrEmpty(f))
            {
                result.Append(f);
            }
            result.Append("[[field_end]]");
            
            return result.ToString();
        }


        public static string EncodeError(Error error)
        {
            StringBuilder result = new StringBuilder();
               result.Append("[[error]]");
            result.Append(EncodeField(error.C1));
            result.Append(EncodeField(error.C2));
            result.Append(EncodeField(error.C3));
            result.Append(EncodeField(error.C4));
            result.Append(EncodeField(error.C5));
            result.Append("[[error_end]]");

            return result.ToString();
        }

        public static string EncodeErrors(ObservableCollection<Error> errors)
        {
            StringBuilder result = new StringBuilder();

            foreach (Error e in errors)
            {
                result.Append(EncodeError(e));
            }

            return result.ToString();
        }

        public static string DecodeField(string from)
        {
            int ind = from.IndexOf("[[field]]");

            if (ind != 0)
            {
                throw new ArgumentException(from);
            }

            int indEnd = from.IndexOf("[[field_end]]");

            if (indEnd == -1)
            {
               throw new ArgumentException(from);
            }

            return from.Substring(9, indEnd - 9);
        }


        public static Error DecodeError(string error)
        {
            Error result = new Error();

            string field = null;
            int ind = 0;
            int fieldNum = 1;
            while (GetField(error, ref field, ref ind))
            {
                string f = DecodeField(field);
                switch (fieldNum)
                {
                    case 1:
                        result.C1 = f;
                        break;
                    case 2:
                        result.C2 = f;
                        break;
                    case 3:
                        result.C3 = f;
                        break;
                    case 4:
                        result.C4 = f;
                        break;
                    case 5:
                        result.C5 = f;
                        break;
                }

                fieldNum++;
            }
            
            return result;
        }

        public static ObservableCollection<Error> DecodeErrors(string errors)
        {
            ObservableCollection<Error> result = new ObservableCollection<Error>();
         
            string err = null;
            int ind = 0;
            while (GetError(errors, ref err, ref ind))
            {
                Error error = DecodeError(err);
                result.Add(error);
            }

            return result;
        }

        public static WorkItem GetTfsWorkItem(
  TfsTeamProjectCollection collection,
    string projectName,
    int workItemId)
        {
            collection.EnsureAuthenticated();

            WorkItemStore workitemstore = collection.GetService<WorkItemStore>();
            WorkItem item = workitemstore.GetWorkItem(workItemId);

            return item;
        }

        
        public static WorkItem CreateTfsWorkItem(
          TfsTeamProjectCollection collection,
          string projectName,
          string title,
          string area,
          string iteration,
          string description,
          string itemType,
          Dictionary<string, object> parameters)
        {
            WorkItem workItem = new WorkItem(collection.GetService<WorkItemStore>().Projects[projectName].WorkItemTypes[itemType]);
            workItem.Title = title;
            workItem.Description = description;
            foreach (string key in parameters.Keys)
            {
                if (workItem.Fields.Contains(key))
                    workItem.Fields[key].Value = (object)parameters[key];
            }
            return workItem;
        }

        public static TfsTeamProjectCollection GetTfsTeamProjectCollection(
          string url)
        {
            return new TfsTeamProjectCollection(new Uri(url));
        }
    }
}
 