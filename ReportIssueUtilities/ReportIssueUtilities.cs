// Decompiled with JetBrains decompiler
// Type: ReportIssueUtilities.ReportIssueUtilities
// Assembly: ReportIssueUtilities, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BCC50BC3-68E9-403C-A04A-109EA9FA2C74
// Assembly location: C:\Users\Admin\Downloads\Dropbox\ri\Release\Release\ReportIssueUtilities.dll

using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;

namespace ReportIssueUtilities
{
    public static class ReportIssueUtilities
    {

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
