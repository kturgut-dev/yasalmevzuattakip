using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using KYS.YasalMevzuatTakip.Helper;
using KYS.YasalMevzuatTakip.Helpers;
using KYS.YasalMevzuatTakip.Models;
using RestSharp;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using UnidecodeSharpFork;

namespace KYS.YasalMevzuatTakip
{
    public class JobFuncs
    {
        public Loog4NetHelper loog4NetHelper { get; set; }
        public Models.ProjectSetting _projectSetting { get; set; }
        private string _Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\YasalMevzuatTakip\\ResmiGazete\\";
        public JobFuncs()
        {
            loog4NetHelper = new Loog4NetHelper(this.GetType());
            _projectSetting = ConfigurationManagerHelper.GetProjectSetting();
        }
        public void CheckNewspaperToDay()
        {
            try
            {
                string date = DateTime.Now.ToString("yyyyMMdd");

                //_Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\YasalMevzuatTakip\\ResmiGazete\\";
                if (!System.IO.Directory.Exists(_Path))
                    System.IO.Directory.CreateDirectory(_Path);

                string downLoadPath = string.Format("https://www.resmigazete.gov.tr/eskiler/{0}/{1}/{2}.pdf", DateTime.Now.Year, DateTime.Now.Month.ToString("00"), date);

                string filename = System.AppDomain.CurrentDomain.BaseDirectory;
                filename = _Path + date + "_ResmiGazete.pdf";

                bool pageRequestResponse = this.RestSharpSendRequest(@"https://www.resmigazete.gov.tr/", string.Format("/eskiler/{0}/{1}/{2}.pdf", DateTime.Now.Year, DateTime.Now.Month.ToString("00"), date), filename);

                if (!pageRequestResponse)
                    return;

                string[] keywords = _projectSetting.SearchKeys;

                List<int> pages = new List<int>();
                string res = string.Empty;
                string icindekilerPDF = _Path + date + "_Icindekiler.pdf";
                if (File.Exists(filename))
                {
                    PdfReader pdfReader = new PdfReader(filename);
                    if (keywords != null)
                        foreach (string key in keywords)
                        {
                            pages = new List<int>();
                            for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                            {
                                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                                string currentPageText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);
                                if (currentPageText.Unidecode().ToLower().Contains(key.Unidecode().ToLower()))
                                    pages.Add(page);
                            }
                            using (PdfReader reader = new PdfReader(filename))
                            {
                                reader.SelectPages(reader.NumberOfPages.ToString());
                                PdfStamper stamper = new PdfStamper(reader, new FileStream(icindekilerPDF, FileMode.Create, FileAccess.Write));
                                stamper.Close();
                                //reader.Close();
                            }

                            res += "<strong>" + key + "</strong>: " + string.Join(",", pages.Select(n => n.ToString()).ToArray()) + "<br>";
                        }
                    pdfReader.Close();
                }

                FileInfo fileInfoDownloaded = new FileInfo(filename);
                double fileInfoDownloadedMB = SizeSuffix(fileInfoDownloaded.Length);

                AttachmentCollection Attachmentc = new MailMessage().Attachments;
                #region Gazete PDF
                if (fileInfoDownloadedMB < 10)
                    Attachmentc.Add(AttachmentCreateToPath(filename));
                #endregion

                #region İçindekiler PDF
                if (File.Exists(icindekilerPDF))
                    Attachmentc.Add(AttachmentCreateToPath(icindekilerPDF));
                #endregion

                if (!File.Exists(filename))
                    loog4NetHelper.Fatal("Dosya Bulunamadı. Path: " + filename);
                else
                {
                    string MailMessage = _projectSetting.MailConf.MailTemplate;
                    string MailTitle = _projectSetting.MailConf.MailTitle;

                    MailMessage = string.Format(MailMessage, fileInfoDownloadedMB <= 10
                        ? "Bugün tarihli resmi gazete ektedir."
                        : "Bugün tarihli resmi gazetenin dosya boyutu büyük olduğundan dolayı <a href='" + downLoadPath + "'>buradan</a> ulaşabilirsiniz. (Dosya Boyutu: " + fileInfoDownloadedMB + " MB)", "İçindekiler sayfası ekte bulunmaktadır.");

                    MailMessage = MailMessage + res;

                    MailGonderOffice365(MailMessage, MailTitle, Attachmentc);

                    if (File.Exists(icindekilerPDF))
                        File.Delete(icindekilerPDF);
                }
            }
            catch (Exception ex)
            {
                loog4NetHelper.Fatal(ex.Message);
            }
        }

        private Attachment AttachmentCreateToPath(string filePath)
        {
            System.IO.File.WriteAllBytes(filePath, File.ReadAllBytes(filePath));
            Attachment ataachmentFile = new Attachment(filePath, MediaTypeNames.Application.Octet);
            ContentDisposition disposition = ataachmentFile.ContentDisposition;
            disposition.CreationDate = System.IO.File.GetCreationTime(filePath);
            disposition.ModificationDate = System.IO.File.GetLastWriteTime(filePath);
            disposition.ReadDate = System.IO.File.GetLastAccessTime(filePath);
            disposition.FileName = System.IO.Path.GetFileName(filePath);
            disposition.Size = new FileInfo(filePath).Length;
            disposition.DispositionType = DispositionTypeNames.Attachment;
            return ataachmentFile;
        }

        private bool RestSharpSendRequest(string baseUrl, string url, string fileName)
        {
            try
            {
                RestRequest request = new RestRequest(url, Method.GET);
                RestClient client = new RestClient(baseUrl);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception($"Unable to download file");
                response.RawBytes.SaveAs(fileName);
                return true;
            }
            catch (Exception ex)
            {
                loog4NetHelper.Fatal(ex.Message);
                return false;
            }
        }

        static double SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return SizeSuffix(-value, decimalPlaces); }
            if (value == 0) { return Convert.ToDouble(string.Format("{0:n" + decimalPlaces + "}", 0)); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return Convert.ToDouble(string.Format("{0:n}",
                adjustedSize));
        }

        public bool MailGonderOffice365(string mailbody, string subject, System.Net.Mail.AttachmentCollection attachments = null)
        {
            MailConf mailConf = _projectSetting.MailConf;
            MailRecipient MailRecipients = _projectSetting.MailRecipients;
            string emailadresi = string.Empty, CC = string.Empty, BCC = string.Empty;

            emailadresi = string.Join(",", MailRecipients.MailTo);

            if (MailRecipients.MailCC != null)
                CC = string.Join(",", MailRecipients.MailCC);

            if (MailRecipients.MailBCC != null)
                BCC = string.Join(",", MailRecipients.MailBCC);

            bool Result = true;
            System.Net.Mail.MailMessage msgs = new System.Net.Mail.MailMessage();
            msgs.From = new MailAddress(mailConf.MailAddress);

            if (!string.IsNullOrEmpty(emailadresi))
                msgs.To.Add(emailadresi);

            if (!string.IsNullOrEmpty(CC))
                msgs.CC.Add(CC);

            if (!string.IsNullOrEmpty(BCC))
                msgs.Bcc.Add(BCC);

            msgs.Subject = subject;
            msgs.Body = mailbody;
            msgs.IsBodyHtml = true;
            msgs.SubjectEncoding = Encoding.UTF8;
            msgs.BodyEncoding = Encoding.UTF8;

            if (attachments != null)
                foreach (Attachment item in attachments)
                    msgs.Attachments.Add(item);

            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(mailConf.MailAddress, mailConf.Password);
            client.Port = mailConf.Port;
            client.Host = mailConf.Host;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = mailConf.EnableSSL;
            if (!string.IsNullOrEmpty(mailConf.TargetName))
                client.TargetName = mailConf.TargetName;
            try
            {
                client.Send(msgs);
                loog4NetHelper.Info("Mail Başarıyla Gönderildi. Alıcı Adres: " + emailadresi);
            }
            catch (Exception ex)
            {
                loog4NetHelper.Info("Mail Gönderilemedi. Alıcı Adres: " + emailadresi);
                loog4NetHelper.Fatal(ex.Message);
                Result = false;
            }
            finally
            {
                msgs.Dispose();
            }

            return Result;
        }
    }


}

