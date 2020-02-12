using System;
using System.IO;
using System.Windows.Forms;
using ApaxLib;
using System.Xml;
using System.Threading;
using System.Collections.Generic;
using NLog;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Drawing;

namespace Apax
{
    public partial class Apax : Form
    {
        object lockMessageLine = new object();
        

        public static Context viaCon = null;

        TreeViewer treeContext=null; // Дерево параметров модели 
        TreeViewer treeViewSearch = null; // Дерево результата поиска по параметрам модели 
        ViewerText textBox=null;      // Текстовое поле значений параметров   
        ViewerText messageBox = null; // Текстовое поле логов(сообщений) пульта

        TabControlExt tabconTreeViewDeviceAndSearch=null;
        TabPageExt tabPageTreeViewDeviceParam = null;
        TabPageExt tabPageTreeViewSearchResult = null;

        TextBoxExt textBoxSearch = null; // Текстовое поле для поиска по дереву устройств и их параметров (регистров)

        string startxml="";
        public Apax(string startxml)
        {
            InitializeComponent();
            this.startxml = startxml;


            //StartApax();
            
            treeContext = new TreeViewer();
            treeViewSearch = new TreeViewer();
            textBox = new ViewerText();
            messageBox = new ViewerText();
            textBoxSearch = new TextBoxExt();

            tabconTreeViewDeviceAndSearch = new TabControlExt();
            tabPageTreeViewDeviceParam = new TabPageExt();
            tabPageTreeViewSearchResult = new TabPageExt();

            this.treeContext.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeContext_AfterSelect);
            this.treeViewSearch.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeContext_AfterSelect);
            this.textBoxSearch.TextChanged += new EventHandler(this.SearchText);
        }

        void SearchText(object sender, EventArgs args)
        {
            //this.tabconTreeViewDeviceAndSearch.SelectedTab=this.tabPageTreeViewSearchResult;

            string textSearch = textBoxSearch.Text.ToLower();
            //treeViewSearch.Nodes.Clear();
            treeViewSearch.Nodes.Add("Результаты:");
            treeViewSearch.Nodes.Clear();
            //TreeNode tr = SearchTextInTreeView.FindNode(treeContext,textSearch);
            //TreeNode tr2 = new TreeNode();
            //tr2.c;


            //treeViewSearch.Nodes.Clear();
            //treeViewSearch.Nodes[0].Nodes.Add(tr);
            //treeViewSearch.Nodes.chi.Nodes.Add(tr);

            SearchTextInTreeView.SearchInTree(textSearch,treeContext.Nodes);
            textBox.Text=SearchTextInTreeView.tx;

            try
            {
                string p = "";
                string[] valueTN = new string[2];
                TreeNode tn1 = new TreeNode();
                valueTN = SearchTextInTreeView.listTreeNode[0].Split(new Char[] { '&' });
                tn1 = treeViewSearch.Nodes.Add(valueTN[0]);
                foreach (string s in SearchTextInTreeView.listTreeNode)
                {
                    p = valueTN[0];


                    valueTN = s.Split(new Char[] { '&' });

                    if (p == valueTN[0])
                    {
                        TreeNode tn2 = tn1.Nodes.Add(valueTN[1]);
                        // TreeNode tn3 = tn2.Nodes.Add(valueTN[2]);
                    }
                    else
                    {
                        tn1 = treeViewSearch.Nodes.Add(valueTN[0]);
                    }


                }
                treeViewSearch.ExpandAll();
            }
            catch (Exception)
            {

            }
            //if()
        }



        void StartApax()
        {
            Util.time = DateTime.Now;
            Util.messageLine = new List<string>();
            Util.message("Начинаем загрузку ");
            XmlDocument xml = new XmlDocument();
            xml.Load(startxml);
            if (!ServerWork.loads(xml)) Util.errorMessage("Неверное описание сервера", " ");
            VarWork.loads(xml);
            BlindsWork.LoadBlind(xml);
            ServerWork.LoadDevices(xml);
            BlindsWork.BuildBlind();
            treeContext.BeginUpdate();
            treeContext.Nodes.Clear();
            Context conroot = Server.getContext(Server.PointRoot);
            for (int i = 0; i < conroot.Count; i++)
            {
                Context con = Server.getContext(conroot.getChaild(i));
                treeContext.Nodes.Add(new TreeNode(con.Name));
                for (int j = 0; j < con.Count; j++)
                {
                    Context con1 = Server.getContext(con.getChaild(j));
                    treeContext.Nodes[i].Nodes.Add(new TreeNode(con1.Name + "\t  -->" + con1.Description));
                    if (con1.Count == 0) continue;
                    for (int k = 0; k < con1.Count; k++)
                    {
                        Context con2 = Server.getContext(con1.getChaild(k));
                        treeContext.Nodes[i].Nodes[j].Nodes.Add(new TreeNode(con2.Name + "\t  -->" + con2.Description));
                    }
                }
            }
            treeContext.Sort();
            treeContext.EndUpdate();
            Server.InitDevices();
            Thread.Sleep(10000);  // Спим десять секунд смотрим лампочки
            if (Util.debug)
            {

            }
            if (Util.work)
            {
                Util.status = 0;
                Server.mainloop = new MainLoop(Server.stepserver);
                Server.threadml = new Thread(Server.mainloop.Run);
                Server.loopdevices = new LoopDevices(Server.recondevice);
                Server.threadld = new Thread(Server.loopdevices.Run);
                Server.threadml.Start();
                Server.threadld.Start();

                Server.messthr = new Thread(TCPCombo.Run);
                Server.messthr.Start();
            }
        }

        private void expandVariable(Context con)
        {
            Variable convar = (Variable)con.defContext;
            textBox.Text += convar.ReadOnly ? "Состояние: Только чтение\n" : "Состояние: Можно перезаписать\n";
            textBox.Text += "Время последнего изменения " + convar.time.GetDateTimeFormats()[12] + "\n";
            textBox.Text += "Тип:" + convar.Type + "\n";
            if (convar.Size == 1)
            {
                textBox.Text += "Значение:" + convar.Value.ToString() + "\n";
            }
            else
            {
                textBox.Text += "Значений " + convar.Size.ToString() + " : ";
                switch (convar.Type)
                {
                    case Util.TypeVar.Boolean:
                        {
                            bool[] bb = (bool[])convar.Value;
                            for (int i = 0; i < bb.Length; i++) textBox.Text += bb[i].ToString() + " ";
                            textBox.Text += "\n";
                            break;
                        }
                    case Util.TypeVar.Integer:
                        {
                            int[] bb;
                            try
                            {
                                bb = (int[])convar.Value;

                            }
                            catch 
                            {
                                ushort[] b = (ushort[])convar.Value;
                                bb = new int[b.Length];
                                for(int i = 0; i < bb.Length; i++)  bb[i] = (int)b[i]; 
                            }
                            for (int i = 0; i < bb.Length; i++) textBox.Text += bb[i].ToString() + " ";
                            textBox.Text += "\n";
                            break;
                        }
                    case Util.TypeVar.Float:
                        {
                            float[] bb = (float[])convar.Value;
                            for (int i = 0; i < bb.Length; i++) textBox.Text += bb[i].ToString() + " ";
                            textBox.Text += "\n";
                            break;
                        }
                    case Util.TypeVar.Long:
                        {
                            long[] bb = (long[])convar.Value;
                            for (int i = 0; i < bb.Length; i++) textBox.Text += bb[i].ToString() + " ";
                            textBox.Text += "\n";
                            break;
                        }
                    case Util.TypeVar.Double:
                        {
                            double[] bb = (double[])convar.Value;
                            for (int i = 0; i < bb.Length; i++) textBox.Text += bb[i].ToString() + " ";
                            textBox.Text += "\n";
                            break;
                        }
                    case Util.TypeVar.String:
                        break;
                    case Util.TypeVar.Error:
                        break;
                    default:
                        break;
                }
            }
            if (convar.blinds.Count > 0)
            {
                textBox.Text += "Используется в выражениях:\n";
                for (int i = 0; i < convar.blinds.Count; i++)
                {
                    Context bl = Server.getContext(convar.blinds[i]);
                    textBox.Text += bl.Name+"->"+bl.Description + "\n";
                }
            }
            if (convar.Device > 0)
            {
                Context condev = Server.getContext(convar.Device);
                Device dev = (Device)condev.defContext;
                textBox.Text += "На устройстве " + condev.Name + " " + condev.Description + "\n";

            }
        }
        private void expandContext(Context con)
        {
            viaCon = con;
            textBox.Text = "";
            textBox.Text = Util.TypeContextToString(con.Type)+"\n";
            textBox.Text += "Имя : " + con.Name + "\n";
            textBox.Text += "Описание: " + con.Description + "\n";
            if (con.Type == Util.TypeContext.Root)
            {
                textBox.Text += "Шаг сервера " + Server.stepserver.ToString()+" ms.\n";
                textBox.Text += "Отладчик "+(Util.debug?"включен":"выключен")+"\n";
                if (Server.mainloop == null) { textBox.Text += "Основной процесс не создан\n"; return; }
                if (Server.threadml==null) { textBox.Text += "Поток основного процесса не создан\n"; return; }
                textBox.Text += Server.threadml.IsAlive ? "Поток в работе " : "Поток остановлен";
                
                return;
            }
            if (con.defContext == null) return;
            if (con.Type == Util.TypeContext.Blinds)
            {
                Blind conbl = (Blind)con.defContext;
                if (conbl.OnTimer) textBox.Text += "Запуск по времени. Интервал запуска " + conbl.StepTime.ToString() + " (ms)\n";
                textBox.Text += "Вход:\n";
                for (int i = 0; i < conbl.listParamIn.Count; i++)
                {
                    Context convar = Server.getContext(conbl.listParamIn[i]);
                    textBox.Text += "Номер "+i.ToString()+" "+convar.Name + " " + convar.Description+"\n";
                    expandVariable(convar);
                }
                textBox.Text += "\nВыход:\n";
                for (int i = 0; i < conbl.listParamOut.Count; i++)
                {
                    Context convar = Server.getContext(conbl.listParamOut[i]);
                    textBox.Text += "Номер " + i.ToString() + " " + convar.Name + " " + convar.Description + "\n";
                    expandVariable(convar);
                }
                return;
            }
            if (con.Type == Util.TypeContext.Variables)
            {
                expandVariable(con);
            }
            if (con.Type == Util.TypeContext.Devices)
            {
                Device dev = (Device)con.defContext;

                textBox.Text += dev.StatusDevice();
            }

        }
        private void treeContext_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.ByMouse|| e.Action == TreeViewAction.ByKeyboard)
            {
                for (int i = 0; i < Server.Count; i++)
                {
                    Context con = Server.getContext(i);
                    string text = e.Node.Text;
                    int pos = text.IndexOf("\t");
                    if (pos >= 0)
                    {
                        text = text.Substring(0, pos);
                    }
                    if (con.Name.Equals(text))
                    {
                        expandContext(con);
                        return;
                    }
                }
            }
        }

        private void onesec_Tick(object sender, EventArgs e)
        {
            if (!Util.show) return;
            if(viaCon == null)
            {
                viaCon = Server.getContext(Server.PointRoot);
            }
            expandContext(viaCon);
            Util.show = false;
        }

        private void DebugOn_Click(object sender, EventArgs e)
        {
            Util.debug = true;
        }

        private void DebugOff_Click(object sender, EventArgs e)
        {
            Util.debug = false;
        }

        private void LoopStop_Click(object sender, EventArgs e)
        {
            Server.threadml.Abort();
            Server.threadml.Join();
            Server.threadml = null;
        }

        private void LoopStart_Click(object sender, EventArgs e)
        {
            Server.threadml = new Thread(Server.mainloop.Run);
            Server.threadml.Start();
        }

        private void ShowServer_Click(object sender, EventArgs e)
        {
            viaCon = Server.getContext(Server.PointRoot);
            expandContext(viaCon);
        }



        //public static int portServ = 5510;
        //public static string ipServer = "192.168.10.30";

        
        private void message_Tick(object sender, EventArgs e)
        {            
            Util.time = DateTime.Now;
            lock (lockMessageLine)
            {                
                DateTime tm = DateTime.Now;
                for (int i = 0; i < Util.messageLine.Count; i++)
                {                    
                    messageBox.Text = tm.GetDateTimeFormats()[12] + " "+Util.messageLine[i] + messageBox.Text;
                    //Util.tcpMessage.Add(Util.messageLine[i]);                    
                }

                Util.messageLine.Clear();              
            }            
        }

        private void остановитьВсеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Context devcon = Server.getContext(Server.PointDevices);
            for (int i = 0; i < devcon.Count; i++)
            {
                Context dv = Server.getContext(devcon.getChaild(i));
                if (dv == null) { Util.errorMessage("Нет контеста устройства ", i.ToString()); return; }
                if (dv.Type != Util.TypeContext.Devices) { Util.errorMessage("В контексте не устройство ", i.ToString()); return; }
                Device dev = (Device)dv.defContext;
                dev.stopDevice();
            }

        }

        private void StartDevices_Click(object sender, EventArgs e)
        {
            Context devcon = Server.getContext(Server.PointDevices);
            for (int i = 0; i < devcon.Count; i++)
            {
                Context dv = Server.getContext(devcon.getChaild(i));
                if (dv == null) { Util.errorMessage("Нет контеста устройства ", i.ToString()); return; }
                if (dv.Type != Util.TypeContext.Devices) { Util.errorMessage("В контексте не устройство ", i.ToString()); return; }
                Device dev = (Device)dv.defContext;
                dev.InitDevice();
            }
        }


        private void ShowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Util.show = true;
        }

        private void messageBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void Apax_Load(object sender, EventArgs e)
        {
            this.Location = new Point(10, 10);
            this.Width = (int)(SystemInformation.PrimaryMonitorSize.Width * 0.62);
            this.Height = (int)(SystemInformation.PrimaryMonitorSize.Height * 0.62);
            this.Opacity = 1.0;

            this.Text = "Параметры пульта Управления";

            textBox.ReadOnly = true;
            messageBox.ReadOnly = true;

            textBox.BackColor = Color.White;
            //textBox.ForeColor = Color.Black;
            messageBox.BackColor = Color.White;
            //messageBox.ForeColor = Color.Black;
            textBoxSearch.BackColor = Color.White;
            textBoxSearch.ForeColor = Color.Red;

            PositionElementWinForm();

            //this.Controls.Add(this.treeContext);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.messageBox);
            this.Controls.Add(this.textBoxSearch);

            this.tabPageTreeViewDeviceParam.Controls.Add(this.treeContext);
            this.tabconTreeViewDeviceAndSearch.TabPages.Add(this.tabPageTreeViewDeviceParam);
            this.tabconTreeViewDeviceAndSearch.TabPages[0].Text = "Устройства ОПУ";

            this.tabPageTreeViewSearchResult.Controls.Add(this.treeViewSearch);
            this.tabconTreeViewDeviceAndSearch.TabPages.Add(this.tabPageTreeViewSearchResult);
            //this.tabconTreeViewDeviceAndSearch.TabPages[1].Text = "Результаты поиска по устройства ОПУ";
            this.tabconTreeViewDeviceAndSearch.TabPages[1].Text = "Результаты поиска";

            this.Controls.Add(this.tabconTreeViewDeviceAndSearch);

            StartApax();

        }

        protected override void OnResize(EventArgs e)
        {
            //if (this.Width < seekText.Location.X + seekText.Width+dx)
            //    this.Width = seekText.Location.X + seekText.Width+20;
            if (this.Width < (int)(0.3 * SystemInformation.PrimaryMonitorSize.Width * 0.92))
            {
                this.Width = (int)(0.3 * SystemInformation.PrimaryMonitorSize.Width * 0.92);
                //subsystem.Width = (int)(this.Width * 0.5);
            }

            if (this.Height < (int)(0.3 * SystemInformation.PrimaryMonitorSize.Height * 0.92))
            {
                this.Height = (int)(0.3 * SystemInformation.PrimaryMonitorSize.Height * 0.92);
                //subsystem.Width = (int)(this.Width * 0.5);
            }

            PositionElementWinForm();
            base.OnResize(e);
        }

        void PositionElementWinForm()
        {
            try
            {

                tabconTreeViewDeviceAndSearch.Location = new Point(10, 57);
                tabconTreeViewDeviceAndSearch.Width = (int)((this.Width) * 0.30);
                tabconTreeViewDeviceAndSearch.Height = (int)((this.Height) * 0.9);

                treeContext.Location = new Point(1, 1);
                treeContext.Width = (int)(tabconTreeViewDeviceAndSearch.Width*0.99);
                treeContext.Height = tabconTreeViewDeviceAndSearch.Height;

                treeViewSearch.Location = new Point(1, 1);
                treeViewSearch.Width = (int)(tabconTreeViewDeviceAndSearch.Width * 0.99);
                treeViewSearch.Height = tabconTreeViewDeviceAndSearch.Height;

                textBox.Location = new Point(tabconTreeViewDeviceAndSearch.Location.X+ tabconTreeViewDeviceAndSearch.Width +5, tabconTreeViewDeviceAndSearch.Location.Y);
                textBox.Width = (int)((this.Width- textBox.Location.X-15));
                textBox.Height = (int)((this.Height) * 0.50);

                messageBox.Location = new Point(textBox.Location.X, textBox.Location.Y+ textBox.Height+3);
                messageBox.Width = textBox.Width;
                messageBox.Height = (int)(tabconTreeViewDeviceAndSearch.Height- textBox.Height-5);

                textBoxSearch.Location = new Point(10, 30);
                textBoxSearch.Width = tabconTreeViewDeviceAndSearch.Width;// (int)((this.Width * 0.9 - 20));
                //textBoxSearch.Height = (int)((this.Height) * 0.9);

            }
            catch (Exception err)
            {
                //File.AppendAllText(CreateName.GeneratorNameDateFile("Developer_Tool__Logger__Error", "txt"), err.Message);
            }
        }






        /*
        public static void SearchText(string text, TreeNodeCollection nodes)
        {            
            foreach (TreeNode node in nodes)
            {
                if (node.Text.ToLower().Contains(text.ToLower()))
                {
                    //searchTextInTree.Add(node.Parent.Text + ":" + node.Text);
                    //listTableAndColumns.Add(new TableAndColumn { nameTable = node.Parent.Text, nameColumn = node.Text });
                    //listTable.Add(node.Parent.Text);
                }
                SearchTextCheckChildren(node, text);
            }
        }

        private static void SearchTextCheckChildren(TreeNode rootNode, string text)
        {
            if(node)
            foreach (TreeNode node in rootNode.Nodes)
            {
                SearchTextCheckChildren(node, text);
                if (node.Text.ToLower().Contains(text.ToLower()))
                {
                    //searchTextInTree.Add(node.Parent.Text + ":" + node.Text);
                    //listTableAndColumns.Add(new TableAndColumn { nameTable = node.Parent.Text, nameColumn = node.Text });
                    //listTable.Add(node.Parent.Text);
                }
            }
        }


        /**/



    }
}
