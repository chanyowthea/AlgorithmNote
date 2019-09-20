using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCSharpBreadth
{
    public class BinTree
    {
        public BinTree m_LeftChild;
        public BinTree m_RightChild;
        public int m_Data;
    }
 
    public class BinNode
    {
        public bool m_HasVisitRight;
        public BinTree m_BinTree;
    }
 
    public class Test
    {
        public void Start()
        {
            BinTree root = new BinTree();
            root.m_Data = 1;
 
            BinTree left = new BinTree();
            left.m_Data = 2;
            left.m_LeftChild = new BinTree { m_Data = 4 };
 
            BinTree right = new BinTree();
            right.m_Data = 3;
            right.m_LeftChild = new BinTree { m_Data = 6 };
            right.m_RightChild = new BinTree { m_Data = 7 };
 
            root.m_LeftChild = left;
            root.m_RightChild = right;
 
            Iterate(root);
            Console.WriteLine("==============");
 
            Console.WriteLine("data=" + root.m_Data);
            Recurse(root);
        }
 
        /// <summary>
        /// 二叉树广度遍历
        /// </summary>
        /// <param name="root"></param>
        void Iterate(BinTree root)
        {
            var curNode = root;
            Queue<BinTree> stack = new Queue<BinTree>();
            stack.Enqueue(root);
            while (stack.Count > 0)
            {
                curNode = stack.Dequeue();
                Console.WriteLine("data=" + curNode.m_Data);
                if (curNode.m_LeftChild != null)
                {
                    stack.Enqueue(curNode.m_LeftChild);
                }
                if (curNode.m_RightChild != null)
                {
                    stack.Enqueue(curNode.m_RightChild);
                }
            }
        }
 
        void Recurse(BinTree root)
        {
            if (root == null)
            {
                return;
            }
            if (root.m_LeftChild != null)
            {
                Console.WriteLine("data=" + root.m_LeftChild.m_Data);
            }
            if (root.m_RightChild != null)
            {
                Console.WriteLine("data=" + root.m_RightChild.m_Data);
            }
            Recurse(root.m_LeftChild);
            Recurse(root.m_RightChild);
        }
    }
}
