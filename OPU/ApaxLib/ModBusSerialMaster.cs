using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;
using System.Collections;
using System.Diagnostics;

// В данной библиотеке создается класс с его свойствами и методами, определяющие операции с устройством ModBusSerialMaster: 
// 1. создается дочерний класс ModBusSerialMaster на основе шаблонного родительского DriverModBus



namespace ApaxLib
{
    public class ModBusSerialMaster : DriverModBus
    {
        

        SerialPort port = null;
        const int maxUid= 255; 
        int[,] old = new int[maxUid,16];
        private int[] count = new int[maxUid];
        private bool[] flag = new bool[maxUid];
        const int bufsize = 1024;
        byte[] writeBuffer = new byte[bufsize];

        public ModBusSerialMaster(int device, ModBusDriverParam param) : base(device, param)
        {
            this.device.master = true;
            this.device.tcp = false;
            for (int i = 0; i < flag.Length; i++)
            {
                flag[i] = false;
                for (int j = 0; j < 16; j++) { old[i, j] = 0; }
            }                        
        }

        override public void StartDriver() // переопределяемая функция запуска драйвера
        {
            try
            {

                if (port != null)
                {
                    port.Close();
                    port = null;
                }/**/
                port = new SerialPort(param.portname);
                port.BaudRate = param.baudRate; // скорость передачи
                port.DataBits = param.databits;
                port.Parity = param.parity;
                port.StopBits = param.stopbits;
                port.ReadTimeout = device.timeout;
                port.WriteTimeout = device.timeout;
                port.Open(); 
 
                drvthr = new Thread(this.Run);
                drvthr.Start();

                device.status = 0;
                device.ready = true;
            }
            catch (Exception err)
            {
                Util.errorFD();
                Util.errorMessage("Start ModbuserialMaster "+err.Message,"");
                device.status = err.HResult;
                device.ready = false;
            }
        }
        bool compare(int uId, ushort[] b)
        {

            for (int i = 0; i < b.Length; i++)
            {
                if (old[uId,i] != b[i])
                {
                    flag[uId] = false;
                    count[uId] = 0;
                    return false;
                }
            }
            if (flag[uId]) return true;
            count[uId] += 1;
            if (count[uId] > 10) { count[uId] = 0; flag[uId] = true; return false; }
            return true;
        }
        void modify(int uId, ushort[] b)
        {
            for (int i = 0; i < b.Length; i++)
            {
                old[uId,i] = b[i];
            }
        }
        void cleanQuery()
        {
            // вначале разбираем очередь на запись
            writeComm wr;

            // создание дубликата writeQuveryDuble и освобождение исходной коллеции
            List<writeComm> writeQuveryDuble = new List<writeComm>();
            lock (block)
            {
                for (int i = 0; i < writeQuvery.Count; i++)
                {
                    writeQuveryDuble.Add(writeQuvery[i]);
                }
                writeQuvery.Clear();
            } //lock

            try
            {
                bool[] pres = new bool[maxUid];
                for (int i = 0; i < pres.Length; i++)
                {
                    pres[i] = false;
                }

                for (int i = 0; i < writeQuveryDuble.Count; i++)
                {
                    wr = writeQuveryDuble[i];
                    ushort[] b = (ushort[])wr.value;
                    if (pres[wr.unitId]) continue;
                    if (!compare(wr.unitId, b))
                    {
 
                        WriteComModbus(wr);
                        modify(wr.unitId, b);
                        pres[wr.unitId] = true;
                    }
                }
            }
            catch (Exception err)
            {
                Util.errorFD();
                Util.errorMessage("ModbuserialMaster ", err.Message);
                device.status = err.HResult;
                device.ready = false;
            }
        }
        public void WriteComModbus(writeComm wr )
        {

            try
            {
                ushort[] b = (ushort[])wr.value;
                writeBuffer[0] = (byte)(wr.unitId & 0xff);
                writeBuffer[1] = 0x10;
                writeBuffer[2] = (byte) ((wr.address>>8)&0xff);
                writeBuffer[3] = (byte)(wr.address & 0xff);
                int countbyte = formula.ushort2byte(b, writeBuffer, 7);
                int countregs = countbyte / 2;
                writeBuffer[4] = (byte)((countregs >> 8) & 0xff);
                writeBuffer[5] = (byte)(countregs & 0xff);
                writeBuffer[6] = (byte)(countbyte & 0xff);
                int[] crc = formula.calculateCRC(writeBuffer, 0, countbyte + 7);
                writeBuffer[countbyte + 7] = (byte)(crc[0] & 0xff);
                writeBuffer[countbyte + 8] = (byte)(crc[1] & 0xff);

                port.DiscardInBuffer();
                port.Write(writeBuffer, 0, countbyte + 9);
                Thread.Sleep(25);

            }
            catch (Exception err)
            {
                Util.errorFD();
                Util.errorMessage("ModbuserialMaster ", err.Message);
                device.status = err.HResult;
                device.ready = false;
            }

        }
    override public void Run() // переопределяемая функция запуска програмного потока для обслуживания работающего драйвера ModbusSerialMaster
        {
            //Util.errorMessage("Start ModbuserialMaster","");
            do
            {
                DateTime tm = DateTime.Now;  // переменная хранит начальное время запуска потока
                cleanQuery();

                DateTime tmnow = DateTime.Now; // переменная хранит время завершения операции в потоке
                int st = (int)((tmnow.Ticks - tm.Ticks) / 10000); // если превышено время обхода в 100 миллисекунд генерируется предупреждение с указанием колличества циклов                 

                if (st > device.steptime) {

                    Util.errorMessage("Время обработки цикла " +
                       st.ToString() +
                       " превысило заданный период  " +
                       device.steptime + "  для устройства ",
                       description);

                    st = 0;                                        
                }
                else st = device.steptime - st;
                if (st > 0) Thread.Sleep(st);

            } while (true);
        }

        override public void stopDriver() // переопределяемая функция остановки драйвера
        {
            if (device.ready)
            {
                
                drvthr.Abort();
                drvthr.Join();
            }
            if (port != null)
            {
                port.Dispose();
                port.Close();
                port = null;
            }
            device.status = 0;
            device.ready = false;
        }
    }
}