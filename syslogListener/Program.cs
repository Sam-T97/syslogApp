using System;
using System.Collections.Generic;
using Rebex.Net;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Extensions.Configuration;
using SyslogShared;
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
            using var dbContext = GetContext();
            var alert = new Alerts {ID = 123, message = "test"};
            dbContext.Add(alert);
            dbContext.SaveChanges();
            Thread writeMessage = new Thread(new ThreadStart(test));
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
        private static void test()
        {
            while (true)
            {
                try
                {
                    SyslogMessage m = queue.Dequeue();
                    Console.WriteLine("{0} {1} {2} {3}",m.RemoteEndPoint,m.Facility,m.Severity,m.Text);
                }
                catch
                {
                    // ignored
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
