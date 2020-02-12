using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApaxLib;
using System.Net.Sockets;
using System.IO;

namespace Apax
{
        

    class TCPCombo
    {
        static private object lockSentTCP = new object();

        private static void SentToServerInet(string IPAddr, int port, string strMes)
        {
            TcpClient client = null;
            NetworkStream stream = null;
            BinaryWriter writer = null;
            try
            {

                using (client = new TcpClient(IPAddr, port))
                {
                    using (stream = client.GetStream())
                    {
                        Byte[] sendBytes;
                        sendBytes = Encoding.UTF8.GetBytes(strMes);
                        stream.Write(sendBytes, 0, sendBytes.Length);
                    }
                    client.Close();
                    //return "";
                }

            }
            catch (Exception e)
            {
                //Util.errorMessage(e.Message,"Error sent LOG to tcp server");
                //return e.Message;
            }
        }

        private static void loggerSays(string message)
        {
            SentToServerInet(Server.GetIP1Serv(), Server.GetPortServ(), message);
            SentToServerInet(Server.GetIP2Serv(), Server.GetPortServ(), message);
        }

        public static void Run() // функция запуска потока отправки сообщений со Щита на сервера Combo и Rpu
        {
            //Util.message("Отправляем сообщения со Щита на сервера Combo и Rpu");            
            do
            {
                lock (lockSentTCP)
                {
                    System.Threading.Thread.Sleep(400);
                    if (Util.messageLine.Count > 0)
                    {

                        for (int i = 0; i < Util.messageLine.Count; i++)
                        {
                            loggerSays(Util.messageLine[i]);
                        }
                        //Util.tcpMessage.Clear();
                    }
                    //System.Threading.Thread.Sleep(400);
                }
            } while (true);
        }
    }    
}
