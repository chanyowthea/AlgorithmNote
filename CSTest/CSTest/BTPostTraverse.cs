using System;
using System.Collections.Generic;

namespace TestCSharpPost
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

            PostTraverseRecurse(root);
            Console.WriteLine("================");
            PostTraverse1(root);
        }

        void PostTraverseRecurse(BinTree root)
        {
            if (root == null)
            {
                return;
            }

            PostTraverse(root.m_LeftChild);
            PostTraverse(root.m_RightChild);
            Console.WriteLine("visit data=" + root.m_Data);
        }

        /// <summary>
        /// 二叉树后序遍历0，先左节点到末端，再右节点，如果没有可以访问的子节点，那么输出数据。先左后右最后退栈检查父节点
        /// </summary>
        /// <param name="root"></param>
        public void PostTraverse(BinTree root)
        {
            Stack<BinNode> nodes = new Stack<BinNode>();
            BinTree curTree = root;
            while (nodes.Count > 0 || curTree != null)
            {
                // 向左寻找子节点，并加入每个途径的节点到栈中，一直到找不到左节点为止
                while (curTree != null)
                {
                    var node = new BinNode();
                    node.m_BinTree = curTree;
                    node.m_HasVisitRight = false;
                    nodes.Push(node);
                    curTree = curTree.m_LeftChild;
                }

                if (nodes.Count > 0)
                {
                    var temp = nodes.Peek();
                    if (!temp.m_HasVisitRight)
                    {
                        temp.m_HasVisitRight = true;
                        curTree = temp.m_BinTree.m_RightChild;
                    }
                    else
                    {
                        Console.WriteLine("Visit Data=" + temp.m_BinTree.m_Data);
                        nodes.Pop();
                        curTree = null;
                    }
                }
            }
        }

        /// <summary>
        /// 逆向思维，将根节点，右节点，左节点依次存入栈中，访问时，依次访问左右根
        /// </summary>
        /// <param name="root"></param>
        public void PostTraverse1(BinTree root)
        {
            Stack<BinTree> nodes = new Stack<BinTree>();
            BinTree curTree = null;
            BinTree preTree = null;
            nodes.Push(root);
            while (nodes.Count > 0)
            {
                curTree = nodes.Peek();
                if (curTree.m_LeftChild == null && curTree.m_RightChild == null
                    || (preTree != null && (preTree == curTree.m_LeftChild || preTree == curTree.m_RightChild)))
                {
                    Console.WriteLine("Visit Data=" + curTree.m_Data);
                    nodes.Pop();
                    preTree = curTree;
                }
                else
                {
                    if (curTree.m_RightChild != null)
                    {
                        nodes.Push(curTree.m_RightChild);
                    }
                    if (curTree.m_LeftChild != null)
                    {
                        nodes.Push(curTree.m_LeftChild);
                    }
                }
            }
        }
    }
}
