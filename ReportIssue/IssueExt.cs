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

        public byte[] PictureBytes { get; set; }

        public bool Selected { get; set; }

        [NotMapped]
        public List<Rectangle> Markers { get; set; }

        public Issue()
        {
            this.Markers = new List<Rectangle>();
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
