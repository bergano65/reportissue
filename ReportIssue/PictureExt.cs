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
        public Picture()
        {
            this.ID = Guid.NewGuid().ToString();
        }
    }
}
