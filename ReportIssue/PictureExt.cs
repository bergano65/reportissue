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
    public partial class Picture
    {
        [NotMapped]
        public Bitmap Bitmap { get; set; }

        [NotMapped]
        public List<Rectangle> Markers { get; set; }

        public Picture()
        {
            this.ID = Guid.NewGuid().ToString();
            this.Markers = new List<Rectangle>();
        }

        public bool IsValid(out string msg)
        {
            if (this.Bytes == null)
            {
                msg = "Add at least one picture";
                return false;
            }

            if (this.Markers.Count == 0)
            {
                msg = "Add at least one marker";
                return false;
            }

            msg = "";
            return true;
        }

        public void Save()
        {
            StringBuilder markers = new StringBuilder();
            foreach (System.Drawing.Rectangle m in this.Markers)
            {
                if (this.Markers.IndexOf(m) > 0)
                {
                    markers.Append(":");
                }

                markers.AppendFormat("{0},{1},{2},{3}",
                    m.Top, m.Left, m.Right, m.Bottom);
            }

            this.MarkerString = markers.ToString();
            MemoryStream memoryStream = new MemoryStream();
            this.Bitmap.Save((Stream)memoryStream, ImageFormat.Png);
            this.Bytes = memoryStream.ToArray();
        }

        public void Open()
        {

        }

    }
}
