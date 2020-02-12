using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// В данном классе построен интерфейс контекста:
// 1. создан класс контекста - области в которой производится обработка данных(сигналов) для преобразования их в конкретные типы
// 2. создаются коллекции данных значений



namespace ApaxLib
{
    public class Context
    {
        private Util.TypeContext type;
        private string name;
        private string description;
        private int parent;
        private List<int> chaildList = null;
        private object context;

        //  класс содержит несколько перегруженных конструкторов контекста с разным числом переменных

        public Context(string name, string description, Util.TypeContext type)
        {
            this.name = name;
            this.description = description;
            this.type = type;
            chaildList = new List<int>();
            this.context = null;
            parent = Util.SetParent(type);
        }
        public Context(string name, string description, Util.TypeContext type,int parent)
        {
            this.name = name;
            this.description = description;
            this.type = type;
            chaildList = new List<int>();
            this.context = null;
            this.parent = parent;
        }
        public Context(string name, string description, Util.TypeContext type, object context)
        {
            this.name = name;
            this.description = description;
            this.type = type;
            chaildList = new List<int>();
            this.context = context;
            this.parent = Util.SetParent(type);
        }

        public int Parent // свойство - хранящее значение родительского объекта
        {
            get { return parent;}
            set { parent = value < 0 ? 0: value;  }
        }
        public int getChaild(int index) // функция получения номера дочернего объекта и возвращения его текущего номера
        {
            if (index < 0 || index >= chaildList.Count) return -1;
            return chaildList[index];
        }
        public void appendChaild(int context) // функция дополнения коллекции дочерним объектом
        {
            if (context < 0) return;
            chaildList.Add(context);
        }
        public string Name { get { return name; } } //  имя
        public string Description { get { return description; } } //  описание
        public Util.TypeContext Type { get { return type; } } // тип контекста
        public int Count { get { return chaildList.Count; } } // список
        public object defContext
        {
            get { return context; }
            set { this.context = value; }
        }
    }
    
}
