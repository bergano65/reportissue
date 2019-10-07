using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportIssue
{
    public partial class Filter
    {
        public Filter()
        {
            this.ID = Guid.NewGuid().ToString();
            this.Reset();
        }
        public void Reset()
        {
            this.Issue = "";
            this.Product = "";
            this.Wrong = "";
            this.Right = "";
            this.Submitted = "Any";
            this.Fixed = "Any";
            this.Status = "Any";
        }
    }
}
