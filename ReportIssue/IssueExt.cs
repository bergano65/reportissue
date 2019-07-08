using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace ReportIssue
{
    public partial class Issue
    {
        [NotMapped]
        private MemoryStream _openStream;

        [NotMapped]
        public Bitmap Picture { get; set; }

        public bool Selected { get; set; }

        [NotMapped]
        public List<Picture> Pictures { get; set; }

        public Issue()
        {
            this.ID = Guid.NewGuid().ToString();

            this.BugPath = "";
            this.English = "";
            this.Fixed = true;
            this.Url = "";
            this.Selected = true;
            this.IssueTxt = "";
            this.Product = "";
            this.Reason = "";
            this.Right = "";
            this.Submitted = true;
            this.Template = "";
            this.Url = "";
            this.Wrong = "";
            this.WhereFound = "";
            this.Parameter1 = "";
            this.Parameter2 = "";
            this.Parameter3 = "";
            this.Parameter4 = "";
            this.Parameter5 = "";
            this.Parameter6 = "";
            this.Parameter7 = "";
            this.Parameter8 = "";
            this.Parameter9 = "";
            this.Parameter10 = "";
            this.Parameter11 = "";
            this.Parameter12 = "";
            this.Parameter13 = "";
            this.Parameter14 = "";
            this.Parameter15 = "";
            this.Parameter16 = "";
            this.Parameter17 = "";
            this.Parameter18 = "";
            this.Parameter19 = "";
            this.Parameter20 = "";
            this.UpdateTime = DateTime.Now;
        }

        public void Open()
        {
            RIDataModelContainer d = new RIDataModelContainer();

            this.Pictures.Clear();    
            string[] ids = this.PictureString.Split(',');
            foreach(string id in ids)
            {
                Picture p = d.Pictures.Find(id);
                if (p != null)
                {
                    this.Pictures.Add(p);
                }
            }

        }

        public void Save()
        {
            this.UpdateTime = DateTime.Now;

        }

        public Issue(Issue from)
        {

        }

    }
}
