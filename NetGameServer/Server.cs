using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace NetGameServer
{
    class Server
    {
        public Socket listenfd;
        public Conn[] conns;
        public int maxConn = 50;
        MySqlConnection sqlConn;

        public int NewIndex()
        {
            if (conns == null)
                return -1;
            for(int i=0;i<conns.Length;i++)
            {
                if (conns[i]==null)
                {
                    conns[i] = new Conn();
                    return i;
                }
                else if(conns[i].isUse == false)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Start(string host,int port)
        {
            //数据库
            string connStr = "Database=db1;Data Source=127.0.0.1;";
            connStr += "User Id=root;Password=Hook1990*;port=3306";
            sqlConn = new MySqlConnection(connStr);
            try
            {
                sqlConn.Open();
            }catch(Exception e)
            {
                Console.WriteLine("[数据库]连接失败！！"+e.Message);
                return;
            }
            conns = new Conn[maxConn];
            for(int i=0;i<maxConn;i++)
            {
                conns[i] = new Conn();
            }
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAdr = IPAddress.Parse(host);
            IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
            listenfd.Bind(ipEp);
            listenfd.Listen(maxConn);
            listenfd.BeginAccept(AcceptCb, null);
            Console.WriteLine("[服务器] 启动成功");
        }
        private void AcceptCb(IAsyncResult ar)
        {
            Socket socket = listenfd.EndAccept(ar);
            int index = NewIndex();
            try
            {
                if (index < 0)
                {
                    socket.Close();
                    Console.WriteLine("[警告]连接已满");
                }
                else
                {
                    Conn conn = conns[index];
                    conn.Init(socket);
                    string adr = conn.GetAddress();
                    Console.WriteLine("客户端链接[" + adr + "] coon池ID: " + index);
                    conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
                    listenfd.BeginAccept(AcceptCb, null);
                }
            }catch (Exception e)
            {
                Console.WriteLine("AccpetCb 失败： " + e.Message);
            }
            
        }

        private void ReceiveCb(IAsyncResult ar)
        {
            Conn conn = (Conn)ar.AsyncState;
            try
            {
                int count = conn.socket.EndReceive(ar);
                if (count <=0)
                {
                    Console.WriteLine("收到 [" + conn.GetAddress() + "]断开链接");
                    conn.Close();
                    return;
                }
                string str = System.Text.Encoding.UTF8.GetString(conn.readBuff, 0, count);
                Console.WriteLine("收到数据：" + str);
                HandleMsg(conn, str);
                str = conn.GetAddress() + ":" + str;
                byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
                for(int i=0;i< conns.Length;i++)
                {
                    if (conns[i] == null)
                        continue;
                    if (!conns[i].isUse)
                        continue;
                    conns[i].socket.Send(bytes);
                }
                conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn.BuffRemain(),
                    SocketFlags.None, ReceiveCb, conn);
            }catch(Exception e)
            {
                Console.WriteLine("断开链接"+conn.GetAddress()+e.Message);
                conn.Close();
            }
        }

        public void HandleMsg(Conn conn,string str)
        {
            if (str == "_GET")
            {
                string cmdStr = "select * from msg order by id desc limit 10;";
                MySqlCommand cmd = new MySqlCommand(cmdStr,sqlConn);
                try
                {
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    str = "";
                    while(dataReader.Read())
                    {
                        str += dataReader["id"] + ":" + dataReader["msg"] + "\n\r";

                    }
                    dataReader.Close();
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
                    conn.socket.Send(bytes);
                }
                catch { }
            }
            else
            {
                string cmdStrFormat = "insert into msg(name,msg)value('{0}','{1}');";
                string cmdStr = string.Format(cmdStrFormat, conn.GetAddress(), str);
                MySqlCommand cmd = new MySqlCommand(cmdStr,sqlConn);
                try
                {
                    cmd.ExecuteNonQuery();
                }catch(Exception e)
                {
                    Console.WriteLine("[数据库]插入失败" + e.Message);
                }
            }
        }
    }
}
