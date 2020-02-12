using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// В данной библиотеке : 
// 1. Вычисляется время отклика обхода всех устройств - норма 100 миллисекунд
// 2. для этого создается функция потока по устройствам, также системные вычислители времени на каждую операцию
// 3. 




namespace ApaxLib
{
    public class LoopDevices
    {
        public DateTime tm;
        public int steptime;
        public LoopDevices(int steptime) // конструктор-функция цикла обхода всех устройств
        {
            DateTime tm = DateTime.Now;
            this.steptime = steptime;
        }
        public void Run() // функция запуска потока по всем устройствам для вычисления общего времени 
        {
            Util.message("Запустили поток по  устройствам");
            Context devcon = Server.getContext(Server.PointDevices);
            do
            {
                tm = DateTime.Now;
                Util.time = tm;

                for (int i = 0; i < devcon.Count; i++)
                {
                    Context dv = Server.getContext(devcon.getChaild(i));
                    if (dv == null) { Util.errorMessage("Нет контеста устройства ", i.ToString()); return; }
                    if (dv.Type != Util.TypeContext.Devices) { Util.errorMessage("В контексте не устройство ", i.ToString()); return; }
                    Device dev = (Device)dv.defContext;
                    if (dev.ready&&dev.single) continue;
                    if (dev.single)
                    {
                        Util.message("Попытка разбудить " + dv.Name);
                    }
                    dev.Reconnect();
                }
                DateTime tmnow = DateTime.Now;
                TimeSpan te = tmnow - tm;
                double st = te.TotalMilliseconds;
                //doouble st = (int)((tmnow.Ticks - tm.Ticks)/10000);
                //if (((int) st) > steptime) { Util.errorMessage("Devices Очень долго....","LoopDevices"); st = 0; }
                if (((int)st) > steptime)
                {
                    Util.errorMessage("Предупреждение!!! Время цикла обработки [ " +
                    st.ToString() +
                    " ] превысило нормальное значение работы цикла [ " +
                    steptime + " ]; \r\n",
                    "[[[ LoopDevices ]]]"); st = 0;
                }

                else st = steptime - st;
                //Util.message("Ждем Devices" + st.ToString());
                if (st > 0) Thread.Sleep((int)st);
            } while (true);
        }
    }
    public class MainLoop
    {
        public long intime;
        public DateTime tm;
        public int steptime;

        public MainLoop(int steptime)
        {
            DateTime tm = DateTime.Now;
            this.intime = tm.Ticks;
            this.steptime = steptime;
        }
        public void Run()
        {
            Util.message("Запустили поток по формулам");
            Context blcon = Server.getContext(Server.PointBlinds);
            do
            {
                tm= DateTime.Now;
                Util.time = tm;
                this.intime = tm.Ticks;
                int count = 0;
                    for (int i = 0; i < blcon.Count; i++)
                    {
                        //Util.message("i=" + i.ToString());
                        Context bc = Server.getContext(blcon.getChaild(i));
                        if (bc == null) { Util.errorMessage("Нет контеста формулы ", i.ToString()); return; }
                        if (bc.Type != Util.TypeContext.Blinds) { Util.errorMessage("В контексте не формула ", i.ToString()); return; }
                        Blind bb = (Blind)bc.defContext;
                        count+=(bb.goBlind(intime))?1:0;
                    }
                DateTime tmnow = DateTime.Now;
                int st =(int) ((tmnow.Ticks - tm.Ticks)/10000); // если превышено время обхода в 100 миллисекунд генерируется предупреждение с указанием колличества циклов
                if (st > steptime) {
                    Util.errorMessage(
                    "Предупреждение!!! Время цикла обработки [" + 
                    st.ToString() + 
                    "] превысило нормальное значение работы цикла [" +
                    steptime+"]; колличество циклов "+count+"\r\n","MainLoop"); st = 0;
                }
                else st = steptime - st;
                //Util.message("Ждем "+st.ToString());
                if (st>0)Thread.Sleep(st);
                Util.blink = !Util.blink;
            } while (true);
        }
    }
}
