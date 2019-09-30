using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace NetGameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server serv = new Server();
            serv.Start("127.0.0.1", 1234);
            //serv.Start("192.160.3.69", 1234);
            while (true)
            {
                string str = Console.ReadLine();
                switch(str)
                {
                    case "quit":
                        return;
                }
            }
            //Socket listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
            //IPEndPoint ipEp = new IPEndPoint(ipAdr, 1234);
            //listenfd.Bind(ipEp);
            //listenfd.Listen(0);
            //while(true)
            //{
            //    Socket connect = listenfd.Accept();
            //    //recv
            //    Byte[] readBuff = new Byte[1024];
            //    int count = connect.Receive(readBuff);
            //    String str = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
            //    Console.WriteLine("服务器接收 ： "+str);
            //    //send
            //    Byte[] bytes = System.Text.Encoding.UTF8.GetBytes("serv recv " + str);
            //    connect.Send(bytes);
            //}
        }
    }
}
