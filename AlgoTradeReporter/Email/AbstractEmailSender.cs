using AlgoTradeReporter.Config;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mail;

namespace AlgoTradeReporter.Email
{
    abstract class AbstractEmailSender
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(AbstractEmailSender));

        protected System.Net.Mail.MailMessage mail;
        protected string subject;
        protected string body;
        protected SmtpClient mailClient;
        protected Attachment attachment;
        //add by zhaoyu for qq email implicit SSL
        protected System.Web.Mail.MailMessage mailQQ;

        public AbstractEmailSender()
        {
            this.mail = new System.Net.Mail.MailMessage();
            this.mail.IsBodyHtml = false;
            this.mail.BodyEncoding = System.Text.Encoding.UTF8;
            this.mail.Priority = System.Net.Mail.MailPriority.Normal;
            this.mailClient = new SmtpClient();
            this.mailQQ = new System.Web.Mail.MailMessage();            
        }

        protected void clearReceiver()
        {
            this.mail.To.Clear();
            this.mail.CC.Clear();
            this.mailQQ.To = "";
            this.mailQQ.Cc = "";
        }

        protected void initMailClient(EmailConfig config_)
        {
            mailClient.Host = config_.getEmailServer();
            mailClient.Port = config_.getPort();
            mailClient.EnableSsl = config_.getIsSSL();
            mailClient.Credentials = new NetworkCredential(config_.getSender(), config_.getPassword());      
            if (config_.getEmailServer().Equals("smtp.exmail.qq.com"))
            {
                mailQQ.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", "1"); //身份验证  
                mailQQ.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusername", config_.getSender()); //邮箱登录账号，这里跟前面的发送账号一样就行  
                mailQQ.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendpassword", config_.getPassword()); //这个密码要注意：如果是一般账号，要用授权码；企业账号用登录密码  
                mailQQ.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpserverport", config_.getPort());//端口  
                mailQQ.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpusessl", config_.getIsSSL());//SSL加密  
            }
        }

        public void initSender(string senderAddress_, string senderName_)
        {
            mail.From = new MailAddress(senderAddress_, senderName_);
            mailQQ.From = mail.From.ToString();
        }

        public void initReceiver(List<string> receivers_)
        {
            mail.To.Clear();
            mailQQ.To = "";
            foreach (string receiverAddress in receivers_)
            {
                mail.To.Add(new MailAddress(receiverAddress));
                mailQQ.To += receiverAddress + ";";
            }
        }

        public void initCClist(List<string> cc_)
        {
            mail.CC.Clear();
            mailQQ.Cc = "";
            if (cc_.Count == 0)
            {
                return;
            }
            else
            {
                foreach (string ccReceiver in cc_)
                {
                    mail.CC.Add(new MailAddress(ccReceiver));
                    mailQQ.Cc += ccReceiver + ";";
                }
            }
        }

        public void addAttachment(string file_)
        {
            attachment = new Attachment(file_, new ContentType("application/vnd.ms-excel"));
            //attachment.NameEncoding = System.Text.Encoding.UTF8;
            //attachment.TransferEncoding = TransferEncoding.QuotedPrintable;
            
           // 算法一期交易报告 to20123333333333333333333
            //attachment.Name = "算法一期交易报告测试_1101_1234.xls";
            mail.Attachments.Add(attachment);
            mailQQ.Attachments.Add(new System.Web.Mail.MailAttachment(file_));
        }

        public void addRarAttachment(string file_)
        {
            string rarFile = file_ + ".rar";

            FileStream inputStream = new FileStream(file_, FileMode.Open, FileAccess.Read);
            FileStream outputStream = new FileStream(rarFile, FileMode.Create, FileAccess.Write);
            byte[] buffer = new byte[inputStream.Length];
            inputStream.Read(buffer, 0, buffer.Length);

            GZipStream compressionStream = new GZipStream(outputStream, CompressionMode.Compress);
            compressionStream.Write(buffer, 0, buffer.Length);
            compressionStream.Close();
            inputStream.Close();
            outputStream.Close();

            attachment = new Attachment(rarFile);
            mail.Attachments.Add(attachment);
            mailQQ.Attachments.Add(new System.Web.Mail.MailAttachment(rarFile));
        }

        public void send()
        {
            mail.Subject = subject;
            mail.Body = body;
            logger.Info("To " + mail.To.ToString());
            logger.Info("CC " + mail.CC.ToString());
            Console.Out.WriteLine("To " + mail.To.ToString());
            Console.Out.WriteLine("CC " + mail.CC.ToString());

            try
            {
                if (!mailClient.Host.Equals("smtp.exmail.qq.com"))
                {
                    mailClient.Send(mail);
                }
                else
                {                                        
                    mailQQ.Subject = mail.Subject.ToString();
                    mailQQ.BodyFormat = System.Web.Mail.MailFormat.Text;
                    mailQQ.Body = mail.Body;                    
                    System.Web.Mail.SmtpMail.SmtpServer = "smtp.exmail.qq.com";         
                    System.Web.Mail.SmtpMail.Send(mailQQ);                    
                }
                logger.Info("Succeed.");
                Console.Out.WriteLine("Succeed.");
                foreach (Attachment attachment in mail.Attachments)
                {
                    attachment.Dispose();
                }
            }
            catch (Exception e_)
            {
                logger.Error("Failed.");
                logger.Error(e_.StackTrace);
                logger.Error(e_.Message);
                logger.Fatal(e_);
                Console.Out.WriteLine("Failed to send email to " + mail.To.ToString());
                throw new Exception("Failed to send Email to " + mail.To.ToString(), e_);
            }
        }

        public string logRecipents()
        {
            string mailReceivers = "To " + mail.To;
            logger.Info("To " + mail.To);
            Console.WriteLine("To " + mail.To);
            string CC = null;
            if (0 != mail.CC.Count)
            {
                CC = " CC ";
                foreach (MailAddress cc in mail.CC)
                {
                    CC += " " + cc.Address;
                }
                logger.Info(CC);
                Console.WriteLine(CC);
            }
            return mailReceivers + CC;
        }

        public void dispose()
        {
            mail.Dispose();
            mailClient.Dispose();            
        }
    }
}
