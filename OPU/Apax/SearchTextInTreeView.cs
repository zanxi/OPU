using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Apax
{
    class SearchTextInTreeView
    {
        public static int countSearch = 0;
        public static string tx = "";
        public static string tx2 = "";
        public static List<string> listTable = new List<string>();
        public static List<string> listTreeNode = new List<string>();
        public static void SearchInTree(string text, TreeNodeCollection tNodeList)
        {
            listTable.Clear();
            listTreeNode.Clear();
            tx = "";
            tx2 = "";
            SearchText(text, tNodeList);
            tx += "<<< "+listTable.Count+" >>>" + "\r\n";
            foreach (string s in listTable)
            {
               // tx += s + "\r\n";
            }
        }

        private static void SearchText(string text, TreeNodeCollection nodes)
        {
            //searchTextInTree.Clear();
            //listTableAndColumns.Clear();
            //listTable.Clear();
            countSearch = 0;
            foreach (TreeNode node in nodes)
            {
                if (node.Text.ToLower().Contains(text.ToLower()))
                {
                    //tx += node.Parent.Text + ":" + node.Text + "\r\n";
                    //searchTextInTree.Add(node.Parent.Text + ":" + node.Text);
                    //listTableAndColumns.Add(new TableAndColumn { nameTable = node.Parent.Text, nameColumn = node.Text });
                    //listTable.Add(node.Parent.Text);
                }
                SearchTextCheckChildren(node, text);
            }
        }

        private static void SearchTextCheckChildren(TreeNode rootNode, string text)
        {          
             
            foreach (TreeNode node in rootNode.Nodes)
            {
                //SearchTextCheckChildren(node, text);
                if (node.Text.ToLower().Contains(text.ToLower()))
                {
                    //tx += "1-я ветка - <<"+node.Parent.Text + ">>> : подузел - <<<" + node.Text + ">>>"+ node.Nodes[0].Text+ "\r\n";
                    tx += "1-я ветка - <<" + node.Parent.Text + ">>> : подузел - <<<" + node.Text + ">>>" + "\r\n";
                    tx2 += node.Parent.Text+ "&" + node.Text + "\r\n";
                    //tx += node.Parent.Text + ":" + node.Text+"\r\n";
                    //Util..Add(node.Parent.Text + ":" + node.Text);
                    //searchTextInTree.Add(node.Parent.Text + ":" + node.Text);
                    //listTableAndColumns.Add(new TableAndColumn { nameTable = node.Parent.Text, nameColumn = node.Text });
                    listTable.Add(node.Parent.Text);

                    string pathTree = node.Parent.Text + "&" + node.Text;
                    //listTreeNode.Add(node.Parent.Text + "&" + node.Text+"#"+node.Nodes[0].Text);
                    //foreach (TreeNode tn in node.LastNode.Nodes)
                    {
                      //  pathTree += "&"+tn.Text;
                    }
                    listTreeNode.Add(pathTree);


                }
            }
        }

        // 30 апреля 2018
        // http://gitlab.rsreu.ru/vstolchnev/csharpdocs/raw/master/C%23%20%D0%B4%D0%BB%D1%8F%20Windows.pdf
        // 16 page

        public static TreeNode FindNode(TreeView tv, string name)
        {
            foreach (TreeNode tn in tv.Nodes)
            {
                if (tn.Text.ToLower().Contains(name.ToLower()))
                {
                    return tn;
                }
            }
            TreeNode node;
            foreach (TreeNode tn in tv.Nodes)
            {
                node = FindNode(tn, name);
                if (node != null)
                {
                    return node;
                }
            }
            return null;
        }




        private static TreeNode FindNode(TreeNode treenode, string name)
        {
            foreach(TreeNode tn in treenode.Nodes)
            {
                if (tn.Text.ToLower().Contains(name.ToLower()))
                {
                    return tn;
                }
            }

            TreeNode node;
            foreach (TreeNode tn in treenode.Nodes)
            {
                node = FindNode(tn,name);
                if (node != null)
                {
                    return node;
                }                
            }
            return null;
        }









    }
}
