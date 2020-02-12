using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// В данном классе определяются действия с устройствами:
// 1. определен объект устройства со всеми его свойствами, и методами(также на случай неопределенного или ненайденного устройства)

namespace ApaxLib
{
    public class Device
    {

        public List<int> listVariables;
        public Driver driver = null;

        public bool ready = false;
        public int status;
        public int steptime;                                // интервал запуска по времени в миллисекундах
        public long nextstart = 0L;                         // время следующего запуска по времени  в миллисекундах
        public bool master =false;
        public bool tcp = false;
        public bool single = true;
        public int timeout;
        public Util.TypeDriver type= Util.TypeDriver.NotDriver;
        public Device(int steptime)
        {
            this.steptime = steptime;
            listVariables=new List<int>();
        }
        public void setValueToDrive(int conVariable,object value) // установка внутренних значений устройства
        {
            for (int i = 0; i < listVariables.Count; i++)
            {
                if(listVariables[i]== conVariable)
                {
                    driver.setValue(conVariable, value);
                }
            }
        }
        public void stepDevice(long time) //  функция совершения отдного такта устройством,  протяженностью определенной параметром в скобках
        {
            if (!ready && nextstart < time)
            {
                InitDevice();
                DateTime tm = DateTime.Now.AddTicks(steptime*10000);
                nextstart = tm.Ticks;
            }
        }
        public void stopDevice() // функция остановки устройства
        {
            if (driver == null) return;
            driver.stopDriver();
        }

        public void InitDevice() // функция инициализации устройства
        {
            if (driver == null) return;
            driver.initDriver();
        }
        public void Reconnect() // функция переподключения устройства
        {
            if (driver == null) return;
            driver.Reconnect();
        }
        public string StatusDevice() // статус устройства
        {
            string s1= "Устройство " + (master ? "мастер" : "подчиненный") + " Готовность " + (ready ? "есть" : "нет") + " Код соcтояния " + status.ToString() + "\n";
            s1 += "Интервал запуска " + steptime.ToString() + " Таймаут " + timeout.ToString()+"\n";
            if (driver == null) return s1;
            return s1 + driver.Status();
        }
    }
}
