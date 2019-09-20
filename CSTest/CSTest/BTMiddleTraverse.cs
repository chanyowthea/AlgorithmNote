using System;
using System.Collections.Generic;

namespace TestCSharpMid
{
    public class BinTree
    {
        public BinTree m_LeftChild;
        public BinTree m_RightChild;
        public int m_Data;
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
 
            MidTraverseIterator(root);
        }
 
        void MidTraverse(BinTree root)
        {
            if (root == null)
            {
                return;
            }
 
            MidTraverse(root.m_LeftChild);
            Console.WriteLine("visit data=" + root.m_Data);
            MidTraverse(root.m_RightChild);
        }
 
        void MidTraverseIterator(BinTree root)
        {
            Stack<BinTree> stack = new Stack<BinTree>();
            BinTree curTree = root; 
            while (stack.Count > 0 || curTree != null)
            {
                if (curTree != null)
                {
                    stack.Push(curTree);
                    curTree = curTree.m_LeftChild; 
                }
                else
                {
                    var temp = stack.Pop();
                    Console.WriteLine("visit data=" + temp.m_Data); 
                    curTree = temp.m_RightChild; 
                }
            }
        }
    }
}
