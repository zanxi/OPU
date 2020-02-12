using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace xmldata
{
    class Program
    {
        class xml2
        {
            public xml2()
            {
                XmlDocument regxml = new XmlDocument();
                //regxml.Load(namefile);            
                foreach (XmlNode n in regxml.SelectNodes("table/records/record"))
                {
                    string name = "", description = "", address = "0", ssize = "1", type = "0", format = "2", unitId = "1";
                    foreach (XmlNode m in n.ChildNodes)
                    {

                        if ("description".Equals(m.Attributes["name"].Value)) description = m.InnerText;
                        if ("address".Equals(m.Attributes["name"].Value)) address = m.InnerText;
                        if ("size".Equals(m.Attributes["name"].Value)) ssize = m.InnerText;
                        if ("type".Equals(m.Attributes["name"].Value)) type = m.InnerText;
                        if ("format".Equals(m.Attributes["name"].Value)) format = m.InnerText;
                        if ("unitId".Equals(m.Attributes["name"].Value)) unitId = m.InnerText;
                    }
                    ushort size = ushort.Parse(ssize);
                    ushort iaddress = ushort.Parse(address);
                    int typereg = int.Parse(type);
                    int iformat = int.Parse(format);


                    /* создаем переменную*/

                }
            

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
                    XPathExpression expr;
                //expr = nav.Compile(expression);
                //XPathNodeIterator iterator = nav.Select(expr);

                    // Iterate on the node set

                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

            }
            static void Main(string[] args)
            {




                

                string pathToXml = "books5.xml";

                XmlTextWriter textWritter = new XmlTextWriter(pathToXml, Encoding.UTF8);
                textWritter.WriteStartDocument(); // Создаём в файле заголовок XML-документа
                textWritter.WriteStartElement("head"); //Создём голову (head)
                textWritter.WriteEndElement(); // Закрываем её

                textWritter.Close(); // И закрываем наш XmlTextWriter



            //Получили:




            }
        }
    }
}
