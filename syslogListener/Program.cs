using System;
using System.Collections.Generic;
using Rebex.Net;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SyslogShared.Models;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using syslogSite.Data;

namespace syslogListener
{
    class Program
    {
        private static Queue<SyslogMessage> queue = new Queue<SyslogMessage>();
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
        }

        private static void Server_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                queue.Enqueue(e.Message);
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
                    SyslogMessage m = queue.Dequeue();
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
                    dbContext.SaveChanges();
                }
                catch
                {
                    // ignored queue is empty 
                }
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
