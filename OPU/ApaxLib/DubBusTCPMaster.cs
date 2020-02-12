using Modbus.Device;
using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;



// В данной библиотеке: 
// 1. Создаются защитные вторичные копии устройств Модбаса
// 2. 


namespace ApaxLib
{
    public class DubBusTCPMaster : DriverModBus // второй экземпляр Модбаса
    {


        TcpClient[] tcpClient = new TcpClient[2];
        ModbusIpMaster[] master = new ModbusIpMaster[2];

        public DubBusTCPMaster(int device, ModBusDriverParam param) : base(device, param)
        {
            this.device.master = true;
            this.device.tcp = true;
            this.device.single = false;
            for (int i = 0; i < 2; i++)
            {
                tcpClient[i] = null;
                master[i] = null;
            }
        }

        override public void StartDriver() // переопределяемая функция родительского класса запуска драйвера
        {

            for (int i = 0; i < 2; i++)
            {
                if (tcpClient[i] == null || master[i] == null)
                {
                    try
                    {
                        //tcpClient[i] = new TcpClient(i == 0 ? param.ip : param.ipdub, i == 0 ? param.port : param.portdub);
                        tcpClient[i] = new TcpClient();
                        tcpClient[i].Connect(i == 0 ? param.ip : param.ipdub, i == 0 ? param.port : param.portdub);
                        //tcpClient[i].Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                        master[i] = ModbusIpMaster.CreateIp(tcpClient[i]);
                        master[i].Transport.Retries = 2;
                        master[i].Transport.WriteTimeout = device.timeout;
                        master[i].Transport.ReadTimeout = device.timeout;
                    }
                    catch (Exception err)
                    {
                        Util.errorMessage(err.Message, description);
                        tcpClient[i] = null;
                        master[i] = null;
                        Canal = (Canal + 1) & 1;
                    }
                }
            }
            if (drvthr==null)
            {
                drvthr = new Thread(this.Run);
                drvthr.Start();
                device.status = 0;
                device.ready = true;
            }
        }
        void cleanQuery()
        {
            // вначале разбираем очередь на запись
            // Исправить на внутреннюю очередь
            writeComm wr;

            // создание дубликата writeQuveryDuble и освобождение исходной коллеции
            List<writeComm> writeQuveryDuble = new List<writeComm>();
            lock (block)  // блокировка доступа - поочередное использование переменных, параметров коллеции каждым из потоков
            {             // создание дубликата коллекции и уничтожение самой коллекции 
                for (int i = 0; i < writeQuvery.Count; i++)
                {
                    writeQuveryDuble.Add(writeQuvery[i]);
                }
                writeQuvery.Clear();
            } //lock Исправлено Русинов 

            try
            {
                for (int i = 0; i < writeQuveryDuble.Count; i++)
                {
                    wr = writeQuveryDuble[i];
                    if (wr.reg == 0)
                    {
                        if (wr.len == 1) masterWriteSingleCoil(wr.address, (bool)wr.value);
                        else
                        {
                            bool[] b = (bool[])wr.value;
                            masterWriteMultipleCoils(wr.address, b);
                        }
                    }
                    if (wr.reg == 3)
                    {
                        if (wr.type == 2 || wr.type == 3) //2 байтный int
                        {
                            if (wr.len == 1) masterWriteSingleRegister(wr.address, BitConverter.ToUInt16(BitConverter.GetBytes((int)wr.value), 0));
                            else
                            {
                                ushort[] b = (ushort[])wr.value;
                                masterWriteMultipleRegisters(wr.address, b);
                            }
                        }
                        if (wr.type >= 4 && wr.type <= 7) //4 байтный int
                        {
                            if (wr.len == 1)
                            {
                                ushort[] b = new ushort[2];
                                b[0] = BitConverter.ToUInt16(BitConverter.GetBytes((int)wr.value), 2);
                                b[1] = BitConverter.ToUInt16(BitConverter.GetBytes((int)wr.value), 0);
                                masterWriteMultipleRegisters(wr.address, b);
                            }
                            else
                            {
                                int[] b = (int[])wr.value;
                                for (ushort j = 0; j < wr.len; j++)
                                {
                                    ushort[] bb = new ushort[2];
                                    bb[0] = BitConverter.ToUInt16(BitConverter.GetBytes(b[j]), 2);
                                    bb[1] = BitConverter.ToUInt16(BitConverter.GetBytes(b[j]), 0);
                                    masterWriteMultipleRegisters((ushort)(wr.address + (j * 2)), bb);
                                }
                            }
                        }
                        if (wr.type >= 8 && wr.type <= 9) //4 байтный float
                        {
                            if (wr.len == 1)
                            {
                                byte[] b = BitConverter.GetBytes((float)wr.value);
                                ushort[] bb = new ushort[2];
                                bb[0] = BitConverter.ToUInt16(b, 2);
                                bb[1] = BitConverter.ToUInt16(b, 0);
                                masterWriteMultipleRegisters(wr.address, bb);
                            }
                            else
                            {
                                float[] bb = (float[])wr.value;
                                for (int j = 0; j < wr.len; j++)
                                {
                                    byte[] b = BitConverter.GetBytes(bb[j]);
                                    ushort[] bbb = new ushort[2];
                                    bbb[0] = BitConverter.ToUInt16(b, 2);
                                    bbb[1] = BitConverter.ToUInt16(b, 0);
                                    masterWriteMultipleRegisters((ushort)(wr.address + (j * 2)), bbb);
                                }
                            }
                        }//if
                    }//if
                }//for

            }
            catch (Exception err)
            {
                Util.errorMessage(err.Message, description + "=1");
                if (master[Canal] != null) master[Canal].Dispose();
                if (tcpClient[Canal] != null) tcpClient[Canal].Close();
                master[Canal] = null;
                tcpClient[Canal] = null;
                setCanal();
            }
            //writeQuvery.Clear();
        }

        private void masterWriteMultipleRegisters(ushort address, ushort[] b) // запись множества регистров
        {
            bool flag = true;
            for (int i = 0; i < b.Length; i++)
            {
                if (varhr[i + address] != b[i]) flag = false;
            }
            if (flag) return;
            for (int i = 0; i < 2; i++)
            {
                if (tcpClient[i] != null && tcpClient[i].Connected && master[i] != null)
                {
                    master[i].WriteMultipleRegisters(address, b);
                }
            }

            //Util.errorMessage(" ... masterWriteMultipleRegisters ...", "");
        }

        private void masterWriteSingleRegister(ushort address, ushort v) // запись одного регистра
        {
            if (varhr[address] == v) return;
            // Подсчет времени потраченного на операцию
            //DateTime tm = DateTime.Now;  // переменная хранит начальное время запуска потока
            //Util.time = tm;


            for (int i = 0; i < 2; i++)
            {
                if (tcpClient[i] != null && tcpClient[i].Connected && master[i] != null)
                    master[i].WriteSingleRegister(address, v);
            }

            //DateTime tmnow = DateTime.Now; 
            //int st = (int)((tmnow.Ticks - tm.Ticks) / 10000); 
            //Util.errorMessage(" masterWriteSingleRegister time operation=", st.ToString());
        }

        private void masterWriteMultipleCoils(ushort address, bool[] b)
        {
            bool flag = true;
            for (int i = 0; i < b.Length; i++)
            {
                if (varcoils[i + address] != b[i]) flag = false;
            }
            if (flag) return;
            for (int i = 0; i < 2; i++)
            {
                if (tcpClient[i] != null && tcpClient[i].Connected && master[i] != null) master[i].WriteMultipleCoils(address, b);
            }
        }
        private void masterWriteSingleCoil(ushort address, bool value)
        {
            if (varcoils[address] == value) return;
            for (int i = 0; i < 2; i++)
            {
                if (tcpClient[i] != null && tcpClient[i].Connected && master[i] != null)
                {
                    master[i].WriteSingleCoil(address, value);
                }
            }
        }

        void writeVariable() // функция записи переменных конфигурации
        {

            // Подсчет времени потраченного на операцию
            //DateTime tm = DateTime.Now;  // переменная хранит начальное время запуска потока
            //Util.time = tm;

            // создание дубликата writeQuveryDuble и освобождение исходной коллеции
            // Исправлено Русинов. Зачем тут блокировать и создавать копию очереди?
            //            List<writeComm> writeQuveryDuble = new List<writeComm>();
            //            lock (block_DubBusTcpMaster)
            //            {
            //                for (int i = 0; i < writeQuvery.Count; i++)
            //                {
            //                    writeQuveryDuble.Add(writeQuvery[i]);
            //                }
            //                writeQuvery.Clear();

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
             //            } // lock


            //DateTime tmnow = DateTime.Now;
            //int st = (int)((tmnow.Ticks - tm.Ticks) / 10000);
            //Util.errorMessage(" writeVariable time operation=", st.ToString());

        }
        // выбор канала для чтения из устройств . Не забываем что пишем всегда в оба канала по возможности
        bool setCanal()
        {
            device.status = 0;
            device.ready = true;

            /*
            bool statusConnect = false;
            for (int i = 0; i < 2; i++)
            {
                statusConnect = false;
                if (tcpClient[i] != null && master[i] != null && tcpClient[i].Connected)
                {
                    statusConnect = true;
                    Canal = i;
                }
            }

            if (statusConnect) return true;
            else
            {
                device.status = -1;
                device.ready = false;
                return false;
            } 
            /**/

            if (tcpClient[Canal] != null && master[Canal] != null && tcpClient[Canal].Connected) return true;
            Canal = Canal == 0 ? 1 : 0;
            if (tcpClient[Canal] != null && master[Canal] != null && tcpClient[Canal].Connected) return true;
            device.status = -1;
            device.ready = false;
            return false;
        }
        override public void Run() // переопределяемая функция запуска програмного потока для обслуживания работающего дубликата драйвера Модбас
        {
            do
            {


                //lock (block_DubBusTcpMaster)
                {
                    // счетчик таймера операций DubBusTCPMaster

                    DateTime tm = DateTime.Now;  // переменная хранит начальное время запуска потока
                    Util.time = tm;
                    //if (!setCanal()) break;
                    if (!setCanal())
                    {
                        masterDispose();
                        tcpClientClose();
                        device.status = 280;
                        device.ready = false;
                        break;
                    }


                    cleanQuery();
                    try
                    {
                        if (lencoils > 0) varcoils = master[Canal].ReadCoils(0, lencoils);
                        if (lendi > 0) vardi = master[Canal].ReadInputs(0, lendi);
                        if (lenir > 0)
                        {
                            ushort stadr = 0;
                            int len = lenir;
                            do
                            {
                                ushort lenl = (len > 100) ? (ushort)100 : (ushort)len;
                                ir = master[Canal].ReadInputRegisters(stadr, lenl);
                                //master[Canal].
                                for (int i = 0; i < lenl; i++) varir[stadr + i] = ir[i];

                                stadr += 100;
                                len -= 100;
                            } while (len > 0);

                            /*
                            if (tcpClient[Canal] == null)
                            {
                                tcpClient[Canal] = new TcpClient();
                                tcpClient[Canal].Connect(param.ip, param.port);
                            }

                            }
                            {

                                if (master[Canal] == null)
                            {
                                master[Canal] = ModbusIpMaster.CreateIp(tcpClient[Canal]);
                                master[Canal].Transport.Retries = 2;
                                master[Canal].Transport.WriteTimeout = device.timeout;
                                master[Canal].Transport.ReadTimeout = device.timeout;
                            }
                            /**/

                        }
                        if (lenhr > 0)
                        {
                            ushort stadr = 0;
                            int len = lenhr;
                            do
                            {
                                ushort lenl = (len > 100) ? (ushort)100 : (ushort)len;
                                //try
                                {
                                    hr = master[Canal].ReadHoldingRegisters(stadr, lenl);
                                }
                                //catch (Exception)
                                {
                                    //for (int k = 0; k < hr.Length; k++) if (hr != null) hr[k] = 0;

                                }
                                //hr = master[Canal].ReadHoldingRegisters(stadr, 100);
                                for (int i = 0; i < lenl; i++) varhr[stadr + i] = hr[i];
                                stadr += 100;
                                len -= 100;
                            } while (len > 0);
                        }
                    }
                    catch (Exception err)
                    {
                        Util.errorMessage(err.Message, description + "=2");
                        if (master[Canal] != null) master[Canal].Dispose();
                        if (tcpClient[Canal] != null) tcpClient[Canal].Close();
                        master[Canal] = null;
                        tcpClient[Canal] = null;
                        if (!setCanal())
                        {
                            masterDispose();
                            tcpClientClose();
                            device.status = 280;
                            device.ready = false;
                            break;
                        }
                        continue;
                    }
                    writeVariable();

                    //Thread.Sleep(500);


                    DateTime tmnow = DateTime.Now; // переменная хранит время завершения операции в потоке
                    int st = (int)((tmnow.Ticks - tm.Ticks) / 10000); // если превышено время обхода в 100 миллисекунд генерируется предупреждение с указанием колличества циклов
                                                                      // Util.errorMessage("Время потраченное на операции в DubBusTCPMaster : ", st.ToString());
                                                                      //if (st > device.steptime) { Util.errorMessage("Очень долго...." + 
                                                                      //st.ToString() + " | step time system=" + device.steptime, "; << DubBusTCPMaster >>"); st = 0; }


                    if (st > device.steptime)
                    {
                        Util.errorMessage("Время обработки цикла " +
                   st.ToString() +
                   " превысило заданный период  " +
                   device.steptime + "  для устройства ",
                   description);
                        st = 0;
                    }


                    else
                    {
                        st = device.steptime - st; // Util.errorMessage("Нормальный режим работы с задержкой[" + st.ToString() + " ms.] | step time system=" + device.steptime, "; << DubBusTCPMaster >>");
                    }
                    if (st > 0) Thread.Sleep(st);/**/

                    //Thread.Sleep(device.steptime);

                }
            } while (true);
        }

        override public void stopDriver()// переопределяема функция остановки дубликата драйвера
        {
            if (drvthr != null)
            {
                drvthr.Abort();
                drvthr.Join();
            }

            drvthr = null;
            masterDispose();
            tcpClientClose();
            device.status = 0;
            device.ready = false;
        }

        private void tcpClientClose()
        {
            if (tcpClient[0] != null) tcpClient[0].Close();
            if (tcpClient[1] != null) tcpClient[1].Close();
            tcpClient[1] = null;
            tcpClient[0] = null;

        }

        private void masterDispose()
        {
            if (master[0] != null) master[0].Dispose();
            if (master[1] != null) master[1].Dispose();
            master[0] = null;
            master[1] = null;
        }
    }
}

