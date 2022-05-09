using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;



namespace MVS38Pages.Pages
{
    public class ViewPrintSpool : PageModel
    {
        private IWebHostEnvironment _environment;

        private static HostSettings? hostSettings = null;

        private static string[] fileEntries = null;

        [ViewData]
        public string JclText
        {
            get; set;
        }


        public ViewPrintSpool(IWebHostEnvironment environment)
        {

            _environment = environment;
            //_httpContextAccessor = httpContextAccessor;
            if (hostSettings == null)
            {
                JclText = string.Empty;
                var file = Path.Combine(_environment.ContentRootPath, "", "hostsettings.json");

                string json = System.IO.File.ReadAllText(file);
                hostSettings =
                    JsonSerializer.Deserialize<HostSettings>(json);



            }
            string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
                               Environment.OSVersion.Platform == PlatformID.MacOSX)
                ? Environment.GetEnvironmentVariable("HOME")
                : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
            fileEntries = Directory.GetFiles(homePath + hostSettings.JobFilePath);


        }

        [BindProperty]
        public string SelectedCategoryId { set; get; }

        public List<SelectListItem> CategoryItems { set; get; }

        public static int CompareFiles(string file1, string file2)
        {
            FileInfo fi1 = new FileInfo(file1);
            FileInfo fi2 = new FileInfo(file2);
            return fi2.CreationTime.CompareTo(fi1.CreationTime);
        }

        public string JobNumber { get; set; }
        public string JobName { get; set; }

        public void OnGet()
        {

            List<SelectListItem> items = new List<SelectListItem>();

            List<string> sorted = fileEntries.ToList();
            JobInfo jiCheck = null;

            sorted.Sort(CompareFiles);

            JobContent = System.IO.File.ReadAllText(sorted[0]);

            char gug = (char)0xc;

            JobContent = JobContent.Replace(gug, '\n');

            int ind = 0;
            foreach (string fil in sorted)
            {
                JobInfo ji = new JobInfo(fil);

                if (ji.IsJob)
                {
                    if (ind == 0)
                    {
                        jiCheck = ji;
                        ind++;
                    }

                    items.Add(new SelectListItem { Text = $"{ji.JobName} - ({ji.JobNumber})", Value = ji.Path });
                }
            }

            if (jiCheck != null)
            {
                JobContent = System.IO.File.ReadAllText(jiCheck.Path);

                JobContent = JobContent.Replace(gug, '\n');

                JobName = jiCheck.JobName;
                JobNumber = jiCheck.JobNumber;
            }

            CategoryItems = items;
        }

        public string JobContent { get; set; }
        public IActionResult OnPost()
        {
            var selectedCategoryId = this.SelectedCategoryId;
            List<SelectListItem> items = new List<SelectListItem>();

            List<string> sorted = fileEntries.ToList();

            sorted.Sort(CompareFiles);


            foreach (string fil in sorted)
            {
                JobInfo ji = new JobInfo(fil);

                if (ji.IsJob)
                {
                    items.Add(new SelectListItem { Text = $"{ji.JobName} - ({ji.JobNumber})", Value = ji.Path });
                }
            }

            CategoryItems = items;

            JobContent = System.IO.File.ReadAllText(selectedCategoryId);

            JobInfo jiCheck = new JobInfo(SelectedCategoryId);
            JobName = jiCheck.JobName;
            JobNumber = jiCheck.JobNumber;

            char gug = (char)0xc;

            JobContent = JobContent.Replace(gug, '\n');

            return new PageResult();
        }
    }
}