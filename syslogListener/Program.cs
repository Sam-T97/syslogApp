using System;
using System.Collections.Generic;
using System.Linq;
using Rebex.Net;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SyslogShared.Models;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SyslogShared;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace syslogListener
{
    class Program
    {
        private static readonly Queue<SyslogMessage> Queue = new Queue<SyslogMessage>();
        private static string gmailPassword;
        static void Main(string[] args)
        {
            var dbContext = GetContext();
            Console.WriteLine("Enter password for email account used to send alerts");
            gmailPassword = Console.ReadLine(); //TODO secure password for deployments 
            Console.Clear();
            SyslogServer server = new SyslogServer(Syslog.DefaultPort) {TcpEnabled = true, UdpEnabled = true};
            server.MessageReceived += Server_MessageReceived;
            server.Start();
            Console.WriteLine("Starting syslog listener");
            Thread writeMessage = new Thread(MessageHandler);
            writeMessage.Start();
            writeMessage.Join();
            Thread heartBeatThread = new Thread(HeartBeat);
            heartBeatThread.Start();
            heartBeatThread.Join();
        }

        private static void Server_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                Queue.Enqueue(e.Message);
                Console.WriteLine("Message received added to processing buffer");
            }
            catch
            {
                Console.WriteLine("A message has been received that can't be processed message follows:");
                Console.WriteLine(e.Message);
            }
        }
        private static void MessageHandler()
        {
            while (true)
            {
                try
                {
                    SyslogMessage m = Queue.Dequeue();
                    var dbContext = GetContext();
                    
                    var Alert = new Alerts
                    {
                        Facility = (Regex.Match(m.Text,@"(?<=%)(.*?)(?=-)").ToString()),//m.Facility.ToString(),
                        Received = m.Received,
                        HostIP = m.RemoteEndPoint.Address.ToString(),
                        Severity = (int)m.Severity,
                        Message = m.Text,
                        Unread = true,
                        DeviceID = dbContext.Devices.Where(x => x.IP == m.RemoteEndPoint.Address.ToString()).Select(x => x.ID).First()
                    };
                    dbContext.alerts.Add(Alert);
                    if (Alert.Severity < 3)
                    {
                        Task.Run(() => EmailAlert(m));
                    }

                    var saveChanges = dbContext.SaveChanges();
                    Console.WriteLine("Message handled and saved to database");
                }
                catch (Exception e)
                {
                    Thread.Sleep(2000);
                }
            }
        }

        private static void HeartBeat()
        {
            var dbContext = GetContext();
            int interval;
            int.TryParse(dbContext.appvars.Where(a => a.VariableName == "HBInterval")
                .Select(a => a.Value)
                .FirstOrDefault(),out interval);
            while (true)
            {
                try
                {
                    List<Device> devices = new List<Device>();
                    foreach (Device d in dbContext.Devices)
                    {
                        Ping testPing = new Ping();
                        PingReply reply = testPing.Send(d.IP,2000);
                        if (reply.Status == IPStatus.Success) {continue; }
                        devices.Add(d);
                    }

                    if (devices.Count != 0)
                    {
                        Task.Run(() => EmailAlert(null, devices));
                    }

                    Thread.Sleep((int)TimeSpan.FromMinutes(interval).TotalMilliseconds);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Something went wrong with the Heart beater");
                    Console.WriteLine(e.Message);
                    Thread.Sleep((int) TimeSpan.FromMinutes(interval).TotalMilliseconds);
                }
            }
        }
        private static void EmailAlert([Optional]SyslogMessage m, [Optional] List<Device> device)
        {
            using var message = new MailMessage();
            if (m != null)
            {
                message.To.Add(""); //TODO get emails from db
                message.From = new MailAddress("syslogsnapper@gmail.com", "SyslogSnapper");
                message.Subject = "A high priority alert has been received from " + m.RemoteEndPoint.Address;
                message.Body = "The details are as follows: <br/> Received: " + m.Received
                                                                              + "<br/> Facility: " + m.Facility
                                                                              + "<br/> Severity: " + m.Severity
                                                                              + "<br/> Full Message: " + m.Text;
            }
            else
            {
                StringBuilder hostnamesBuilder = new StringBuilder();
                foreach (Device d in device)
                {
                    hostnamesBuilder.AppendLine(d.HostName);
                }
                message.To.Add(""); //TODO get emails from db
                message.From = new MailAddress("syslogsnapper@gmail.com", "SyslogSnapper");
                message.Subject = "Device(s) have failed to heartbeat";
                message.Body = "The non-responsive devices are: <br/> " + hostnamesBuilder;
            }

            message.IsBodyHtml = true;
            try
            {
                using var client = new SmtpClient()
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential("syslogsnapper@gmail.com", gmailPassword)//TODO password here
                };
                client.Send(message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static ApplicationDbContext GetContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseMySql(
                GetConfig().GetConnectionString("DefaultConnection"),
                mySqlOptions =>
                {
                    mySqlOptions.ServerVersion(new Version(8, 0, 17), ServerType.MySql);
                    mySqlOptions.MigrationsAssembly("syslogSite");
                });
            return new ApplicationDbContext(optionsBuilder.Options);
        }
        private static IConfiguration GetConfig()
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .AddEnvironmentVariables();
            return builder.Build();
        }
    }
}
