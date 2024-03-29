﻿ using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ReportIssue;
using ReportIssueUtilities;
using System.Data.Entity.Validation;
using System.IO;
using System.Reflection;

namespace UnitTests
{  
    [TestClass]
    public class UnitTest
    {

        [TestMethod]
        public void TestEncodeError()
        {
            ObservableCollection<Error> collection = new ObservableCollection<Error>();

            Error e1 = new Error();
            e1.C1 = "C1";
            e1.C2 = "C2";
            e1.C3 = "C3";
            e1.C4 = "C4";
            e1.C5 = "C5";


            Error e2 = new Error();
            e2.C1 = "C1";
            e2.C2 = "C2";
            e2.C3 = "C3";
            e2.C4 = "C4";
            e2.C5 = "C5";

            collection.Add(e1);
            collection.Add(e2);

            string errors = ReportIssueUtilities.EncodeErrors(errors);
        }


        [TestMethod]
        public void TestEditIssue()
        {
            EditIssueDlg editIssueDlg = new EditIssueDlg(null, null);
            editIssueDlg.ShowDialog();
        }

        [TestMethod]
        public void TestSaveIssue()
        {
            try
            {
                Issue i = new Issue();
                i.BugPath = "bp";
                i.English = "e";
                i.Fixed = true;
                i.Url = "l";
                i.Selected = true;
                i.IssueTxt = "it";
                i.Product = "p";
                i.Reason = "r";
                i.Right = "rr";
                i.Submitted = true;
                i.Template = "t";
                i.Url = "u";
                i.Wrong = "w";
                i.WhereFound = "";
                i.Parameter1 = "p1";
                i.Parameter2 = "p2";
                i.Parameter3 = "p3";
                i.Parameter4 = "p4";
                i.Parameter5 = "p5";
                i.Parameter6 = "p6";
                i.Parameter7 = "p7";
                i.Parameter8 = "p8";
                i.Parameter9 = "p9";
                i.Parameter10 = "p9";
                i.Parameter11 = "p11";

                i.Parameter12 = "p1";
                i.Parameter13 = "p2";
                i.Parameter14 = "p3";
                i.Parameter15 = "p4";
                i.Parameter16 = "p5";
                i.Parameter17 = "p6";
                i.Parameter18 = "p7";
                i.Parameter19 = "p9";
                i.Parameter20 = "p20";
                i.UpdateTime = DateTime.Now;
                //            i.Save();
                RIDataModelContainer d = new RIDataModelContainer();
                d.Issues.Add(i);
                d.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                var ee = e.EntityValidationErrors;
                foreach (DbEntityValidationResult r in ee)
                {
                    foreach (DbValidationError err in r.ValidationErrors)
                    {

                    }
                }
            }
            catch (Exception e)
            {
                Exception ie = e.InnerException;
            }
        }

    }
}

 