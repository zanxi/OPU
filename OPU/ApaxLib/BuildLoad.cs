using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO.Ports;

// В данной библиотеке : 
// 1. загружаются настройки для имитационного стенда "Apax"  из конфигурационных файлов OPURight.xml (правая часть стенда 192.168.1.173) или OPULeft.xml (левая часть стенда 192.168.1.174)
// 2. В случае отсутствия в конфигурационных файлах из пункта 1. генерируются сообщения, и подставляются значения по умолчанию
// 3. С принятыми параметрами из пунктов 1. и 2. запускается работа следующих технических устройств установки "Apax"


namespace ApaxLib
{
    public struct smallVar           // структура состоит из имен переменных, используемых для считывания значений из конфигурационных файлов пункта 1., приведенного в коментарии в начале файла
    {
        public string name;
        public string description;
        public Util.TypeVar type;
        public object value;
        public int size;
    }
    public static class ServerWork  // класс служит для обработки конфигурационных файлов
    {
        public static bool loads(XmlDocument xml) // функция загрузки конфигурационного файла OPULeft.xml или OPURight.xml
        {
            XmlNode x = xml.SelectSingleNode("Apax/Server"); // выбор подсекции Server основной секции Apax конфигурационного файла OPULeft.xml или OPURight.xml для считывания значений
            if (x == null) return false;
            Server.InitServer(
                               XmlExt.GetAttrValue(x,"name"), 
                               XmlExt.GetAttrValue(x,"description"),                               
                               XmlExt.GetAttrIntValue(x,"lines"),
                               XmlExt.GetAttrIntValue(x,"step"), 
                               XmlExt.GetAttrIntValue(x,"reconect"),
                               XmlExt.GetAttrIntValue(x,"port"),
                               XmlExt.GetAttrValue(x,"ip"),
                               XmlExt.GetAttrValue(x,"ip2")
                              );
            return true;
        }

        public static void parsedevice(string devname)
        {

        }


        public static bool LoadDevices(XmlDocument xml) // функция загрузки конфигурационного файла OPULeft.xml или OPURight.xml
        {
            foreach (XmlNode n in xml.SelectNodes("Apax/Devices/device")) // выбор подподсекции device подподсекции Device основной секции Apax конфигурационного файла OPULeft.xml или OPURight.xml для считывания значений
            {
                string name, description, step;
                int istep;
                name = XmlExt.GetAttrValue(n,"name");
                string Pref = name+":";
                description = XmlExt.GetAttrValue(n,"description");
                
                //try { step = XmlExt.GetAttrValue(n,"step"); }catch (Exception) { step = "0"; }
                step = XmlExt.GetAttrValue(n, "step"); if(step=="") step = "0";

                try
                {
                    istep = int.Parse(step);
                }
                catch (Exception)
                {
                    istep = 0;
                }
                Context condev = new Context(name, description, Util.TypeContext.Devices); // создание назначения/сущности устройства 
                Device dev = new Device( istep); // создание переменной устройства
                condev.defContext = dev;
                Context devroot = Server.getContext(Server.PointDevices);  // получение назначения/сущности устройства устройства и запись в переменную контекста
                int icondev = Server.Count;
                devroot.appendChaild(icondev);
                Server.appendContext(condev);
                Server.setContext(Server.PointDevices, devroot);
                string stype;
                bool master = true;
                Util.TypeDriver type = Util.TypeDriver.NotDriver; // назначен тип отсуствующего драйвера


                stype = XmlExt.GetAttrValue(n,"type"); // считывание типа текущего устройства из конфигурационного файла

                switch (stype)  // Выбор соотвествующего драйвера в зависимости от типа текущего устройства
                {
                    case "ModBusTCPMaster":
                        type = Util.TypeDriver.ModBusTCPMaster; master = true; 
                        break;
                    case "ModBusTCPSlave":
                        type = Util.TypeDriver.ModBusTCPSlave;
                        break;
                    case "ModBusSerialMaster":
                        type = Util.TypeDriver.ModBusSerialMaster; master = true;
                        break;
                    case "ModBusSerialSlave":
                        type = Util.TypeDriver.ModBusTCPSlave;
                        break;
                    case "ApaxInput":
                        type = Util.TypeDriver.ApaxInput;
                        break;
                    case "ApaxOutput":
                        type = Util.TypeDriver.ApaxOutput;
                        break;
                    case "RezCanal":
                        type = Util.TypeDriver.RezCanal; master = true;
                        break;
                    default:                        
                        break;
                }

                
                if (type == Util.TypeDriver.NotDriver)  // если не найден драйвер для устройства, генерируется сообщение и производится выход из обработчика
                {
                      Util.errorMessage("Отсутствует тип драйвера", n.ToString()); return false;
                }


                dev.type = type;
                dev.master = master;
                dev.timeout = XmlExt.GetAttrIntValue(n,"timeout");
                if (type== Util.TypeDriver.ApaxInput|| type == Util.TypeDriver.ApaxOutput)
                {
                    if (type == Util.TypeDriver.ApaxInput) dev.driver = new ApaxInput(icondev);
                    else dev.driver = new ApaxOutput(icondev);


                    string pathLampR;
                    //pathLampR = XmlExt.GetAttrValue(n,"load");                    
                    // блок генерации исключения если отсутствует параметр load адреса, содержащего путь к конфигурационному файлу LampR.xml - значения лампочек слева или справа, генерируется текстовое предупреждение и присваивается значение по умолчанию
                    //try { pathLampR = XmlExt.GetAttrValue(n,"load"); } catch (Exception) { pathLampR = @"c:/OPU/LampR.xml"; Util.errorMessage("Предупреждение: отсутствуют настройки лампочек справа LampR ..... Присвоено по умолчанию:  ", pathLampR); }
                    pathLampR = XmlExt.GetAttrValue(n, "load"); if (pathLampR == "") pathLampR = @"c:/OPU/LampR.xml";

                    LoadRegistersApax(pathLampR, icondev, dev,type); // загрузка значений лампочек в устройство Apax



                    Server.setContext(icondev, condev);
                    continue;
                }
                ModBusDriverParam param = new ModBusDriverParam(); // создание драйвера Modbus

                if (type == Util.TypeDriver.ModBusTCPMaster || type == Util.TypeDriver.ModBusTCPSlave)
                {
                    string TCPIp="127.0.0.1";                        // блок генерации исключения если отсутствует параметр ip, генерируется текстовое предупреждение и присваивается значение по умолчанию
                    if (type == Util.TypeDriver.ModBusTCPMaster) {
                        //try { TCPIp = XmlExt.GetAttrValue(n,"ip"); } catch (Exception) { TCPIp = "127.0.0.1"; Util.errorMessage("Предупреждение: отсутствует параметр IP. Присвоено по умолчанию: ", "127.0.0.1"); }
                        TCPIp = XmlExt.GetAttrValue(n, "ip"); if (TCPIp == "") TCPIp = "127.0.0.1";
                    }
                    param.ip = TCPIp;
                    // блок генерации исключения если отсутствует параметр port, генерируется текстовое предупреждение и присваивается значение по умолчанию

                    //try { param.port = XmlExt.GetAttrIntValue(n,"port")); } catch (Exception) { param.port = 502; Util.errorMessage("Предупреждение: отсутствует параметр Port. Присвоено по умолчанию:  ", 502.ToString()); }
                    param.port = XmlExt.GetAttrIntValue(n, "port"); if (param.port == 0) param.port=502;

                     master = type == Util.TypeDriver.ModBusTCPMaster;
                    Pref = "";
                }
                if (type == Util.TypeDriver.RezCanal)
                {
                    // блок генерации исключения если отсутствует параметр ip, генерируется текстовое предупреждение и присваивается значение по умолчанию
                    //try { param.ip = XmlExt.GetAttrValue(n,"ip"); } catch (Exception) { param.ip = "192.168.1.171"; Util.errorMessage("Предупреждение: отсутствует параметр IP. Присвоено по умолчанию:  ", "192.168.1.171");  }
                    param.ip = XmlExt.GetAttrValue(n, "ip"); if(param.ip=="") param.ip= "192.168.10.35";

                    // блок генерации исключения если отсутствует параметр ipdub, генерируется текстовое предупреждение и присваивается значение по умолчанию
                    //try { param.ipdub = XmlExt.GetAttrValue(n,"ipdub"); } catch (Exception) { param.ipdub = "192.168.1.171"; Util.errorMessage("Предупреждение: отсутствует параметр IPDub. Присвоено по умолчанию:  ", "192.168.1.171"); }
                    param.ipdub = XmlExt.GetAttrValue(n, "ipdub"); if (param.ipdub == "") param.ipdub = "192.168.10.35";

                    // блок генерации исключения если отсутствует параметр port, генерируется текстовое предупреждение и присваивается значение по умолчанию
                    //try { param.port = XmlExt.GetAttrIntValue(n,"port")); } catch (Exception) { param.port = 502; Util.errorMessage("Предупреждение: отсутствует параметр Port. Присвоено по умолчанию:  ", 502.ToString()); }
                    param.port = XmlExt.GetAttrIntValue(n, "port"); if (param.port == 0) param.port = 502;
                    // блок генерации исключения если отсутствует параметр portdub, генерируется текстовое предупреждение и присваивается значение по умолчанию
                    //try { param.portdub = XmlExt.GetAttrIntValue(n,"portdub")); } catch (Exception) { param.portdub = 502; Util.errorMessage("Предупреждение: отсутствует параметр Port. Присвоено по умолчанию:  ", 502.ToString());  }
                    param.portdub = XmlExt.GetAttrIntValue(n, "portdub"); if (param.portdub == 0) param.portdub =502;


                     //try { } catch (Exception) { Util.errorMessage("Предупреждение: отсутствует параметр ..... Присвоено по умолчанию:  ", 502.ToString());  }

                     master = true;
                } 
                if (type == Util.TypeDriver.ModBusSerialMaster || type == Util.TypeDriver.ModBusSerialSlave)
                {
                    string encoding, portname, baudRate,  databits, stopbits, parity;
                    //try { } catch (Exception) { Util.errorMessage("Предупреждение: отсутствует параметр ..... Присвоено по умолчанию:  ", 502.ToString()); }
                    // блок генерации исключения если отсутствует параметр ..., генерируется текстовое предупреждение и присваивается значение по умолчанию
                    
                    //try { encoding = XmlExt.GetAttrValue(n,"encoding"); } catch (Exception) { encoding ="RTU"; Util.errorMessage("Предупреждение: отсутствует параметр encoding. Присвоено по умолчанию:  ", "RTU"); }                    
                    encoding = XmlExt.GetAttrValue(n, "encoding"); if(encoding=="") encoding= "RTU";

                      //try { portname = XmlExt.GetAttrValue(n,"portname"); } catch (Exception) { portname ="COM1"; Util.errorMessage("Предупреждение: отсутствует параметр portname. Присвоено по умолчанию:  ", "COM1"); }
                    portname = XmlExt.GetAttrValue(n, "portname");if (portname == "") portname = "COM1";

                     //try { baudRate = XmlExt.GetAttrValue(n,"baudRate"); } catch (Exception) { baudRate = "38400";  Util.errorMessage("Предупреждение: отсутствует параметр baudRate. Присвоено по умолчанию:  ", "19200"); }
                     baudRate = XmlExt.GetAttrValue(n, "baudRate");if (baudRate == "") baudRate = "38400";

                     //try { databits = XmlExt.GetAttrValue(n,"databits"); } catch (Exception) { databits ="8"; Util.errorMessage("Предупреждение: отсутствует параметр databits. Присвоено по умолчанию:  ", "8"); }
                     databits = XmlExt.GetAttrValue(n, "databits"); if (databits == "") databits = "8";

                     //try { stopbits = XmlExt.GetAttrValue(n,"stopbits"); } catch (Exception) { stopbits ="1";  Util.errorMessage("Предупреждение: отсутствует параметр stopbits. Присвоено по умолчанию:  ", "1"); }
                     stopbits = XmlExt.GetAttrValue(n, "stopbits"); if(stopbits=="") stopbits="1";

                    //try { parity = XmlExt.GetAttrValue(n,"parity"); } catch (Exception) { parity = "None"; Util.errorMessage("Предупреждение: отсутствует параметр parity. Присвоено по умолчанию:  ", "None"); }
                    parity = XmlExt.GetAttrValue(n, "parity"); if (parity == "") parity = "None";

                    //try { } catch (Exception) { Util.errorMessage("Предупреждение: отсутствует параметр ..... Присвоено по умолчанию:  ", 502.ToString()); }                   

                    param.portname = portname;
                    param.encoding = encoding;
                    param.baudRate = int.Parse(baudRate);
                    param.stopbits= "0".Equals(stopbits) ? StopBits.None:("1".Equals(stopbits) ? StopBits.One : ("2".Equals("2") ? StopBits.Two  : StopBits.OnePointFive));

                    param.parity = Parity.Even;

                    switch (parity) // В зависимости от команды протокола RS-232 устанавливается соотвествующее значение 
                    {
                        case "None":
                            param.parity = Parity.None;
                            break;
                        case "Even":
                            param.parity = Parity.Even;
                            break;
                        case "Odd":
                            param.parity = Parity.Odd;
                            break;
                        case "Mark":
                            param.parity = Parity.Mark;
                            break;
                        case "Space":
                            param.parity = Parity.Space;
                            break;
                    }

                    param.databits = int.Parse(databits);
                    master = type == Util.TypeDriver.ModBusSerialMaster;
                    Pref = "";
                }
                switch (type) // в зависимости от типа драйвера создается соотвествующий новый драйвер устройства
                {
                    case Util.TypeDriver.ModBusTCPMaster:
                        dev.driver = new ModBusTCPMaster(icondev, param);
                        break;
                    case Util.TypeDriver.ModBusTCPSlave:
                        dev.driver = new ModBusTCPSlave(icondev, param);
                        break;
                    case Util.TypeDriver.ModBusSerialMaster:
                        dev.driver = new ModBusSerialMaster(icondev, param);
                        break;
                    case Util.TypeDriver.ModBusSerialSlave:
                        dev.driver = new ModBusSerialSlave(icondev, param);
                        break;
                    case Util.TypeDriver.RezCanal:
                        dev.driver = new DubBusTCPMaster(icondev, param);
                        break;

                }
                string pathRegModbus;
                //pathRegModbus = XmlExt.GetAttrValue(n,"load");
                // блок генерации исключения если отсутствует параметр load, генерируется текстовое предупреждение и присваивается значение по умолчанию
                //try { pathRegModbus = XmlExt.GetAttrValue(n,"load"); } catch (Exception) { pathRegModbus = @"c:/OPU/du.xml"; Util.errorMessage("Предупреждение: отсутствуют настройки регистров Modbus du.xml ..... Присвоено по умолчанию:  ", pathRegModbus); }
                pathRegModbus = XmlExt.GetAttrValue(n, "load"); if (pathRegModbus == "") pathRegModbus = @"c:/OPU/du.xml";

                LoadRegistersModBus(pathRegModbus, icondev, dev, type,Pref);
                Server.setContext(icondev, condev);
                continue;
            }
            return true;
        }

        private static void LoadRegistersModBus(string namefile, int icondev, Device dev, Util.TypeDriver drvtype, string Pref) //  функция загрузки регистров Modbus
        {
            XmlDocument regxml = new XmlDocument();
            regxml.Load(namefile);
            DriverModBus driver = (DriverModBus)dev.driver;
            foreach (XmlNode n in regxml.SelectNodes("table/records/record")) // Считывание регистров из указанной подсекции конфигурационного файла
            {
                string name = "", description = "", address = "0", ssize = "1", type = "0", format = "2", unitId = "1";
                foreach (XmlNode m in n.ChildNodes)
                {
                    string attr = XmlExt.GetAttrValue(m,"name"), attr_txt=m.InnerText;

                    switch (attr) //  В зависимости от типа аттрибута, присваивается значение переменной
                    {
                        case "name":
                            name = Pref + attr_txt;
                            break;
                        case "description":
                            description = attr_txt;
                            break;
                        case "address":
                            address = attr_txt;
                            break;
                        case "size":
                            ssize = attr_txt;
                            break;
                        case "type":
                            type = attr_txt;
                            break;
                        case "format":
                            format = attr_txt;
                            break;
                        case "unitId":
                            unitId = attr_txt;
                            break;
                    }
                }
                ushort size = ushort.Parse(ssize);
                ushort iaddress = ushort.Parse(address);
                int typereg = int.Parse(type);
                int iformat = int.Parse(format);
                Util.TypeVar vtype = Util.TypeVar.Error;
                if (typereg == 0 || typereg == 1) vtype = Util.TypeVar.Boolean;
                else
                {
                    if (iformat > 0 && iformat <= 7) vtype = Util.TypeVar.Integer;
                    if (iformat > 7 && iformat < 11) vtype = Util.TypeVar.Float;
                    if (iformat > 10 && iformat < 14) vtype = Util.TypeVar.Long;
                    if (iformat > 13 && iformat < 16) vtype = Util.TypeVar.Double;
                    if (iformat > 17 && iformat < 20) vtype = Util.TypeVar.String;
                }
                /* создаем переменную*/
                Variable.appendVariable(name, description, vtype, size);
                int ipv = Server.Count - 1;
                Context con = Server.getContext(ipv);
                Variable var = (Variable)con.defContext;
                if ((typereg == 1 || typereg == 2) && dev.master) var.ReadOnly = true;
                else var.ReadOnly = false;
                var.Device = icondev;
                regModBus regs = new regModBus();
                regs.varcontext = ipv;
                regs.address = iaddress;
                regs.len = size;
                regs.regtype = typereg;
                regs.type = iformat;
                regs.unitId = byte.Parse(unitId);
                driver.registers.Add(regs);
                dev.listVariables.Add(ipv);
                Server.setContext(ipv, con);
            }
        }
        private static void LoadRegistersApax(string namefile, int icondev, Device dev, Util.TypeDriver drvtype) // Загрузка регистров "Apax"
        {
            XmlDocument regxml = new XmlDocument();
            regxml.Load(namefile);
            DriverApax driver= (DriverApax)dev.driver;
            bool onlyread= drvtype == Util.TypeDriver.ApaxInput;
            foreach (XmlNode n in regxml.SelectNodes("table/records/record")) // из соотвествующей подсекции конфигурационного файла
            {
                string name="", description="", address="0",ssize="1",slot="0";
                foreach(XmlNode m in n.ChildNodes)
                {

                    string attr = XmlExt.GetAttrValue(m,"name"), attr_txt = m.InnerText;

                    switch (attr)
                    {
                        case "name":
                            name = attr_txt;
                            break;
                        case "description":
                            description = attr_txt;
                            break;
                        case "slot":
                            slot = attr_txt;
                            break;
                        case "address":
                            address = attr_txt;
                            break;
                        case "size":
                            ssize = attr_txt;
                            break;
                    }


                }
                int size = int.Parse(ssize);
                int iaddress= int.Parse(address);
                int islot = int.Parse(slot);
                /* создаем переменную*/
                Variable.appendVariable(name, description, Util.TypeVar.Boolean, size);
                int ipv = Server.Count - 1;
                Context con = Server.getContext(ipv);
                Variable var = (Variable)con.defContext;
                var.ReadOnly = onlyread;
                var.Device = icondev;
                regApax regs = new regApax();
                regs.varcontext = ipv;
                regs.slot = islot;
                regs.address = iaddress;
                regs.len = size;
                driver.registers.Add(regs);
                dev.listVariables.Add(ipv);
                Server.setContext(ipv, con);
            }
        }
    }
    public static class VarWork // Класс загружает значения переменных в устройства Apax
    {
        //"/Apax/Variables/var"
        //"/Apax/Constants/var"
        static List<smallVar> listVar = null;
        public static bool loads(XmlDocument xml)
        {
            if (!LoadVars(xml, "/Apax/Variables/var")) { Util.errorMessage("Ошибка загрузки", "/Apax/Variables/var"); return false; }
            if (!BuildModels()) { Util.errorMessage("Ошибка построения", "/Apax/Variables/var"); return false; }
            if (!LoadVars(xml, "/Apax/Constants/var")) { Util.errorMessage("Ошибка загрузки", "/Apax/Constants/var"); return false; }
            if (!BuildConstants()) { Util.errorMessage("Ошибка построения", "/Apax/Constants/var"); return false; }
            return true;
        }
        static bool LoadVars(XmlDocument xml,string area) // загрузка значений переменных
        {
            listVar = new List<smallVar>();
            foreach (XmlNode n in xml.SelectNodes(area))
            {
                smallVar var = new smallVar();
                string type,value,size=null;
                var.name= XmlExt.GetAttrValue(n,"name");
                var.description = XmlExt.GetAttrValue(n,"description");
                
                //try { type = XmlExt.GetAttrValue(n,"type"); } catch (Exception) { type = null; }
                type = XmlExt.GetAttrValue(n, "type"); if (type == "") type="";

                 value =XmlExt.GetAttrValue(n,"value");
                
                //try {size = XmlExt.GetAttrValue(n,"size");} catch (Exception){ size = null; }
                size = XmlExt.GetAttrValue(n, "size"); if (size == "") size ="";

                if (type == null) type = "bool";
                if (size == null) size = "1";
                try
                {
                    var.size = int.Parse(size);
                }
                catch (Exception)
                {
                    var.size = 1;
                }
                var.type = Util.StringToTypeVar(type);

                if (value == null) var.value = Util.DefaultValue(var.type, var.size);
                else var.value = Util.StringToValue(var.type, value);
                listVar.Add(var);
            }
            return true;
        }
        static bool BuildModels()
        {
            for(int i = 0; i < listVar.Count; i++)
            {
                smallVar var = listVar[i];
                Model.appendVariable(var.name, var.description, var.type, var.size, var.value);
                Util.message("переменная " + var.name + " " + var.description + " загружена");
            }
            return true;
        }
        static bool BuildConstants()
        {
            for (int i = 0; i < listVar.Count; i++)
            {
                smallVar var = listVar[i];
                Constanta.appendVariable(var.name, var.description, var.type, var.value);
                Util.message("константа " + var.name + " " + var.description + " загружена");
            }
            return true;
        }
    }

    public struct SmallBlind
    {
        public string nameFunction;
        public List<string> nameParamIn;
        public List<string> nameParamOut;
        public List<bool> ups;
        public bool onstart;
        public long steptimer;
    }
    public static class BlindsWork
    {
        static List<SmallBlind> defBlind = new List<SmallBlind>();
        public static bool LoadBlind(XmlDocument xml)
        {
            string onstart, time;
            foreach (XmlNode k in xml.SelectNodes("/Apax/Blinds"))
            {
                string loadfile;
                //string pathBlinds;
                //loadfile = XmlExt.GetAttrValue(k,"load");
                
                //try { loadfile = XmlExt.GetAttrValue(k,"load"); } catch (Exception) { loadfile = @"c:/OPU/blindR.xml"; Util.errorMessage("Предупреждение: отсутствуют настройки вывода на лампочки Blinds ..... Присвоено по умолчанию:  ", loadfile); }
                loadfile = XmlExt.GetAttrValue(k, "load"); if (loadfile == "") loadfile = @"c:/OPU/blindR.xml";


                 XmlDocument xmlload = new XmlDocument();
                xmlload.Load(loadfile);
                foreach (XmlNode n in xmlload.SelectNodes("/Blinds/blind")) // считывание состояния вывода состояния на лампочки из конфигурационного файла
                {
                    SmallBlind sBl = new SmallBlind();
                    sBl.nameParamIn = new List<string>();
                    sBl.nameParamOut = new List<string>();
                    sBl.ups = new List<bool>();

                    sBl.nameFunction = XmlExt.GetAttrValue(n,"name");
                    //try{ onstart = XmlExt.GetAttrValue(n,"start"); } catch (Exception) { onstart = "true"; }
                    //try { time = XmlExt.GetAttrValue(n,"time"); } catch (Exception) { time = 0; }

                    onstart = XmlExt.GetAttrValue(n,"start");if (onstart == "") onstart= "true"; // ?????
                    time = XmlExt.GetAttrValue(n,"time");if (time == "") time="0";
                    if (onstart == null) onstart = "false";
                    if (time == null) time = "0";
                    try
                    {
                        sBl.onstart = bool.Parse(onstart);
                        sBl.steptimer = int.Parse(time);
                    }
                    catch (Exception)
                    {
                        Util.errorMessage("Ошибка дополнительных параметров ", sBl.nameFunction);
                    }

                    foreach (XmlNode par in n.ChildNodes)
                    {
                        string name, type;

                        name = XmlExt.GetAttrValue(par,"name");
                        type = XmlExt.GetAttrValue(par,"type");
                        if (type == null) type = "in";

                        if (type.Equals("in")) { sBl.nameParamIn.Add(name); sBl.ups.Add(false); }
                        else
                        {
                            if (type.Equals("up")) { sBl.nameParamIn.Add(name); sBl.ups.Add(true); }
                            else sBl.nameParamOut.Add(name);
                        }

                    }
                    defBlind.Add(sBl);
                }
            }
            return true;
        }
        public static void BuildBlind() // Построение состояний лампочек
        {
            // вычислим примерные размеры массива
            SortedList<string, int> sl = null;
            sl = new SortedList<string, int>(Server.Count);
            Context con = null;
            con = Server.getContext(Server.PointVariables);

            for (int i = 0; i < con.Count; i++)
            {
                int index = con.getChaild(i);
                Context var = Server.getContext(index);
                sl.Add(var.Name, index);
            }
            con = Server.getContext(Server.PointModels);

            for (int i = 0; i < con.Count; i++)
            {
                int index = con.getChaild(i);
                Context var = Server.getContext(index);
                sl.Add(var.Name, index);
            }
            con = Server.getContext(Server.PointConstants);

            for (int i = 0; i < con.Count; i++)
            {
                int index = con.getChaild(i);
                Context var = Server.getContext(index);
                sl.Add(var.Name, index);
            }

            DateTime tm = DateTime.Now;

            long bazatime = tm.Ticks;

            for (int i = 0; i < defBlind.Count; i++)
            {
                int numFunction;
                SmallBlind sBl = defBlind[i];
                string name = "Blind" + string.Format("{0:0000}", i);

                string description = sBl.nameFunction + "( ";
                for (int j = 0; j < sBl.nameParamIn.Count; j++)
                {
                    description += j == 0 ? "" : ",";
                    description += sBl.nameParamIn[j] + " in";
                    if (sBl.ups[j]) description += " up";
                }
                for (int j = 0; j < sBl.nameParamOut.Count; j++)
                {

                    description += "," + sBl.nameParamOut[j] + " out";
                }
                description += " )";
                numFunction = Function.NameToInt(sBl.nameFunction);

                if (numFunction < 0) { Util.errorMessage("Отсутствует функция ", description); Util.work = false; }
                Blind BL = new Blind(numFunction, sBl.onstart);
                if (sBl.steptimer > 0) BL.setOnTimer(bazatime, sBl.steptimer);
                for (int j = 0; j < sBl.nameParamIn.Count; j++)
                {
                    int ip = sl.IndexOfKey(sBl.nameParamIn[j]);
                    if (ip < 0) { Util.errorMessage("Нет " + sBl.nameParamIn[j], description); Util.work = false; ip = 1; }
                    else
                    {
                        ip = sl[sBl.nameParamIn[j]];
                        if (sBl.ups[j])
                        {
                            Context cont = Server.getContext(ip);
                            Variable var = (Variable)cont.defContext;
                            if (var != null)
                            {
                                var.appendReference(Server.Count);
                                var.blinds.Add(Server.Count);
                                Server.setContext(ip, cont);
                            }
                        }
                        else
                        {
                            Context cont = Server.getContext(ip);
                            Variable var = (Variable)cont.defContext;
                            var.blinds.Add(Server.Count);
                            Server.setContext(ip, cont);
                        }
                        BL.listParamIn.Add(ip);
                    }

                }
                for (int j = 0; j < sBl.nameParamOut.Count; j++)
                {
                    int ip = sl.IndexOfKey(sBl.nameParamOut[j]);
                    if (ip < 0) { Util.errorMessage("Нет " + sBl.nameParamIn[j], description); Util.work = false; ip = 1; }
                    else
                    {
                        ip = sl[sBl.nameParamOut[j]];
                        Context cont = Server.getContext(ip);
                        Variable var = (Variable)cont.defContext;
                        var.blinds.Add(Server.Count);
                        Server.setContext(ip, cont);
                        BL.listParamOut.Add(ip);
                    }
                }
                if (!Function.isCorrect(numFunction, BL.listParamIn, BL.listParamOut)) { Util.errorMessage("Ошибки в параметрах вызова ", description); Util.work = false; };
                Blind.appendBlind(name, description, BL);
                Util.message(name + " " + description + " добавлено ");
            }
            sl = null;
        }
        
    }

    public static class XmlExt
    {
        public static int maxAddres;
        public static Dictionary<String, Driver> defdrv;
        public static String DefultDrv;

        public static List<string> filesXML = new List<string>();
        public static List<string> filesXMLError = new List<string>();
        public static List<string> filesXMLErrorFile = new List<string>();
        public static List<string> vars_Holdi_and_Coil = new List<string>();


        // -------------------------------------------------------------------------
        // переопределенные функции для чтения значений аттрибутов XML с учетом неверных данных в файлах



        public static byte GetAttrByteValue(XmlNode node, string attrName)
        {
            try
            {
                return byte.Parse(node.Attributes[attrName].Value);
            }
            catch (Exception)
            {
                //LoadingUtils.filesXMLError.Add("В целочисленном аттрибуте стоит не счисло или отсутствует параметр: " + attrName + "; в файле: " + LoadingUtils.filesXML[LoadingUtils.filesXML.Count - 1]);
                //LoadingUtils.filesXMLErrorFile.Add(LoadingUtils.filesXML[LoadingUtils.filesXML.Count - 1]);
                //return null;
                return 0;
            }
        }


        public static short GetAttrShortValue(XmlNode node, string attrName)
        {
            try
            {
                return short.Parse(node.Attributes[attrName].Value);
            }
            catch (Exception)
            {
                //LoadingUtils.filesXMLError.Add("В целочисленном аттрибуте стоит не счисло или отсутствует параметр: " + attrName + "; в файле: " + LoadingUtils.filesXML[LoadingUtils.filesXML.Count - 1]);
                //LoadingUtils.filesXMLErrorFile.Add(LoadingUtils.filesXML[LoadingUtils.filesXML.Count - 1]);
                //return null;
                return 0;
            }
        }


        public static int GetAttrIntValue(XmlNode node, string attrName)
        {
            try
            {
                string value = GetAttrValue(node, attrName);
                if (value == null) return 0;
                return int.Parse(node.Attributes[attrName].Value);
            }
            catch (Exception)
            {
                //LoadingUtils.filesXMLError.Add("В целочисленном аттрибуте стоит не счисло или отсутствует параметр: " + attrName + "; в файле: " + LoadingUtils.filesXML[LoadingUtils.filesXML.Count - 1]);
                //LoadingUtils.filesXMLErrorFile.Add(LoadingUtils.filesXML[LoadingUtils.filesXML.Count - 1]);
                //return null;
                return 0;
            }
        }


        public static bool IsAttrValue(XmlNode node, string attrName)
        {
            try
            {
                string val = node.Attributes[attrName].Value;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /**/

        public static string GetAttrValue(XmlNode node, string attrName)
        {
            try
            {
                return node.Attributes[attrName].Value;
            }
            catch (Exception)
            {
                //LoadingUtils.filesXMLError.Add("Отсутствует параметр: " + attrName + "; в файле: " + LoadingUtils.filesXML[LoadingUtils.filesXML.Count - 1]);
                //LoadingUtils.filesXMLErrorFile.Add(LoadingUtils.filesXML[LoadingUtils.filesXML.Count - 1]);
                return "";
            }
        }

        public static XmlNode GetSingleNode(XmlDocument xmlDoc, string nameNode)
        {
            try
            {
                return xmlDoc.SelectSingleNode(nameNode);
            }
            catch (Exception)
            {
                //LoadingUtils.filesXMLError.Add("Ошибка в имени параметра XML: " + nameNode + "; в файле: " + LoadingUtils.filesXML[LoadingUtils.filesXML.Count - 1]);
                //LoadingUtils.filesXMLErrorFile.Add(LoadingUtils.filesXML[LoadingUtils.filesXML.Count - 1]);
                return null;

            }
        }

        public static XmlNodeList GetNodes(XmlDocument xmlDoc, string nameNode)
        {
            try
            {
                return xmlDoc.SelectNodes(nameNode);
            }
            catch (Exception)
            {
                //LoadingUtils.filesXMLError.Add("Ошибка в имени параметра XML: " + nameNode + "; в файле: " + LoadingUtils.filesXML[LoadingUtils.filesXML.Count - 1]);
                //LoadingUtils.filesXMLErrorFile.Add(LoadingUtils.filesXML[LoadingUtils.filesXML.Count - 1]);

                return null;
            }
        }


    }

}
