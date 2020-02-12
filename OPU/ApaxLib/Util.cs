using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


// В данной библиотеке: 
// 1. создаются средства мониторинга за системой - генерация системных ссообщений командами
// 2. Util.errorMessage(string,var);
// 3. Util.message(string)


namespace ApaxLib
{
    static public class Util
    {
        // public static bool debug = false;
        public static bool debug = false;

        public static bool work = true;
        public static bool show = false;
        public static bool blink = false;

        //public static FileStream descriptor_file_data_about_work_Apax = File.Open("C:\\Apax_work_data_2017_09_07_.txt",FileMode.Create,FileAccess.ReadWrite,FileShare.None);
        //public static StreamWriter writer_to_file_data_about_work_Apax = new StreamWriter(descriptor_file_data_about_work_Apax);

        //public static FileStream fileStream_data_about_work_Apax;
        //public static StreamWriter streamWriter_to_file_data_about_work_Apax;


        public static int live = 0;
        public static int status = 999;
        public enum TypeVar { Boolean, Integer, Float, Long, Double,String,Error };
        public enum TypeContext { Root, Devices, Variables, Models,Blinds,Constants}
        public enum TypeDriver { NotDriver,
                                 ModBusTCPMaster,
                                 ModBusTCPSlave,
                                 ModBusSerialMaster,ModBusSerialSlave,ApaxInput,ApaxOutput,RezCanal};
        public static List<string> messageLine, tcpMessage;
        
        public static DateTime time;

        internal static object DefaultValue(TypeVar type)
        {
            switch (type)
            {
                case TypeVar.Boolean: return false;
                case TypeVar.Integer: return 0;
                case TypeVar.Float: return 0.0f;
                case TypeVar.Long: return 0L;
                case TypeVar.Double: return 0d;
                case TypeVar.String: return "";
            }
            { Util.errorMessage("не смог установить значения по дефолту ", type.ToString()); }
            return null;
        }

        internal static void errorAPAX()
        {
            status = status | 1;
        }

        internal static void errorTCP()
        {
            status = status | 2;
        }

        internal static void errorFD()
        {
            status = status | 4;
        }

        internal static TypeVar StringToTypeVar(string value)
        {
            if (value.Equals("bool")) return TypeVar.Boolean;
            if (value.Equals("int")) return TypeVar.Integer;
            if (value.Equals("float")) return TypeVar.Float;
            if (value.Equals("long")) return TypeVar.Long;
            if (value.Equals("double")) return TypeVar.Double;
            if (value.Equals("string")) return TypeVar.String;
            Util.errorMessage("Ошибка: отсутствует параметр <<< value >>>.  ", "not type");
            return TypeVar.Error;
        }
        public static string TypeContextToString(TypeContext type)
        {
            switch (type)
            {
                case TypeContext.Root: return "Root";
                case TypeContext.Devices: return "Devices";
                case TypeContext.Variables:return "Variables";
                case TypeContext.Models: return "Models";
                case TypeContext.Blinds: return "Blinds";
                case TypeContext.Constants: return "Constants";
                
            }
            return "Error";
        }
        internal static object StringToValue(TypeVar type,string value)
        {
            //if (value == "0,0") value = "0";
            try
            {
                switch (type)
                {
                    case TypeVar.Boolean: return bool.Parse(value);
                    case TypeVar.Integer: return (int)(float.Parse(value));
                    case TypeVar.Float: return float.Parse(value);
                    case TypeVar.Long: return (int)(long.Parse(value));
                    case TypeVar.Double: return double.Parse(value);
                    case TypeVar.String: return value;
                }
            }
            catch (Exception)
            {   // не смог сконвертировать  --> 0,0
                Util.errorMessage("type:"+ type.ToString() + "; не смог сконвертировать ", value);
                return null;
            }
            return null;
        }

        internal static object DefaultValue(TypeVar type, int size)
        {
            if (size <= 1) return Util.DefaultValue(type);
            switch (type)
            {
                case TypeVar.Boolean: { bool[] b = new bool[size]; for (int i=0;i<b.Length;i++) b[i] = false; return b; }
                case TypeVar.Integer: { int[] b = new int[size]; for (int i = 0; i < b.Length; i++) b[i] = 0; return b; }
                case TypeVar.Float: { float[] b = new float[size]; for (int i = 0; i < b.Length; i++) b[i] = 0.0F; return b; }
                case TypeVar.Long: { long[] b = new long[size]; for (int i = 0; i < b.Length; i++) b[i] = 0L; return b; }
                case TypeVar.Double: { double[] b = new double[size]; for (int i = 0; i < b.Length; i++) b[i] = 0.0D; return b; }
                case TypeVar.String: return "";
            }
            Util.errorMessage("не смог установить значения по дефолту ", type.ToString()+" "+size.ToString());
            return null;
        }

        internal static int SetParent(TypeContext type)
        {
            switch (type)
            {
                case TypeContext.Blinds: return Server.PointBlinds;
                case TypeContext.Constants: return Server.PointConstants;
                case TypeContext.Devices: return Server.PointDevices;
                case TypeContext.Variables: return Server.PointVariables;
                case TypeContext.Models: return Server.PointModels;
                case TypeContext.Root: return Server.PointRoot;
            }
            return Server.PointRoot;
        }
        static public void errorMessage(string message1, string message2)
        {
            
            lock(messageLine) messageLine.Add( message1 +" --> "+message2+"\r\n");
            //if (Util.debug)
            {
                // Записываем в дескриптор строку сообщения
                //streamWriter_to_file_data_about_work_Apax.WriteLine(tm.GetDateTimeFormats()[12] + " Ошибка " + message1 + " --> " + message2 + "\n");
                //streamWriter_to_file_data_about_work_Apax.Flush();

            }
        }
        static public void message(string message)
        {
            if (!debug) return;
            DateTime tm =DateTime.Now;

            lock(messageLine)
            {
                messageLine.Add(tm.GetDateTimeFormats()[12] + " " + message + "\n");
                //streamWriter_to_file_data_about_work_Apax.WriteLine(tm.GetDateTimeFormats()[12] + " " + message + "\n");
                //streamWriter_to_file_data_about_work_Apax.Flush();
            }
        }


    }
}
