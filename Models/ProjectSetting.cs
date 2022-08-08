using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KYS.YasalMevzuatTakip.Models
{
    public class ProjectSetting
    {
        public MailConf MailConf { get; set; }
        public MailRecipient MailRecipients { get; set; }
        public string[] SearchKeys { get; set; }
        public bool LoggingIsOn { get; set; }
    }
}
