using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;


namespace xmldata
{


    public class OPURW
    {

        public OPURW()
        {

            string startxml= @"OPURight.xml";


            XElement booksFromFile = XElement.Load(startxml);
            Console.WriteLine(booksFromFile); Console.WriteLine(" \n ------------------------------ \n");

            XmlDocument xml = new XmlDocument();
            xml.Load(startxml);

            foreach (XmlNode n in xml.SelectNodes("Apax/Devices/device"))
            {
                string name;                
                name = n.Attributes["name"].Value;
                string stype;
                stype = n.Attributes["type"].Value;

                if(stype== "RezCanal") Console.WriteLine(" ---- RezCanal --------------- \n");

                Console.WriteLine(" ---- xml    : stype="+ stype+ "; name= "+ name);




            }



            Console.WriteLine(" \n ------------------------------ \n");

            XmlTextReader reader = new XmlTextReader(startxml);
            //xml.SelectNodes("Apax/Devices/device");
            StringBuilder s = new StringBuilder();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    /*case XmlNodeType.XmlDeclaration:
                        s.AppendLine("Описание: " + reader.Name + " Value =" + reader.Value);
                        break;*/
                     case XmlNodeType.Element:
                        s.AppendLine("Элемент: " + reader.Name);
                        while (reader.MoveToNextAttribute())
                        {
                            s.Append("Атрибут: " + reader.Name + " Value = " + reader.Value);
                        }
                        s.AppendLine();
                        break;
                    /*case XmlNodeType.Text:
                        s.AppendLine("Текст Value = " + reader.Value);
                        break;*/
                }
            }
            reader.Close();

            //Console.Write(s.ToString()); 


        }



        class xml2
        {


            public void xml3()
            {
                XPathDocument document = new XPathDocument("books.xml");
                XPathNavigator navigator = document.CreateNavigator();
                XPathNodeIterator nodes = navigator.Select("bookstore/book");

                while (nodes.MoveNext())
                {
                    Console.WriteLine(nodes.Current.Name);
                }

                Console.ReadKey();



                string fileName = "books.xml";
                XPathDocument doc = new XPathDocument(fileName);
                XPathNavigator nav = doc.CreateNavigator();

                // Compile a standard XPath expression
                //XPathExpression expr;
                //expr = nav.Compile(expression);
                //XPathNodeIterator iterator = nav.Select(expr);
                // Iterate on the node set
            }


            public void xml4()
            {

                string path = "books4.xml";//путь до файла
                XmlTextReader reader = new XmlTextReader(path);
                StringBuilder s = new StringBuilder();
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.XmlDeclaration:
                            s.AppendLine("Описание: " + reader.Name + " Value =" + reader.Value);
                            break;
                        case XmlNodeType.Element:
                            s.AppendLine("Элемент: " + reader.Name);
                            while (reader.MoveToNextAttribute())
                            {
                                s.Append("Атрибут: " + reader.Name + " Value = " + reader.Value);
                            }
                            s.AppendLine();
                            break;
                        case XmlNodeType.Text:
                            s.AppendLine("Текст Value = " + reader.Value);
                            break;
                    }
                }
                reader.Close();

                Console.Write(s.ToString());

            }
        }




            class Program
               {
        



            }
            static void Main(string[] args)
            {

            //xml2 apacsParseConfig = new xml2();
            //apacsParseConfig.xml3();

            OPURW apacsParse = new OPURW();
            


            Console.WriteLine("\n\n Press Key \n\n");

            Console.ReadKey();

            //Получили:




        }
        }
    }
