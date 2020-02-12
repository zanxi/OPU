using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Advantech.Adam;

namespace APAXIO
{
    class Program
    {
        // Global object
        static void Main(string[] args)
        {
            string DEVICE_DO = "5046";
            string DEVICE_DI = "5040";
            AdamControl m_adamCtl=null;
            Apax5000Config m_aConf;
            string[] m_szSlots;        // Container of all solt device type
            m_adamCtl=new AdamControl(AdamType.Apax5000);
            if (!m_adamCtl.OpenDevice())
            {
                Console.WriteLine("Не смог открыть устройства..");
                Console.ReadKey();
                return;
            }
            if (!m_adamCtl.Configuration().GetSlotInfo(out m_szSlots))
            {
                Console.WriteLine("Не прочитать список устройства..");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("Список устройств на шине");
            for (int i = 0; i < m_szSlots.Length; i++)
            {
                Console.Write(i.ToString() + "=" + m_szSlots[i] + " ");
                if ((i+1) % 4 == 0) Console.WriteLine();
            }
            //Console.WriteLine("Состояние каждого устройства");
            for (int i = 0; i < m_szSlots.Length; i++)
            {
                if (m_szSlots[i] == null) continue;
                bool DO = false;
                bool flag = true;
                _HardwareIOType type=_HardwareIOType.None;
                if (DEVICE_DO.Equals(m_szSlots[i])) { type = _HardwareIOType.DO; DO = true; flag = false; }
                if (DEVICE_DI.Equals(m_szSlots[i])) { type = _HardwareIOType.DI; flag = false; }
                if (flag) continue;
                if (i > 0)
                {
                    Console.Write("Press any key...");
                    Console.ReadKey();
                }
                Console.Write("Устройство номер=" + i.ToString() + " тип=" + m_szSlots[i]);
                Console.WriteLine(DO ? " DO" : " DI");
                if (!m_adamCtl.Configuration().GetModuleConfig(i, out m_aConf))
                {
                    Console.WriteLine("Ошибка чтения конфигурации");
                    continue;
                }
                Console.WriteLine(" Имя модуля=" + m_aConf.GetModuleName() + " Каналов=" + m_aConf.byChTotal.ToString());
                int iChannelTotal = 24;
                bool[] bVal=null;
                
                if (type == _HardwareIOType.DO)
                {
                    if (!m_adamCtl.DigitalOutput().GetValues(i, iChannelTotal, out bVal))
                    {
                        Console.WriteLine("ApiErr:" + m_adamCtl.DigitalOutput().ApiLastError.ToString());
                        continue;
                    }
                }
                if (type == _HardwareIOType.DI)
                {
                    if (!m_adamCtl.DigitalInput().GetValues(i, iChannelTotal, out bVal))
                    {
                        Console.WriteLine("ApiErr:" + m_adamCtl.DigitalOutput().ApiLastError.ToString());
                        continue;
                    }
                }

                for (int j = 0; j < bVal.Length; j++)
                {
                    Console.Write(bVal[j].ToString()+" ");
                    if ((j + 1) % 12 == 0) Console.WriteLine();
                    bVal[j] = !bVal[j];
                }
            
                if (type == _HardwareIOType.DO)
                {
                    if (!m_adamCtl.DigitalOutput().SetValues(i,  bVal))
                    {
                        Console.WriteLine("ApiErr:" + m_adamCtl.DigitalOutput().ApiLastError.ToString());
                        continue;
                    }
                }


            }
            Console.Write("Press any key...");
            Console.ReadKey();
        }

    }
}
