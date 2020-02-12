using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApaxLib
{
    public class Variable
    {
        object value;               //хранение значения переменной
        int device=0;               // ссылка на контест устройства к которму принадлежит переменная
        int context = 0;            // контекст самой переменной тоесть ее номер 
        Util.TypeVar type;          // тип переменной 
        int size=1;
        bool onlyread=false;              // true менять может только хозяин (устройство) либо при создании 
        List<int> reference = new List<int>(); 
        public List<int> blinds = new List<int>();
        public DateTime time=Util.time;
        public Variable( Util.TypeVar type)
        {
            this.type = type;
            this.value = Util.DefaultValue(type);
        }
        public Variable(Util.TypeVar type,int size)
        {
            this.type = type;
            this.size = size;
            this.value = Util.DefaultValue(type,size);
        }
        public Variable(Util.TypeVar type,bool onlyread)
        {
            this.type = type;
            this.onlyread = onlyread;
            this.value = Util.DefaultValue(type);
        }
        public Variable(Util.TypeVar type, int size,bool onlyread)
        {
            this.type = type;
            this.onlyread = onlyread;
            this.size = size;
            this.value = Util.DefaultValue(type,size);
        }

        public Variable()
        {
            this.type = Util.TypeVar.Boolean;
            this.value = Util.DefaultValue(type);
        }
        public bool ReadOnly { get { return onlyread; } set { onlyread = value; } }
        public int Device { get { return this.device; } set { this.device = value; } }
        public int Size { get { return this.size; } set { this.size = value; } }
        public object Value { get { return this.value; }  }
        public int conVariable { get { return this.context; } set { this.context=value; } }
        public void appendReference(int index)
        {
            reference.Add(index);
        }
        public void setValue (object extValue)
        {
            if (onlyread) return;
            //if (extValue.Equals(this.value)) return;
            this.time = Util.time;
            this.value = extValue;
            if (this.device != 0)
            {
                Context con = Server.getContext(this.device);
                if (con == null) return;
                if (con.Type != Util.TypeContext.Devices) return;
                Device dev = (Device) con.defContext;
                dev.setValueToDrive(this.context,this.value);
            }
            ModifyBlindsStart();
        }
        public void setConstant(object value)
        {
            this.time = Util.time;
            this.value = value;
        }


        public void setValueFromDevice(object extValue)
        {
            if (extValue.Equals(this.value)) return;
            this.time = Util.time;
            this.value = extValue;
            ModifyBlindsStart();
        }
        public Util.TypeVar Type { get { return type; } }
        public int Count { get { return reference.Count; } }

        private void ModifyBlindsStart()
        {
            if(reference.Count==0) return;
            for (int i=0;i<reference.Count;i++)
            {
                Context con = Server.getContext(reference[i]);
                if (con == null) continue;
                if (con.Type != Util.TypeContext.Blinds) continue;
                Blind bl = (Blind) con.defContext;
                if (bl == null) continue;
                bl.Flag = true;
            }
        }
        public static void appendVariable(string name, string description, Util.TypeVar type)
        {
            Variable var = new Variable(type);
            var.conVariable = Server.Count;
            Context con = new Context(name, description, Util.TypeContext.Variables, var);
            Server.appendContext(con);
            Server.appendChaild(Server.PointVariables);
        }
        public static void appendVariable(string name, string description, Util.TypeVar type,int size)
        {
            Variable var = new Variable(type,size);
            var.conVariable = Server.Count;
            Context con = new Context(name, description, Util.TypeContext.Variables, var);
            Server.appendContext(con);
            Server.appendChaild(Server.PointVariables);
        }
        public static void appendVariable(string name, string description, Variable var)
        {
            var.conVariable = Server.Count;
            Context con = new Context(name, description, Util.TypeContext.Variables, var);
            Server.appendContext(con);
            Server.appendChaild(Server.PointVariables);
        }

        public static Variable getVar(int ivar)
        {
            Context con = Server.getContext(ivar);
            if (con == null) return null;
            if (con.Type != Util.TypeContext.Variables) return null;
            Variable var = (Variable)con.defContext;
            if (var == null) return null;
            return var;
        }

        public static Util.TypeVar getVarType(int ivar)
        {
            Context con = Server.getContext(ivar);
            if (con == null) return Util.TypeVar.Error;
            if (con.Type!= Util.TypeContext.Variables) return Util.TypeVar.Error;
            Variable var = (Variable) con.defContext;
            if(var==null) return Util.TypeVar.Error;
            return var.Type;
        }
        public static bool isVarReadOnly(int ivar)
        {
            Context con = Server.getContext(ivar);
            if (con == null) return false;
            if (con.Type != Util.TypeContext.Variables) return false;
            Variable var = (Variable)con.defContext;
            return var.ReadOnly;
        }
        public static object getVariable(int ivar)
        {
            Context con = Server.getContext(ivar);
            if (con == null) return null;
            if (con.Type != Util.TypeContext.Variables) return null;
            Variable var = (Variable)con.defContext;
            return var.Value;
        }
        public static void setVariable(int ivar,object value)
        {
            Context con = Server.getContext(ivar);
            if (con == null) return;
            if (con.Type != Util.TypeContext.Variables) return ;
            Variable var = (Variable)con.defContext;
            var.setValue(value);
            //Util.errorMessage("value: ",value.ToString());
            Server.setContext(ivar, con);
        }
        public static void setVariable(int ivar, object value,bool fromdriver)
        {
            if (!fromdriver) { setVariable(ivar, value); return; }
            Context con = Server.getContext(ivar);
            if (con == null) return;
            if (con.Type != Util.TypeContext.Variables) return;
            Variable var = (Variable)con.defContext;
            var.setValueFromDevice(value);
            Server.setContext(ivar, con);
        }
    }
}
