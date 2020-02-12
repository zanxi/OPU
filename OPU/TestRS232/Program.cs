  
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Data;
using Modbus.Device;

namespace TestRS232
{
    class Program
    {
        static void Main(string[] args)
        {
            ushort[] r1 = formula.outFloat(1.2344f, true);
            r1= formula.outIntefer(12345,5, true);
            r1 = formula.outIntefer(12, 2, true);
            r1 = formula.outIntefer(5, 1, true);
            r1 = formula.outTimer( true);
 
        }
    }
    static class formula
    {
        static public ushort[] outFloat(float infloat, bool blink)
        {
            ushort[] rez;
            byte[] rbyte;
            string sres;
            sres = infloat.ToString("####0");
            while (true)
            {
                if (infloat < 10.00f) { sres = infloat.ToString("0.0000"); break; }
                if (infloat < 100.0f) { sres = infloat.ToString("#0.000"); break; }
                if (infloat < 1000.0f) { sres = infloat.ToString("##0.00"); break; }
                if (infloat < 10000.0f) { sres = infloat.ToString("###0.0"); break; }
                break;
            }
            rbyte = convertToDisplay(sres);
            rez = upak(rbyte,blink);
            return rez;
        }
        static public ushort[] outIntefer(int inInt,int len, bool blink)
        {
            ushort[] rez;
            byte[] rbyte;
            string sres="0";
            if (len==5)sres = inInt.ToString("####0");
            if(len==2) sres = inInt.ToString("00");
            if (len == 1) sres = inInt.ToString("0");
            rbyte = convertToDisplay(sres);
            rez = upak(rbyte, blink);
            return rez;
        }
        static public ushort[] outTimer( bool blink)
        {
            ushort[] rez;
            byte[] rbyte;
            DateTime time = DateTime.Now;
            string sres = String.Format("{0:00}:{1:00}:{2:00}", time.Hour, time.Minute, time.Second);
            rbyte = convertToDisplay(sres);
            rez = upak(rbyte, blink);
            return rez;
        }


        static private ushort[] upak(byte[] rbyte,bool blink)
        {
            int len = (rbyte.Length % 2 == 0) ? rbyte.Length / 2 : (rbyte.Length / 2) + 1;
            ushort[] rez=new ushort[++len];
            rez[0] = (ushort) (blink ? 0 : 1);
            int j = 1;
            for (int i = 0; i < rbyte.Length; i++)
            {
                   rez[j] = (ushort) ((i % 2) == 0?(rez[j] | (rbyte[i] << 8)): (rez[j] | (rbyte[i])));
                j += (i % 2) == 0 ? 0 : 1;
            }
            return rez;
        }

        static private byte[] convertToDisplay(string sres)
        {
            bool point = (sres.IndexOf(".")>0)||(sres.IndexOf(",") > 0);

            byte[] rez = new byte[sres.Length - (point ? 1 : 0)];
            int j = 0;
            for (int i = 0; i < sres.Length; i++)
            {
                byte b =0xf;
                switch (sres[i])
                {
                    case '0': b = 0;break;
                    case '1': b = 1; break;
                    case '2': b = 2; break;
                    case '3': b = 3; break;
                    case '4': b = 4; break;
                    case '5': b = 5; break;
                    case '6': b = 6; break;
                    case '7': b = 7; break;
                    case '8': b = 8; break;
                    case '9': b = 9; break;
                    case ':': b = 0x2f;break;
                    case ' ': b = 0x7f; break;
                    case ',': b = 0xff; break;
                    case '.': b = 0xff; break;
                }
                if (b != 0xff)
                {
                    rez[j++] = b;
                }
                else rez[j-1] =(byte) (rez[j-1] | 0x80);
            }
            return rez;
        }
    }
    class Test
    {
        void p()
        {
            using (SerialPort port = new SerialPort("COM1"))
            {
                port.BaudRate = 19200;
                port.DataBits = 8;
                port.Parity = Parity.Odd;
                port.StopBits = StopBits.One;
                port.ReadTimeout = 1000;
                port.WriteTimeout = 1000;

                port.Open();
                IModbusSerialMaster master = ModbusSerialMaster.CreateRtu(port);

                byte slaveId = 1;
                ushort startAddress = 0x180;
                ushort[] res = master.ReadHoldingRegisters(slaveId, startAddress, 1);
                for (int i = 0; i < res.Length; i++)
                {
                    Console.Write(res[i].ToString() + " ");
                    if ((i + 1) % 4 == 0) Console.WriteLine();
                    Console.ReadKey();
                }
            }
        }

    }

}

