using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// В данной библиотеке : 
// 1. определяются константы и модели




namespace ApaxLib
{
    public class Constanta : Variable
    {
        public static void appendVariable(string name, string description, Util.TypeVar type,  object value)
        {
            Variable var = new Variable(type);
            var.conVariable = Server.Count;
            var.Device = 0;
            var.setConstant(value);
            var.ReadOnly = true;
            Context con = new Context(name, description, Util.TypeContext.Variables, var);
            Server.appendContext(con);
            Server.appendChaild(Server.PointConstants);
        }
    }
    public class Model : Variable
    {
        public static void appendVariable(string name, string description, Util.TypeVar type, int size, object value)
        {
            Variable var = new Variable(type, size);
            var.Device = 0;
            if (size==1) var.setConstant(value);
            var.conVariable = Server.Count;
            Context con = new Context(name, description, Util.TypeContext.Variables, var);
            Server.appendContext(con);
            Server.appendChaild(Server.PointModels);
        }
    }

}
