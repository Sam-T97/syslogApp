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
using System.Threading.Tasks;
using SyslogShared;
using System.Net.NetworkInformation;
using System.Text;

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
            Console.WriteLine("Starting the heart beater thread");
            Thread heartBeatThread = new Thread(HeartBeat);
            heartBeatThread.Start();
            writeMessage.Join();
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
                        Facility = (Regex.Match(m.Text,@"(?<=%)(.*?)(?=-)").ToString()), //Facility/Mneonic extraction
                        Received = m.Received, //When
                        HostIP = m.RemoteEndPoint.Address.ToString(), //Who
                        Severity = (int)m.Severity, //How serious
                        Message = m.Text, //Entire message
                        Unread = true, //Set unread flag
                        DeviceID = dbContext.Devices.Where(x => x.IP == m.RemoteEndPoint.Address.ToString()).Select(x => x.ID).First() //Grab the device hostname from the IP field
                    };
                    dbContext.alerts.Add(Alert);
                    if (Alert.Severity <= 3)
                    {
                        Task.Run(() => EmailAlert(m));
                    }

                    var saveChanges = dbContext.SaveChanges();
                    Console.WriteLine("Message handled and saved to database");
                }
                catch
                {
                    Thread.Sleep(2000);
                }
            }
        }

        private static void HeartBeat()
        {
            var dbContext = GetContext();
            int.TryParse(dbContext.appvars.Where(a => a.VariableName == "HBInterval")
                .Select(a => a.Value)
                .FirstOrDefault(),out var interval);
            if (interval < 10) interval = 10;
            while (true)
            {
                try
                {
                    Console.WriteLine("Starting heartbeat checks");
                    List<Device> devices = new List<Device>();
                    Ping testPing = new Ping();
                    foreach (Device d in dbContext.Devices)
                    {
                        PingReply reply = testPing.Send(d.IP,2000);
                        if (reply.Status == IPStatus.Success) {continue; }
                        devices.Add(d);
                    }
                    testPing.Dispose();
                    if (devices.Count != 0)
                    {
                        Task.Run(() => EmailAlert(null, devices));
                    }
                    Console.WriteLine("Heartbeats concluded sleeping for interval");
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
            var dbContext = GetContext();
            using var message = new MailMessage();
            List<string> emails = new List<string>();
            if (m != null)
            {
                try
                {
                    emails = dbContext.MailingListMembers.Where(i => i.MailingListID == 1)
                        .Select(e => e.Email).ToList();
                    foreach (var email in emails)
                    {
                        message.To.Add(email);
                    }
                    message.From = new MailAddress("syslogsnapper@gmail.com", "SyslogSnapper");
                    message.Subject = "A high priority alert has been received from " + m.RemoteEndPoint.Address;
                    message.Body = "The details are as follows: <br/> Received: " + m.Received
                                                                                  + "<br/> Facility: " + m.Facility
                                                                                  + "<br/> Severity: " + m.Severity
                                                                                  + "<br/> Full Message: " + m.Text;
                }catch(Exception e) { Console.WriteLine("There was a problem making the email message: " + e.Message); }
            }
            else
            {
                try
                {
                    StringBuilder hostnamesBuilder = new StringBuilder();
                    foreach (Device d in device)
                    {
                        hostnamesBuilder.AppendLine(d.HostName + "<br/>");
                    }

                    emails = dbContext.MailingListMembers.Where(i => i.MailingListID == 2)
                        .Select(e => e.Email).ToList();
                    foreach (var email in emails)
                    {
                        message.To.Add(email);
                    }

                    message.From = new MailAddress("syslogsnapper@gmail.com", "SyslogSnapper");
                    message.Subject = "Device(s) have failed to heartbeat";
                    message.Body = "The non-responsive devices are: <br/> " + hostnamesBuilder;
                }catch (Exception e) { Console.WriteLine("There was a problem making the email message: " + e.Message); }
            }
            message.IsBodyHtml = true;
            try
            {
                using var client = new SmtpClient()
                {
                    Host = "smtp.gmail.com",
                    TargetName = "STARTTLS/smtp.gmail.com",
                    Port = 587,
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    EnableSsl = true,
                    Credentials = new NetworkCredential("syslogsnapper@gmail.com", gmailPassword)
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
