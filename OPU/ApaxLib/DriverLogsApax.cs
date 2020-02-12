using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Modbus.Device;
using System.Net.Sockets;
using System.Net;
using Modbus.Data;


namespace ApaxLib
{
    public class DriverLogsApax
    {


        TcpClient tcpClient = null;
        string rpuIP="192.168.1.30";        
        int rpuPort=505;

        Thread drvthrlogs;

        public void StartDriver()  // переопределяемая функция запуска драйвера
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(rpuIP, rpuPort);



                //drvthrlogs = new Thread(this.Run);
                //drvthrlogs.Start();
                
            }
            catch (Exception err)
            {
                Util.errorTCP();
                Util.errorMessage(err.Message, "loggings");
            
            }
        }
        void WriteLogs()
        {
            object block = new object();
            byte[] tx=new byte[1024];
            string mess = "!!!!!";
            

            try
            {
                lock (block)
                {
                    mess = "!!!!!";
                    tx = Encoding.ASCII.GetBytes(mess);// + tbPayload.Text);
                    tcpClient.GetStream().Write(tx,0,mess.Length);
                }                
            
              

            }
            catch (Exception err)
            {
                Util.errorTCP();
                Util.errorMessage(err.Message, "WriteLogs");              
            }
            //writeQuvery.Clear();
        }
        
        public void Run() // переопределяемая функция запуска програмного потока для обслуживания работающего драйвера ModbusTCPMaster
        {
            do
            {
                // счетчик таймера операций ModbusTCPMaster

                DateTime tm = DateTime.Now;  // переменная хранит начальное время запуска потока
                Util.time = tm;

                WriteLogs();        

                DateTime tmnow = DateTime.Now; // переменная хранит время завершения операции в потоке
                int st = (int)((tmnow.Ticks - tm.Ticks) / 10000); // если превышено время обхода в 100 миллисекунд генерируется предупреждение с указанием колличества циклов
                                                                  // Util.errorMessage("Время потраченное на операции в ModbusTCPMaster : ", st.ToString());                
                if (st > 0) Thread.Sleep(st);
                
            } while (true);
        }
        public void stopDriver() // переопределяемая функция остановки драйвера
        {
            drvthrlogs.Abort();
            drvthrlogs.Join();
             
            tcpClient.Close();
            
        }

    }
}


