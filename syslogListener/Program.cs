using System;
using System.Collections.Generic;
using Rebex.Net;
using System.Threading;
namespace syslogListener
{
    class Program
    {
        private static Queue<SyslogMessage> queue = new Queue<SyslogMessage>();
        static void Main(string[] args)
        {
            Console.WriteLine("Starting listener");
            SyslogServer server = new SyslogServer(Syslog.DefaultPort) {TcpEnabled = true, UdpEnabled = true};
            server.MessageReceived += Server_MessageReceived;
            server.Start();
            Console.WriteLine("Starting syslog listener");
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
    }
}
