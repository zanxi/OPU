using System.Threading;


// В данном классе определен интерфейс драйвера

namespace ApaxLib
{
    public class Driver       
    {
        

        public  object block = new object();
        public Thread drvthr=null;

        virtual public void initDriver()
        {
        }
        virtual public void StartDriver()
        {
        }
        virtual public void stopDriver()
        {
        }
        virtual public void Run()
        {
        }
        virtual public void setValue(int ivar, object value)
        {
        }
        virtual public void Reconnect()
        {
            stopDriver();
            StartDriver();
        }
        virtual public string Status()
        {
            return "";
        }
    }

}
