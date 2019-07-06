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

        [NotMapped]
        public List<Rectangle> Markers { get; set; }

        public Issue()
        {
            this.ID = Guid.NewGuid().ToString();
            this.Markers = new List<Rectangle>();

            this.BugPath = "";
            this.English = "";
            this.Fixed = true;
            this.Url = "";
            this.Selected = true;
            this.IssueTxt = "";
            this.MarkersString ="";
            this.PictureBytes = new byte[5];
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
            this._openStream = new MemoryStream(this.PictureBytes);
            this.Picture = new Bitmap((Stream)this._openStream);
            if (this.MarkersString == null)
                this.MarkersString = "";
            string[] strArray = this.MarkersString.Split(',');
            int num = strArray.Length / 4;
            this.Markers.Clear();
            for (int index = 0; index < num; ++index)
                this.Markers.Add(new Rectangle(int.Parse(strArray[index * 4]), int.Parse(strArray[index * 4 + 1]), int.Parse(strArray[index * 4 + 2]), int.Parse(strArray[index * 4 + 3])));
        }

        public void Save()
        {
            this.UpdateTime = DateTime.Now;
            MemoryStream memoryStream = new MemoryStream();
            this.Picture.Save((Stream)memoryStream, ImageFormat.Png);
            this.PictureBytes = memoryStream.ToArray();
            bool flag = false;
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < this.Markers.Count; ++index)
            {
                Rectangle marker = this.Markers[index];
                if (flag)
                    stringBuilder.Append(",");
                stringBuilder.AppendFormat("{0},", (object)marker.X);
                stringBuilder.AppendFormat("{0},", (object)marker.Y);
                stringBuilder.AppendFormat("{0},", (object)marker.Width);
                stringBuilder.AppendFormat("{0}", (object)marker.Height);
                flag = true;
            }
            this.MarkersString = stringBuilder.ToString();
        }

        public Issue(Issue from)
        {

        }

    }
}
