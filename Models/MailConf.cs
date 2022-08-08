using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KYS.YasalMevzuatTakip.Models
{
    public class MailConf
    {
        public string MailAddress { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string TargetName { get; set; }
        public bool EnableSSL { get; set; }
        public string MailTemplate { get; set; }
        public string MailTitle { get; set; }
    }
}
