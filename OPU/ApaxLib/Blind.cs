using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// В данной библиотеке:
// 1. производится обработка состяний на вывод на лампочки
// 2. для этого создаются классы тонкой настройки заедржек по времени



namespace ApaxLib
{
    public class Blind
    {
        public List<int> listParamIn=new List<int>();// номера переменных для функции
        public List<int> listParamOut = new List<int>();
        int numberFunction=0;                   // номер функции
        bool flag = false;                      // при очередном проходе нужен расчет если истина
        bool onstart = false;                   // запуск однократно при старте если истина
        bool ontimer = false;                   // запуск от внешего таймера если истина
        long steptime=-1L;                      // интервал запуска по времени в миллисекундах
        long nextstart=0L;                         // время следующего запуска по времени  в миллисекундах
        public Blind(int numberFunction,bool onstart)
        {
            this.numberFunction = numberFunction;
            this.onstart = onstart;
        }
        public void appendParameter(int paramin,int paramout)
        {
            if (paramin < 0 || paramout<0) return;
            listParamIn.Add(paramin);
            listParamOut.Add(paramout);
        }
        public bool OnStart { get { return onstart; } set { onstart = value; } }
        public bool OnTimer { get { return ontimer; } set { ontimer = value; } }
        public long StepTime { get { return steptime; } }
        public void setOnTimer(long bazatime,long steptime)
        {
            if (bazatime <= 0 || steptime <= 0) return;
            this.ontimer = true;
            this.steptime = steptime;
            this.nextstart = bazatime + steptime;
        }
        public bool goBlind(long systemtime )
        {
            
            if (onstart) { flag = true; onstart = false; }
            if (!flag && !ontimer) return false;
            if (ontimer && (nextstart<=systemtime)) {
                //Console.WriteLine("Goblind..Time..");
                nextstart = systemtime + steptime; flag = true; }
            if (!flag) return false;
            Function.Start(numberFunction, listParamIn,listParamOut); ///....?????
            return true;
        }
        public bool Flag { get { return flag; } set { flag = value; } } // переменная сущность как флаг состояния
        static public void  appendBlind(string name, string description, Blind blind)
        {
            Context con = new Context(name, description, Util.TypeContext.Blinds, blind);
            Server.appendContext(con);
            Server.appendChaild(Server.PointBlinds);
        }
    }
}
