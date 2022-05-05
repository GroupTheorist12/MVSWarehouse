using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;


namespace MVS38Pages.Pages
{
    public class SubmitJob : PageModel
    {
        private IWebHostEnvironment _environment;

        private static HostSettings? hostSettings = null;

        private static string[] fileEntries = null;

        private string InvalidateId { get; set; }

        public SubmitJob(IWebHostEnvironment environment)
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

                string homePath = (Environment.OSVersion.Platform == PlatformID.Unix ||
                                   Environment.OSVersion.Platform == PlatformID.MacOSX)
                    ? Environment.GetEnvironmentVariable("HOME")
                    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

                fileEntries = Directory.GetFiles(homePath + hostSettings.JobFilePath);

            }


            Host = hostSettings.Host;
            Port = hostSettings.Port;
        }

        [ViewData]
        public string JclText
        {
            get; set;
        }

        private void SetInvalidateId()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("InvalidateId")))
            {
                HttpContext.Session.SetString("InvalidateId", "1");
                HostHttpUrl = hostSettings.HostHttpUrl + "?InvalidateId=1";
            }
            else
            {
                int iVal = int.Parse(HttpContext.Session.GetString("InvalidateId"));
                iVal++;
                HostHttpUrl = hostSettings.HostHttpUrl + "?InvalidateId=" + iVal.ToString();

            }

        }
        public void OnGet()
        {
            SetInvalidateId();
        }

        public string HostHttpUrl { get; set; }
        [BindProperty]
        public IFormFile Upload { get; set; }
        public async Task OnPostUploadFileAsync()
        {
            var file = Path.Combine(_environment.ContentRootPath, "upload", Upload.FileName);
            using (var fileStream = new FileStream(file, FileMode.Create))
            {
                await Upload.CopyToAsync(fileStream);
            }

            JclText = System.IO.File.ReadAllText(file).Trim();
            HttpContext.Session.SetString("Jcl", JclText);

            SetInvalidateId();

        }

        [BindProperty]
        public string Host { get; set; }
        [BindProperty]
        public string Port { get; set; }

        [BindProperty]
        public string TextValue { get; set; }

        public void OnPostSubmitJcl()
        {
            using TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(Host, int.Parse(Port));

            using NetworkStream netStream = tcpClient.GetStream();

            // Send some data to the peer.
            JclText = TextValue;
            if (JclText != null && JclText != string.Empty)
            {
                byte[] sendBuffer = Encoding.ASCII.GetBytes(JclText);
                netStream.Write(sendBuffer);

            }

            SetInvalidateId();
        }
    }
}