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
        private MemoryStream _openStream;

        public bool IsOpened { get; set; }

        [NotMapped]
        public Bitmap Bitmap { get; set; }

        [NotMapped]
        public List<Rectangle> Markers { get; set; }

        public Picture()
        {
            this.IsOpened = false;
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
                    m.Top, m.Left, m.Width, m.Height);
            }

            this.MarkerString = markers.ToString();
            MemoryStream memoryStream = new MemoryStream();
            this.Bitmap.Save((Stream)memoryStream, ImageFormat.Png);
            this.Bytes = memoryStream.ToArray();

            this.IsOpened = true;
        }

        public void Open()
        {
            if (this.IsOpened)
            {
                return;
            }

            if (this.Bitmap == null)
            {
                if (this.Bytes != null)
                {
                    this._openStream = new MemoryStream(Bytes);
                    this.Bitmap = new Bitmap((Stream)this._openStream);
                }
            }

            if (this.MarkerString == null)
            {
                return;
            }

            try
            {
                string[] markArray = this.MarkerString.Split(':');
                foreach (string m in markArray)
                {
                    string[] strArray = m.Split(',');
                    int num = strArray.Length / 4;
                    for (int index = 0; index < num; index++)
                    {
                        System.Drawing.Rectangle sr =
                                new System.Drawing.Rectangle(
                                    int.Parse(strArray[index * 4]),
                                    int.Parse(strArray[(index * 4) + 1]),
                                    int.Parse(strArray[(index * 4) + 2]),
                                    int.Parse(strArray[(index * 4) + 3]));
                        this.Markers.Add(sr);
                    }

                    this.IsOpened = true;
                }
            }
            catch (Exception e)
            {
            }
        }

    }
}
