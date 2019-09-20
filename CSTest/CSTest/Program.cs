using GCommon;
using System;
using System.Collections.Generic;
using System.IO;

namespace CSTest
{
    class Program
    {
        static void Main(string[] args)
        {
            MemoryStream m = new MemoryStream();
            var b = new BinaryWriter(m);
            b.Write((byte)1);
            b.Write((byte)2);
            var buffs = m.GetBuffer();
            var index = m.Position;
            byte[] bs = new byte[2];
            for (int i = 0; i < bs.Length; i++)
            {
                bs[i] = buffs[i];
            }
            var crc7 = UDPChecksum.CRC7(0, bs);
            Console.WriteLine(crc7);
            Console.ReadKey();
        }
    }
}
