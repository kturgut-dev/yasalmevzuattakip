using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KYS.YasalMevzuatTakip
{
    public class KYSJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(() =>
            {
                new JobFuncs().CheckNewspaperToDay();
            });
        }
    }
}
