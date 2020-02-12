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


// В данной библиотеке создается класс с его свойствами и методами, определяющие операции с устройством ModBusTCPMaster: 
// 1. создается дочерний класс ModBusTCPMaster на основе шаблонного родительского DriverModBus


namespace ApaxLib
{
    public class ModBusTCPMaster : DriverModBus
    {


        TcpClient tcpClient = null;
        ModbusIpMaster master = null;
        public ModBusTCPMaster(int device, ModBusDriverParam param) : base(device, param)
        {
            this.device.master = true;
            this.device.tcp = true;
        }
        override public void StartDriver()  // переопределяемая функция запуска драйвера
        {
            try
            {
                //tcpClient = new TcpClient(param.ip, param.port);
                tcpClient = new TcpClient();
                tcpClient.Connect(param.ip, param.port);
                master = ModbusIpMaster.CreateIp(tcpClient);
                master.Transport.Retries = 2;
                master.Transport.WriteTimeout = device.timeout;
                master.Transport.ReadTimeout = device.timeout;



                drvthr = new Thread(this.Run);
                drvthr.Start();


                device.status = 0;
                device.ready = true;

            }
            catch (Exception err)
            {
                Util.errorTCP();
                Util.errorMessage(err.Message, description);
                device.status = err.HResult;
                device.ready = false;
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
                for (int i = 0; i < writeQuveryDuble.Count; i++)
                {
                    wr = writeQuveryDuble[i];
                    if (wr.reg == 0)
                    {
                        if (wr.len == 1) master.WriteSingleCoil(wr.address, (bool)wr.value);
                        else
                        {
                            bool[] b = (bool[])wr.value;
                            master.WriteMultipleCoils(wr.address, b);
                        }
                    }
                    if (wr.reg == 3)
                    {
                        if (wr.type == 2 || wr.type == 3) //2 байтный int
                        {
                            if (wr.len == 1) master.WriteSingleRegister(wr.address, BitConverter.ToUInt16(BitConverter.GetBytes((int)wr.value), 0));
                            else
                            {
                                ushort[] b = (ushort[])wr.value;
                                master.WriteMultipleRegisters(wr.address, b);
                            }
                        }
                        if (wr.type >= 4 && wr.type <= 7) //4 байтный int
                        {
                            if (wr.len == 1)
                            {
                                ushort[] b = new ushort[2];
                                b[0] = BitConverter.ToUInt16(BitConverter.GetBytes((int)wr.value), 0);
                                b[1] = BitConverter.ToUInt16(BitConverter.GetBytes((int)wr.value), 2);
                                master.WriteMultipleRegisters(wr.address, b);
                            }
                            else
                            {
                                int[] b = (int[])wr.value;
                                for (ushort j = 0; j < wr.len; j++)
                                {
                                    ushort[] bb = new ushort[2];
                                    bb[0] = BitConverter.ToUInt16(BitConverter.GetBytes(b[j]), 0);
                                    bb[1] = BitConverter.ToUInt16(BitConverter.GetBytes(b[j]), 2);
                                    master.WriteMultipleRegisters((ushort)(wr.address + (j * 2)), bb);
                                }
                            }
                        }
                        if (wr.type >= 8 && wr.type <= 9) //4 байтный float
                        {
                            if (wr.len == 1)
                            {
                                byte[] b = BitConverter.GetBytes((float)wr.value);
                                ushort[] bb = new ushort[2];
                                bb[0] = BitConverter.ToUInt16(b, 0);
                                bb[1] = BitConverter.ToUInt16(b, 2);
                                master.WriteMultipleRegisters(wr.address, bb);

                            }
                            else
                            {
                                float[] bb = (float[])wr.value;
                                for (int j = 0; j < wr.len; j++)
                                {
                                    byte[] b = BitConverter.GetBytes(bb[j]);
                                    ushort[] bbb = new ushort[2];
                                    bbb[0] = BitConverter.ToUInt16(b, 0);
                                    bbb[1] = BitConverter.ToUInt16(b, 2);
                                    master.WriteMultipleRegisters((ushort)(wr.address + (j * 2)), bbb);
                                }
                            }
                        }//if
                    }//if
                }//for

            }
            catch (Exception err)
            {
                Util.errorTCP();
                Util.errorMessage(err.Message, description);
                device.status = err.HResult;
                device.ready = false; //??????????????????????????????/
            }
            //writeQuvery.Clear();
        }
        void writeVariable()
        {


            for (int i = 0; i < registers.Count; i++)
            {
                regModBus rg = registers[i];
                if (rg.regtype == 0)
                {
                    if (rg.len == 1)
                    {
                        bool b = varcoils[rg.address];
                        Variable.setVariable(rg.varcontext, b, true);
                    }
                    else
                    {
                        bool[] b = new bool[rg.len];
                        for (int j = 0; j < rg.len; j++) b[j] = varcoils[rg.address + j];
                        Variable.setVariable(rg.varcontext, b, true);
                    }

                }// end type ==0
                if (rg.regtype == 1)
                {
                    if (rg.len == 1)
                    {
                        bool b = vardi[rg.address];
                        Variable.setVariable(rg.varcontext, b, true);
                    }
                    else
                    {
                        bool[] b = new bool[rg.len];
                        for (int j = 0; j < rg.len; j++) b[j] = vardi[rg.address + j];
                        Variable.setVariable(rg.varcontext, b, true);
                    }

                }// end type ==1
                if (rg.regtype == 2)
                {
                    if (rg.type == 2 || rg.type == 3) //2 байтный int
                    {
                        if (rg.len == 1)
                        {
                            ushort b;
                            b = varir[rg.address];
                            Variable.setVariable(rg.varcontext, b, true);
                        }
                        else
                        {
                            ushort[] b = new ushort[rg.len];
                            for (int j = 0; j < rg.len; j++) b[j] = varir[rg.address + j];
                            Variable.setVariable(rg.varcontext, b, true);
                        }
                    }
                    if (rg.type >= 4 && rg.type <= 7) //4 байтный int
                    {
                        if (rg.len == 1)
                        {
                            byte[] b1 = BitConverter.GetBytes(varir[rg.address]);
                            byte[] b2 = BitConverter.GetBytes(varir[rg.address + 1]);
                            byte[] bb = new byte[4];
                            bb[0] = b2[0];
                            bb[1] = b2[1];
                            bb[2] = b1[0];
                            bb[3] = b1[1];
                            int b = BitConverter.ToInt32(bb, 0);
                            Variable.setVariable(rg.varcontext, b, true);
                        }
                        else
                        {
                            int[] b = new int[rg.len];
                            for (int j = 0; j < rg.len; j++)
                            {
                                byte[] b1 = BitConverter.GetBytes(varir[rg.address + (j * 2)]);
                                byte[] b2 = BitConverter.GetBytes(varir[rg.address + (j * 2) + 1]);
                                byte[] bb = new byte[4];
                                bb[0] = b2[0];
                                bb[1] = b2[1];
                                bb[2] = b1[0];
                                bb[3] = b1[1];
                                b[j] = BitConverter.ToInt32(bb, 0);
                            }
                            Variable.setVariable(rg.varcontext, b, true);
                        }
                    }
                    if (rg.type >= 8 && rg.type <= 9) //4 байтный float
                    {
                        if (rg.len == 1)
                        {
                            byte[] b1 = BitConverter.GetBytes(varir[rg.address]);
                            byte[] b2 = BitConverter.GetBytes(varir[rg.address + 1]);
                            byte[] bb = new byte[4];
                            bb[0] = b2[0];
                            bb[1] = b2[1];
                            bb[2] = b1[0];
                            bb[3] = b1[1];
                            float b = BitConverter.ToSingle(bb, 0);
                            Variable.setVariable(rg.varcontext, b, true);
                        }
                        else
                        {
                            float[] b = new float[rg.len];
                            for (int j = 0; j < rg.len; j++)
                            {
                                byte[] b1 = BitConverter.GetBytes(varir[rg.address + (j * 2)]);
                                byte[] b2 = BitConverter.GetBytes(varir[rg.address + (j * 2) + 1]);
                                byte[] bb = new byte[4];
                                bb[0] = b2[0];
                                bb[1] = b2[1];
                                bb[2] = b1[0];
                                bb[3] = b1[1];
                                b[j] = BitConverter.ToSingle(bb, 0);
                            }
                            Variable.setVariable(rg.varcontext, b, true);
                        }
                    }
                }// end type ==2

                if (rg.regtype == 3)
                {
                    if (rg.type == 2 || rg.type == 3) //2 байтный int
                    {
                        if (rg.len == 1)
                        {
                            ushort b;
                            b = varhr[rg.address];
                            Variable.setVariable(rg.varcontext, b, true);
                        }
                        else
                        {
                            ushort[] b = new ushort[rg.len];
                            for (int j = 0; j < rg.len; j++) b[j] = varhr[rg.address + j];
                            Variable.setVariable(rg.varcontext, b, true);
                        }
                    }
                    if (rg.type >= 4 && rg.type <= 7) //4 байтный int
                    {
                        if (rg.len == 1)
                        {
                            byte[] b1 = BitConverter.GetBytes(varhr[rg.address]);
                            byte[] b2 = BitConverter.GetBytes(varhr[rg.address + 1]);
                            byte[] bb = new byte[4];
                            bb[0] = b2[0];
                            bb[1] = b2[1];
                            bb[2] = b1[0];
                            bb[3] = b1[1];
                            int b = BitConverter.ToInt32(bb, 0);
                            Variable.setVariable(rg.varcontext, b, true);
                        }
                        else
                        {
                            int[] b = new int[rg.len];
                            for (int j = 0; j < rg.len; j++)
                            {
                                byte[] b1 = BitConverter.GetBytes(varhr[rg.address + (j * 2)]);
                                byte[] b2 = BitConverter.GetBytes(varhr[rg.address + (j * 2) + 1]);
                                byte[] bb = new byte[4];
                                bb[0] = b2[0];
                                bb[1] = b2[1];
                                bb[2] = b1[0];
                                bb[3] = b1[1];
                                b[j] = BitConverter.ToInt32(bb, 0);
                            }
                            Variable.setVariable(rg.varcontext, b, true);
                        }
                    }
                    if (rg.type >= 8 && rg.type <= 9) //4 байтный float
                    {
                        if (rg.len == 1)
                        {
                            byte[] b1 = BitConverter.GetBytes(varhr[rg.address]);
                            byte[] b2 = BitConverter.GetBytes(varhr[rg.address + 1]);
                            byte[] bb = new byte[4];
                            bb[0] = b2[0];
                            bb[1] = b2[1];
                            bb[2] = b1[0];
                            bb[3] = b1[1];
                            float b = BitConverter.ToSingle(bb, 0);
                            Variable.setVariable(rg.varcontext, b, true);
                        }
                        else
                        {
                            float[] b = new float[rg.len];
                            for (int j = 0; j < rg.len; j++)
                            {
                                byte[] b1 = BitConverter.GetBytes(varhr[rg.address + (j * 2)]);
                                byte[] b2 = BitConverter.GetBytes(varhr[rg.address + (j * 2) + 1]);
                                byte[] bb = new byte[4];
                                bb[0] = b2[0];
                                bb[1] = b2[1];
                                bb[2] = b1[0];
                                bb[3] = b1[1];
                                b[j] = BitConverter.ToSingle(bb, 0);
                            }
                            Variable.setVariable(rg.varcontext, b, true);
                        }
                    }
                }// end type ==3

            }//for
        }
        override public void Run() // переопределяемая функция запуска програмного потока для обслуживания работающего драйвера ModbusTCPMaster
        {
            do
            {

                // счетчик таймера операций ModbusTCPMaster

                DateTime tm = DateTime.Now;  // переменная хранит начальное время запуска потока
                Util.time = tm;
                if (!device.ready)
                {
                    master.Dispose();
                    tcpClient.Close();
                    device.status = 200;
                    device.ready = false;
                    break;
                }
                cleanQuery();
                try
                {
                    if (lencoils > 0) varcoils = master.ReadCoils(0, lencoils);
                    if (lendi > 0) vardi = master.ReadInputs(0, lendi);
                    if (lenir > 0)
                    {
                        ushort stadr = 0;
                        int len = lenir;
                        do
                        {
                            ushort lenl = (len > 100) ? (ushort)100 : (ushort)len;
                            ir = master.ReadInputRegisters(stadr, lenl);
                            for (int i = 0; i < lenl; i++) varir[stadr + i] = ir[i];
                            stadr += 100;
                            len -= 100;
                        } while (len > 0);
                    }
                    if (lenhr > 0)
                    {
                        ushort stadr = 0;
                        int len = lenhr;
                        do
                        {
                            ushort lenl = (len > 100) ? (ushort)100 : (ushort)len;
                            hr = master.ReadHoldingRegisters(stadr, lenl);
                            //hr = master.ReadHoldingRegisters(stadr, 1000);
                            for (int i = 0; i < lenl; i++) varhr[stadr + i] = hr[i];
                            stadr += 100;
                            len -= 100;
                        } while (len > 0);
                    }
                }
                catch (Exception err)
                {
                    Util.errorTCP();
                    Util.errorMessage(err.Message, description);
                    device.status = err.HResult;
                    master.Dispose();
                    tcpClient.Close();
                    device.status = 200;
                    device.ready = false;
                    break;
                }
                writeVariable();

                DateTime tmnow = DateTime.Now; // переменная хранит время завершения операции в потоке
                int st = (int)((tmnow.Ticks - tm.Ticks) / 10000); // если превышено время обхода в 100 миллисекунд генерируется предупреждение с указанием колличества циклов
                                                                  // Util.errorMessage("Время потраченное на операции в ModbusTCPMaster : ", st.ToString());
                                                                  //if (st > device.steptime) { Util.errorMessage("Очень долго...." + st.ToString() + " | step time system=" + device.steptime, "; << ModbusTCPMaster >>"); st = 0; }
                if (st > device.steptime)
                {
                    Util.errorMessage("Время обработки цикла " +
                    st.ToString() +
                    " превысило заданный период  " +
                    device.steptime + "  для устройства ",
                    description);

                    st = 0;
                }



                else st = device.steptime - st;
                if (st > 0) Thread.Sleep(st);

                //Thread.Sleep(device.steptime);

            } while (true);
        }
        override public void stopDriver() // переопределяемая функция остановки драйвера
        {
            if (drvthr != null)
            {
                drvthr.Abort();
                drvthr.Join();
            }
            drvthr = null;
            if (master != null)
            {
                master.Dispose();
                tcpClient.Close();
            }
            device.status = 200;
            device.ready = false;
        }

    }
}


