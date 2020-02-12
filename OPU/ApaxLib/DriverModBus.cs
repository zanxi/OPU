using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Modbus.Device;
using System.Net.Sockets;
using System.Net;
using Modbus.Data;
using System.IO.Ports;

// В данной библиотеке построен драйвер системы Модбас вместе с интерфейсом-объектами,свойствами и методами:
// 1. создан объект инициализатор драйвера Модбас в том числе через значения регистров из конфигурационных файлов
// 2. создана функция Run для запуска независимого програмного потока данного драйвера
// 3. Как разновидности одного и того же драйвера Modbus созданы драйверы ModBusSerialSlave


namespace ApaxLib
{
 
    public struct ModBusDriverParam
    {
        public string ip;
        public int port;
        public string ipdub;
        public int portdub;
        public string nameport;
        public string encoding;     // RTU ASCII 
        public string portname;     // com1 com2 .....
        public int baudRate;        // 2400 ....
        public int databits;        // 5 6 7 8 
        public StopBits stopbits;        // 1 3 2 
        public Parity parity;          // None=0 Even=2 Odd=1 Mark=3 Space=4
    }
    public struct regModBus
    {
        public int varcontext;
        public int regtype;            // Coil=0 Discret Input=1 Input Register=2 Holding=3
        public int type;                
        public ushort address;
        public ushort len;
        public byte unitId;
    }
    public struct writeComm
    {
        public int reg;                 //Coil=0 Holding=3
        public int type;
        public ushort address;
        public ushort len;
        public byte unitId;
        public object value;
    }

    public class ModBusSerialSlave : DriverModBus
    {
        public ModBusSerialSlave(int device, ModBusDriverParam param) : base(device, param)
        {
            this.device.master = false;
            this.device.tcp = false;
        }
    }



    public class DriverModBus : Driver
    {

        public List<regModBus> registers = new List<regModBus>();
        public List<writeComm> writeQuvery = new List<writeComm>();
        public ModBusDriverParam param;
        public Device device;
        public ushort[] ir=new ushort[1000];
        public ushort[] hr= new ushort[1000];
        public bool[] varcoils;
        public bool[] vardi;
        public ushort[] varir;
        public ushort[] varhr;
        public ushort lencoils =0, lendi=0, lenir=0, lenhr=0;
        public string description;






        public int Canal = 0;


        
        public DriverModBus(int device, ModBusDriverParam param)
        {            
            Context con = Server.getContext(device);
            this.device = (Device)con.defContext;
            this.param = param;
            this.description = "Driver:" + con.Name + " " + con.Description;
        }
        override public void initDriver()
        {            
            for (int i = 0; i < registers.Count; i++) //registers[1].unitId
            {
                ushort size = 1,len;
                if (registers[i].type > 3 && registers[i].type < 11) size = 2;
                if (registers[i].type > 9 && registers[i].type < 16) size = 4;
                len = registers[i].address;
                len += (ushort)(registers[i].len * size);

                if (registers[i].regtype == 0 && len > lencoils) lencoils = len;
                if (registers[i].regtype == 1 && len > lendi) lendi = len;
                if (registers[i].regtype == 2 && len > lenir) lenir = len;
                if (registers[i].regtype == 3 && len > lenhr) lenhr = len;
            }
            varcoils = new bool[lencoils];
            for (int i = 0; i < lencoils; i++) varcoils[i] = false;
            vardi = new bool[lendi];
            for (int i = 0; i < lendi; i++) vardi[i] = false;
            varir = new ushort[lenir];
            for (int i = 0; i < lenir; i++) varir[i] = 0;
            varhr = new ushort[lenhr];
            for (int i = 0; i < lenhr; i++) varhr[i] = 0;
            StartDriver();

            



        }
        override public void StartDriver() // функция запуска драйвера
        {

            drvthr = new Thread(this.Run);
            drvthr.Start();

            device.status = 0;
            device.ready = true;
        }
        override public void stopDriver() // функция остановки драйвера
        {                          // ????????????? предположительно здесь ошибка -  попытка установить соединение была безуспешной, т.к. от другого компьютера за требуемое время не получен нужный отклик
            if (drvthr!=null)
            {
                drvthr.Abort();
                drvthr.Join();
            }
            drvthr = null;

            device.status = 0;
            device.ready = false;
        }
        override public void Run() // переопределяемая функция запуска програмного потока для обслуживания работающего драйвера Модбас
        {
            // эту функцию всегда заменять в текущем драйвере ибо тут собственно цикл обмена
            do
            {
                Thread.Sleep(500);
            } while (true);
        }
        override public void setValue(int ivar, object value) // фунуция инициализации драйвера Модбас
        {
            for (int i = 0; i < registers.Count; i++)
            {
                if (ivar == registers[i].varcontext)
                {
                    regModBus reg = registers[i];
                    if (device.master)
                    {
                        if (reg.regtype == 1 || reg.regtype == 2) return;
                    }
                    lock (block)
                    {
                        writeComm wr = new writeComm();
                        wr.reg = reg.regtype;
                        wr.len = reg.len;
                        wr.type = reg.type;
                        wr.address = reg.address;
                        wr.unitId = reg.unitId;
                        wr.value = value;
                        killOld(wr,writeQuvery);
                        writeQuvery.Add(wr);

                        
                    }
                    return;
                }
            }
        }

        private void killOld(writeComm wr, List<writeComm> writeQuvery) // скрытая фунция обнуления данных полученных от данного драйвера
        {
            //lock (SpinLock.block3) // блокировка доступа к коллеции сигналов до завершения работы над ним одним из потоков драйверов
            {


                for (int i = 0; i < writeQuvery.Count; i++)
                {
                    writeComm w = writeQuvery[i];
                    if ((wr.unitId == w.unitId) && (wr.reg == w.reg) && (wr.address == w.address))
                    {
                        writeQuvery.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        override public string Status() // переопределяемая функция, возвращающая статус драйвера Модбас
        {
            string s="";
            if (device.type==Util.TypeDriver.ModBusTCPMaster) s = "IP=" + param.ip + " Порт=" + param.port.ToString();
            if (device.type == Util.TypeDriver.RezCanal)  s = "IP=" + param.ip + " Порт=" + param.port.ToString() + "\n" + "IP Два=" + param.ipdub + " Порт Два=" + param.portdub.ToString() + " Канал=" + Canal.ToString(); 
            if (device.type == Util.TypeDriver.ModBusTCPSlave) s = "Порт=" + param.port.ToString();
            if (device.type == Util.TypeDriver.ModBusSerialMaster|| device.type == Util.TypeDriver.ModBusSerialSlave)
            {
                s  = "PortName=" + param.portname + " Скорость=" + param.baudRate.ToString()+ " encoding="+param.encoding+"\nflowcontrolIn=";
                s += " databits="+param.databits.ToString()+" stopbits=";
                s += (param.stopbits == StopBits.None) ? "None " : "";
                s += (param.stopbits==StopBits.One)?"One ":"";
                s += (param.stopbits == StopBits.OnePointFive) ? "One point five " : "";
                s += (param.stopbits == StopBits.Two) ? "Two " : "";
                s += (param.parity==Parity.None)?"None":"";          // None=0 Even=2 Odd=1 Mark=3 Space=4
                s += (param.parity == Parity.Even) ? "Even" : "";
                s += (param.parity == Parity.Odd) ? "Odd" : "";
                s += (param.parity == Parity.Mark) ? "Mark" : "";
                s += (param.parity == Parity.Space) ? "Space" : "";
            }
            s += "\nДлина очереди на запись " + writeQuvery.Count.ToString() + "\n";
            s += "\nИмя\t\tТип\t\tFormat\tАдресс\tРазмер\n";
            for (int ii = 0; ii < registers.Count; ii++)
            {
                regModBus regs = registers[ii];
                Context con = Server.getContext(regs.varcontext);
                s += con.Name + "\t" + regs.regtype.ToString() + "\t" + regs.type.ToString() + "\t" + regs.address.ToString() + "\t" + regs.len.ToString()+"\t\t";
                if (regs.len==1)
                {
                    switch (regs.regtype)
                    {
                        case 0: s += varcoils[regs.address].ToString(); break;
                        case 1: s += vardi[regs.address].ToString(); break;
                        case 2: s += varir[regs.address].ToString(); break;
                        case 3: s += varhr[regs.address].ToString(); break;
                    }
                }
                else
                {
                    switch (regs.regtype)
                    {
                        case 0:
                                for (int i = 0; i < regs.len; i++) s += varcoils[regs.address+i].ToString()+" ";
                                break;
                        case 1:
                                for (int i = 0; i < regs.len; i++) s += vardi[regs.address].ToString() + " ";
                                break;
                        case 2:
                            for (int i = 0; i < regs.len; i++) s += varir[regs.address].ToString() + " ";
                            break;
                        case 3:
                            for (int i = 0; i < regs.len; i++) s += varhr[regs.address].ToString() + " ";
                            break;
                    }

                }
                s += "\n";
            }
            return s;
        }
    }
}

