using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.IO;

namespace MVS38Pages
{
    public class JobInfo
    {
        public string JobName { get; set; }
        public string JobNumber { get; set; }

        public string Path { get; set; }

        public bool IsJob{get;set;}

        public JobInfo(string p)
        {
            Path = p;

            IsJob = false;
            FileInfo fi = new FileInfo(Path);
            JobName = fi.Name;
            JobNumber = fi.Name;

            string JobContent = System.IO.File.ReadAllText(Path);

            if (JobContent.IndexOf("START  JOB") > 0)
            {
                IsJob = true;
                //****A  START  JOB   13  HERHE1    
                int ind = JobContent.IndexOf("START  JOB");

                JobNumber = "Job " + JobContent.Substring(ind + 10, 7).Trim();
                JobName = JobContent.Substring(ind + 17, 8).Trim();
            }


        }
    }
}