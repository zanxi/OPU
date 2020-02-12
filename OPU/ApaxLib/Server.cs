using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// В данной библиотеке создается класс Server его свойствами и методами, определяющий работу клиент серверного приложения: 
// 1. проводится инициализация сервера и контекста устройств


namespace ApaxLib
{
    public static class Server
    {
        static private List<Context> listContext = null; 
        static private string name;
        static private string description;
        static private int pointRoot=0;
        static private int pointVariables;
        static private int pointDevices;
        static private int pointModels;
        static private int pointBlinds;
        static private int pointConstants;
        static public int stepserver=1000;
        static public int recondevice = 10000;
        static public MainLoop mainloop;
        static public Thread threadml;
        static public LoopDevices loopdevices;
        static public Thread threadld;
        static public Thread messthr;
        static private int port;
        static private string ip1;
        static private string ip2;
        static private int lines;

        public static int GetNumLinesMessageApax()
        {
            return lines;
        }

        public static void InitServer(string names, string desc, int plines, int step, int recon, int pport, string pip, string pip2)
        {
            name = names;
            port = pport;
            ip1 = pip;
            ip2 = pip2;
            lines = plines;
            //Util.errorMessage(port+"; " + ip1 + "; " + ip2 + "; ", "");

            description = desc;
            stepserver = step;
            recondevice = recon;
            listContext = new List<Context>();
            makeRoot();
            makeDevices();
            makeVariables();
            makeModels();
            makeConstants();
            makeBlinds();
            
        }


        static private void SentToServerInet(string IPAddr, int port, string strMes)
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

        public static string GetIP1Serv()
        {
            return ip1;
        }

        public static string GetIP2Serv()
        {
            return ip2;
        }

        public static int GetPortServ()
        {
            return port;
        }

        public static void loggerSays(string message)
        {   
            SentToServerInet(ip1,port,message);
            SentToServerInet(ip2, port, message);            
        }

        private static void makeConstants()
        {
            Context con = new Context("Constants", "Root all constants on this server", Util.TypeContext.Constants, pointRoot);
            pointConstants = listContext.Count;
            appendContext(con);
            appendChaild(pointRoot, pointConstants);
        }

        private static void makeBlinds()
        {
            Context con = new Context("Blinds", "Root all blinds on this server", Util.TypeContext.Blinds, pointRoot);
            pointBlinds = listContext.Count;
            appendContext(con);
            appendChaild(pointRoot, pointBlinds);
        }
        private static void makeVariables()
        {
            Context con = new Context("Variables", "Root all variables on this server", Util.TypeContext.Variables, pointRoot);
            pointVariables = listContext.Count;
            appendContext(con);
            appendChaild(pointRoot, pointVariables);
        }
        private static void makeDevices()
        {
            Context con = new Context("Devices", "Root all devices on this server", Util.TypeContext.Devices,pointRoot);
            pointDevices = listContext.Count;
            appendContext(con);
            appendChaild(pointRoot, pointDevices);
        }
        private static void makeModels()
        {
            Context con = new Context("Models", "Root all models on this server", Util.TypeContext.Models,pointRoot);
            pointModels = listContext.Count;
            appendContext(con);
            appendChaild(pointRoot, pointModels);
        }
        private static void makeRoot()
        {
            Context con = new Context("Root", "Root all context this server", Util.TypeContext.Root);
            appendContext(con);
        }

        public static void appendContext(Context context)
        {
            listContext.Add(context);
        }
        public static Context getContext(int index)
        {
            if (index < 0 || index >= listContext.Count) return null;
            return listContext[index];
        }
        public static void setContext(int index,Context context)
        {
            if (index < 0 || index >= listContext.Count) return;
            listContext[index] = context;
        }
        public static void appendChaild (int contextParent,int context)
        {
            Context con = getContext(contextParent);
            if (con == null) return;
            con.appendChaild(context);
            setContext(contextParent, con);
        }
        public static void appendChaild(int contextParent)
        {
            Context con = getContext(contextParent);
            int context = listContext.Count - 1;
            con.appendChaild(context);
            setContext(contextParent, con);
        }
        public static void InitDevices()
        {
            Context con = getContext(PointDevices);
            for (int i = 0; i < con.Count; i++)
            {
                Context condev = getContext(con.getChaild(i));
                Device dev = (Device)condev.defContext;
                dev.InitDevice();
            }
        }

        public static int PointRoot { get { return pointRoot; } }
        public static int PointDevices { get { return pointDevices; } }
        public static int PointVariables { get { return pointVariables; } }
        public static int PointModels { get { return pointModels; } }
        public static int PointBlinds { get { return pointBlinds; } }
        public static int PointConstants { get { return pointConstants; } }
        public static int Count { get { return listContext.Count; } }

    }
}
