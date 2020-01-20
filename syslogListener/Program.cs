﻿using System;
using System.Collections.Generic;
using Rebex.Net;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SyslogShared.Models;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using syslogSite.Data;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;

namespace syslogListener
{
    class Program
    {
        private static readonly Queue<SyslogMessage> Queue = new Queue<SyslogMessage>();
        static void Main(string[] args)
        {
            SyslogServer server = new SyslogServer(Syslog.DefaultPort) {TcpEnabled = true, UdpEnabled = true};
            server.MessageReceived += Server_MessageReceived;
            server.Start();
            Console.WriteLine("Starting syslog listener");
            Thread writeMessage = new Thread(MessageHandler);
            // Test method for adding a random syslog entry to the table remove when testing concluded
            /*
            var dbContext = GetContext();
            var Alert = new Alerts
            {
                Facility = "User",
                Received = DateTime.Now,
                HostIP = "127.0.0.1",
                Severity = 5,
                Message = "A Test Error"
            };
            dbContext.alerts.Add(Alert);
            dbContext.SaveChanges();
            */
            writeMessage.Start();
            writeMessage.Join();
            Console.WriteLine("System now listening to syslog messages");
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
                        Facility = m.Facility.ToString(),
                        Received = m.Received,
                        HostIP = m.RemoteEndPoint.ToString(),
                        Severity = (int)m.Severity,
                        Message = m.Text
                    };
                    dbContext.alerts.Add(Alert);
                    if (Alert.Severity < 4)
                    {
                        //Task.Run(() => EmailAlert(m));
                    }
                    dbContext.SaveChanges();
                    Console.WriteLine("Message handled and saved to database");
                }
                catch
                {
                    Thread.Sleep(2000);
                }
            }
        }

        private static void EmailAlert(SyslogMessage m)
        {
            using var message = new MailMessage();
            message.To.Add("SamTaylor0497@gmail.com");
            message.From = new MailAddress("syslogsnapper@gmail.com", "SyslogSnapper");
            message.Subject = "A high priority alert has been received from " + m.RemoteEndPoint.Address;
            message.Body = "The details are as follows: <br/> Received: " + m.Received
                                                                       + "<br/> Facility: " + m.Facility
                                                                       + "<br/> Severity: " + m.Severity
                                                                       + "<br/> Full Message: " + m.Text;
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
                    Credentials = new NetworkCredential("syslogsnapper@gmail.com", "Syslogsn@pper1")


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
