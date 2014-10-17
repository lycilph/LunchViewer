using System;
using System.IO;
using System.Net.Mail;
using System.Reflection;

namespace EmailSender
{
    class Program
    {
        static void Main()
        {
            string text;
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("EmailSender.mail_body.txt"))
            using (var sr = new StreamReader(s))
            {
                text = sr.ReadToEnd();
            }
            
            var mail_message = new MailMessage
            {
                From = new MailAddress("oticon@wip.dk"),
                Subject = "Menu uge 43.",
                Body = text,
                IsBodyHtml = true
            };
            mail_message.To.Add("mail@lunchviewer.cloudapp.net");

            try
            {
                var done = false;
                while (!done)
                {
                    Console.WriteLine("Email sender tool");
                    Console.WriteLine("1. Send email");
                    Console.WriteLine("q. Quite");
                    Console.WriteLine();
                    Console.Write("Your choice: ");
                    
                    var key = Console.ReadKey();
                    switch (key.KeyChar)
                    {
                        case '1':
                            using (var client = new SmtpClient("localhost", 25))
                            {
                                client.Send(mail_message);
                            }
                            Console.WriteLine();
                            Console.Write("Mail sent. Press any key to continue...");
                            Console.ReadKey();
                            Console.Clear();
                            break;
                        case 'q':
                            done = true;
                            Console.WriteLine();
                            Console.WriteLine("Exiting");
                            break;
                        default:
                            Console.Clear();
                            break;
                    }

                }
            }
            catch (SmtpException se)
            {
                Console.WriteLine(se);
            }
        }
    }
}
