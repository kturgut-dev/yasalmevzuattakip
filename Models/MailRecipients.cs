using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KYS.YasalMevzuatTakip.Models
{
    public class MailRecipient
    {
        public string[] MailTo { get; set; }
        public string[] MailCC { get; set; }
        public string[] MailBCC { get; set; }
    }
}
