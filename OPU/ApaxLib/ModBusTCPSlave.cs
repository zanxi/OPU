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

// В данной библиотеке создается класс с его свойствами и методами, определяющие операции с устройством ModBusTCPSlave: 
// 1. создается дочерний класс ModBusTCPSlave на основе шаблонного родительского DriverModBus


namespace ApaxLib
{
    public class ModBusTCPSlave : DriverModBus
    {

        

        TcpListener slaveTcpListener = null;
        ModbusSlave slave = null;
        public ModBusTCPSlave(int device, ModBusDriverParam param) : base(device, param)
        {
            this.device.master = false;
            this.device.tcp = true;
        }

        override public void StartDriver() // переопределяемая функция запуска драйвера
        {
            try
            {
                if (slaveTcpListener == null)
                {
                    slaveTcpListener = new TcpListener(param.port);
                    slaveTcpListener.Start();
                }
                if (slave == null)
                {
                    slave = ModbusTcpSlave.CreateTcp(0, slaveTcpListener);
                    slave.DataStore = DataStoreFactory.CreateDefaultDataStore(lencoils, lendi, lenhr, lenir);
                    slave.Listen();
                }

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



            // вначале разбираем очередь на запись


            for (int i = 0; i < writeQuveryDuble.Count; i++)
                {
                    writeComm wr = writeQuveryDuble[i];
                    wr.address++;
                    if (wr.reg == 0)
                    {
                        if (wr.len == 1) slave.DataStore.CoilDiscretes[wr.address] = (bool)wr.value;
                        else
                        {
                            bool[] b = (bool[])wr.value;
                            for (int j = 0; j < wr.len; j++) slave.DataStore.CoilDiscretes[wr.address + j] = b[j];
                        }
                    }
                    if (wr.reg == 1)
                    {
                        if (wr.len == 1) slave.DataStore.InputDiscretes[wr.address] = (bool)wr.value;
                        else
                        {
                            bool[] b = (bool[])wr.value;
                            for (int j = 0; j < wr.len; j++) slave.DataStore.InputDiscretes[wr.address + j] = b[j];
                        }
                    }
                    if (wr.reg == 2)
                    {
                        if (wr.type == 2 || wr.type == 3) //2 байтный int
                        {
                            if (wr.len == 1) slave.DataStore.InputRegisters[wr.address] = (ushort)wr.value;
                            else
                            {
                                ushort[] b = (ushort[])wr.value;
                                for (int j = 0; j < wr.len; j++) slave.DataStore.InputRegisters[wr.address + j] = b[j];
                            }
                        }
                        if (wr.type >= 4 && wr.type <= 7) //4 байтный int
                        {
                            if (wr.len == 1)
                            {
                                ushort lowOrderValue = BitConverter.ToUInt16(BitConverter.GetBytes((int)wr.value), 0);
                                ushort highOrderValue = BitConverter.ToUInt16(BitConverter.GetBytes((int)wr.value), 2);
                                slave.DataStore.InputRegisters[wr.address] = lowOrderValue;
                                slave.DataStore.InputRegisters[wr.address + 1] = highOrderValue;
                            }
                            else
                            {
                                int[] b = (int[])wr.value;
                                for (int j = 0; j < wr.len; j++)
                                {
                                    ushort lowOrderValue = BitConverter.ToUInt16(BitConverter.GetBytes(b[j]), 0);
                                    ushort highOrderValue = BitConverter.ToUInt16(BitConverter.GetBytes(b[j]), 2);
                                    slave.DataStore.InputRegisters[wr.address + (j * 2)] = lowOrderValue;
                                    slave.DataStore.InputRegisters[wr.address + (j * 2) + 1] = highOrderValue;
                                }
                            }
                        }
                        if (wr.type >= 8 && wr.type <= 9) //4 байтный float
                        {
                            if (wr.len == 1)
                            {
                                byte[] b = BitConverter.GetBytes((float)wr.value);
                                ushort lowOrderValue = BitConverter.ToUInt16(b, 0);
                                ushort highOrderValue = BitConverter.ToUInt16(b, 2);
                                slave.DataStore.InputRegisters[wr.address] = lowOrderValue;
                                slave.DataStore.InputRegisters[wr.address + 1] = highOrderValue;
                            }
                            else
                            {
                                float[] bb = (float[])wr.value;
                                for (int j = 0; j < wr.len; j++)
                                {
                                    byte[] b = BitConverter.GetBytes(bb[j]);
                                    ushort lowOrderValue = BitConverter.ToUInt16(b, 0);
                                    ushort highOrderValue = BitConverter.ToUInt16(b, 2);
                                    slave.DataStore.InputRegisters[wr.address + (j * 2)] = lowOrderValue;
                                    slave.DataStore.InputRegisters[wr.address + (j * 2) + 1] = highOrderValue;
                                }
                            }
                        }
                    }
                    if (wr.reg == 3)
                    {
                        if (wr.type == 2 || wr.type == 3) //2 байтный int
                        {
                            if (wr.len == 1) slave.DataStore.HoldingRegisters[wr.address] = (ushort)wr.value;
                            else
                            {
                                ushort[] b = (ushort[])wr.value;
                                for (int j = 0; j < wr.len; j++) slave.DataStore.HoldingRegisters[wr.address + j] = b[j];
                            }
                        }
                        if (wr.type >= 4 && wr.type <= 7) //4 байтный int
                        {
                            if (wr.len == 1)
                            {
                                ushort lowOrderValue = BitConverter.ToUInt16(BitConverter.GetBytes((int)wr.value), 0);
                                ushort highOrderValue = BitConverter.ToUInt16(BitConverter.GetBytes((int)wr.value), 2);
                                slave.DataStore.HoldingRegisters[wr.address] = lowOrderValue;
                                slave.DataStore.HoldingRegisters[wr.address + 1] = highOrderValue;
                            }
                            else
                            {
                                int[] b = (int[])wr.value;
                                for (int j = 0; j < wr.len; j++)
                                {
                                    ushort lowOrderValue = BitConverter.ToUInt16(BitConverter.GetBytes(b[j]), 0);
                                    ushort highOrderValue = BitConverter.ToUInt16(BitConverter.GetBytes(b[j]), 2);
                                    slave.DataStore.HoldingRegisters[wr.address + (j * 2)] = lowOrderValue;
                                    slave.DataStore.HoldingRegisters[wr.address + (j * 2) + 1] = highOrderValue;
                                }
                            }
                        }
                        if (wr.type >= 8 && wr.type <= 9) //4 байтный float
                        {
                            if (wr.len == 1)
                            {
                                byte[] b = BitConverter.GetBytes((float)wr.value);
                                ushort lowOrderValue = BitConverter.ToUInt16(b, 0);
                                ushort highOrderValue = BitConverter.ToUInt16(b, 2);
                                slave.DataStore.HoldingRegisters[wr.address] = lowOrderValue;
                                slave.DataStore.HoldingRegisters[wr.address + 1] = highOrderValue;
                            }
                            else
                            {
                                float[] bb = (float[])wr.value;
                                for (int j = 0; j < wr.len; j++)
                                {
                                    byte[] b = BitConverter.GetBytes(bb[j]);
                                    ushort lowOrderValue = BitConverter.ToUInt16(b, 0);
                                    ushort highOrderValue = BitConverter.ToUInt16(b, 2);
                                    slave.DataStore.HoldingRegisters[wr.address + (j * 2)] = lowOrderValue;
                                    slave.DataStore.HoldingRegisters[wr.address + (j * 2) + 1] = highOrderValue;
                                }
                            }
                        }//if
                    }//if
                }//for
                //writeQuvery.Clear();
        }
        void writeVariable()
        {

                for (int i = 0; i < registers.Count; i++)
                {
                    regModBus rg = registers[i];
                    rg.address++;
                    if (rg.regtype == 0)
                    {
                        if (rg.len == 1)
                        {
                            bool b = slave.DataStore.CoilDiscretes[rg.address];
                            Variable.setVariable(rg.varcontext, b, true);
                        }
                        else
                        {
                            bool[] b = new bool[rg.len];
                            for (int j = 0; j < rg.len; j++) b[j] = slave.DataStore.CoilDiscretes[rg.address + j];
                            Variable.setVariable(rg.varcontext, b, true);
                        }

                    }// end type ==0

                    if (rg.regtype == 3)
                    {
                        if (rg.type == 2 || rg.type == 3) //2 байтный int
                        {
                            if (rg.len == 1)
                            {
                                ushort b;
                                b = slave.DataStore.HoldingRegisters[rg.address];
                                Variable.setVariable(rg.varcontext, b, true);
                            }
                            else
                            {
                                ushort[] b = new ushort[rg.len];
                                for (int j = 0; j < rg.len; j++) b[j] = slave.DataStore.HoldingRegisters[rg.address + j];
                                Variable.setVariable(rg.varcontext, b, true);
                            }
                        }
                        if (rg.type >= 4 && rg.type <= 7) //4 байтный int
                        {
                            if (rg.len == 1)
                            {
                                byte[] b1 = BitConverter.GetBytes(slave.DataStore.HoldingRegisters[rg.address]);
                                byte[] b2 = BitConverter.GetBytes(slave.DataStore.HoldingRegisters[rg.address + 1]);
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
                                    byte[] b1 = BitConverter.GetBytes(slave.DataStore.HoldingRegisters[rg.address + (j * 2)]);
                                    byte[] b2 = BitConverter.GetBytes(slave.DataStore.HoldingRegisters[rg.address + (j * 2) + 1]);
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
                                byte[] b1 = BitConverter.GetBytes(slave.DataStore.HoldingRegisters[rg.address]);
                                byte[] b2 = BitConverter.GetBytes(slave.DataStore.HoldingRegisters[rg.address + 1]);
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
                                    byte[] b1 = BitConverter.GetBytes(slave.DataStore.HoldingRegisters[rg.address + (j * 2)]);
                                    byte[] b2 = BitConverter.GetBytes(slave.DataStore.HoldingRegisters[rg.address + (j * 2) + 1]);
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
        override public void Run()
        {
            do
            {
                // счетчик таймера операций ModbusTCPSlave

                DateTime tm = DateTime.Now;  // переменная хранит начальное время запуска потока
                Util.time = tm;

                if (!device.ready) break;

                cleanQuery();
                for (int i = 0; i < lencoils; i++) varcoils[i] = slave.DataStore.CoilDiscretes[i + 1];
                for (int i = 0; i < lendi; i++) vardi[i] = slave.DataStore.InputDiscretes[i + 1];
                for (int i = 0; i < lenir; i++) varir[i] = slave.DataStore.InputRegisters[i + 1];
                for (int i = 0; i < lenhr; i++) varhr[i] = slave.DataStore.HoldingRegisters[i + 1];
                writeVariable();


                DateTime tmnow = DateTime.Now; // переменная хранит время завершения операции в потоке
                int st = (int)((tmnow.Ticks - tm.Ticks) / 10000); // если превышено время обхода в 100 миллисекунд генерируется предупреждение с указанием колличества циклов
                                                                  // Util.errorMessage("Время потраченное на операции в ModbusTCPSlave : ", st.ToString());
                //if (st > device.steptime) { Util.errorMessage(" долго...." + st.ToString() + " | step time system=" + device.steptime, "; << ModbusTCPSlave >>"); st = 0; }
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

            drvthr.Abort();
            drvthr.Join();
            if (slave != null)
            {
                slave.Dispose();
                slaveTcpListener.Stop();
            }
            slave = null;
            slaveTcpListener = null;
            device.status = 0;
            device.ready = false;
        }

    }
}
