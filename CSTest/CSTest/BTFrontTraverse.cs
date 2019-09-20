using System;
using System.Collections.Generic;

namespace TestCSharpFront
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
            FrontTraverse(root);
            Console.WriteLine("===============================");
            FrontTraverseIterator(root);
        }

        void FrontTraverse(BinTree root)
        {
            if (root == null)
            {
                return;
            }

            Console.WriteLine("visit data=" + root.m_Data);
            FrontTraverse(root.m_LeftChild);
            FrontTraverse(root.m_RightChild);
        }

        void FrontTraverseIterator(BinTree root)
        {
            Stack<BinTree> stack = new Stack<BinTree>();
            stack.Push(root);
            BinTree curTree = root;
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                Console.WriteLine("visit data=" + node.m_Data);
                if (node.m_RightChild != null)
                {
                    stack.Push(node.m_RightChild);
                }

                if (node.m_LeftChild != null)
                {
                    stack.Push(node.m_LeftChild);
                }
            }
        }
    }
}
