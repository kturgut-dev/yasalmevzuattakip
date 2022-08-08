using KYS.YasalMevzuatTakip.Helpers;
using System;
using System.Reflection;

namespace KYS.YasalMevzuatTakip
{
    class Program
    {
        static void Main(string [] args)
        {
            Console.Title = "KYS Yasal Mevzuat Takip Uyarı Sistemi V" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine("Proje Başlatıldı. Tarih: " + DateTime.Now.ToString());

            KYSJobScheduler.Start();

            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
        }


    }
}
