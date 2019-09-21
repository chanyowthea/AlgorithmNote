using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCommon
{
    static class UDPChecksum
    {
        public static void Test()
        {
            System.IO.MemoryStream m = new System.IO.MemoryStream();
            var b = new System.IO.BinaryWriter(m);
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
        }

        private static byte[] crc7tab = new byte[]
        {
            0,
            9,
            18,
            27,
            36,
            45,
            54,
            63,
            72,
            65,
            90,
            83,
            108,
            101,
            126,
            119,
            25,
            16,
            11,
            2,
            61,
            52,
            47,
            38,
            81,
            88,
            67,
            74,
            117,
            124,
            103,
            110,
            50,
            59,
            32,
            41,
            22,
            31,
            4,
            13,
            122,
            115,
            104,
            97,
            94,
            87,
            76,
            69,
            43,
            34,
            57,
            48,
            15,
            6,
            29,
            20,
            99,
            106,
            113,
            120,
            71,
            78,
            85,
            92,
            100,
            109,
            118,
            127,
            64,
            73,
            82,
            91,
            44,
            37,
            62,
            55,
            8,
            1,
            26,
            19,
            125,
            116,
            111,
            102,
            89,
            80,
            75,
            66,
            53,
            60,
            39,
            46,
            17,
            24,
            3,
            10,
            86,
            95,
            68,
            77,
            114,
            123,
            96,
            105,
            30,
            23,
            12,
            5,
            58,
            51,
            40,
            33,
            79,
            70,
            93,
            84,
            107,
            98,
            121,
            112,
            7,
            14,
            21,
            28,
            35,
            42,
            49,
            56,
            65,
            72,
            83,
            90,
            101,
            108,
            119,
            126,
            9,
            0,
            27,
            18,
            45,
            36,
            63,
            54,
            88,
            81,
            74,
            67,
            124,
            117,
            110,
            103,
            16,
            25,
            2,
            11,
            52,
            61,
            38,
            47,
            115,
            122,
            97,
            104,
            87,
            94,
            69,
            76,
            59,
            50,
            41,
            32,
            31,
            22,
            13,
            4,
            106,
            99,
            120,
            113,
            78,
            71,
            92,
            85,
            34,
            43,
            48,
            57,
            6,
            15,
            20,
            29,
            37,
            44,
            55,
            62,
            1,
            8,
            19,
            26,
            109,
            100,
            127,
            118,
            73,
            64,
            91,
            82,
            60,
            53,
            46,
            39,
            24,
            17,
            10,
            3,
            116,
            125,
            102,
            111,
            80,
            89,
            66,
            75,
            23,
            30,
            5,
            12,
            51,
            58,
            33,
            40,
            95,
            86,
            77,
            68,
            123,
            114,
            105,
            96,
            14,
            7,
            28,
            21,
            42,
            35,
            56,
            49,
            70,
            79,
            84,
            93,
            98,
            107,
            112,
            121
        };
        
        // 单字节查表
        // CRC7的除数是 x^7 + x^3 + 1 == 0x89
        // CRC7的除数，去掉首位，0x09
        public static byte CRC7(byte crcIn, byte v)
        {
            // 之所以 <<1 是因为校验码是七位，而这个异或要高位对齐（左边对齐），所以需要向左移动一位
            // B1 ^ 0 -> b1
            // B2 ^ b1 -> b2
            // B3 ^ b2 -> b3
            // ...
            // Bn ^ bn-1 -> bn

            // B1: 0000 0001
            // b1: 0000 1001
            // B2: 0000 0010
            // 0000 1001
            // 
            return crc7tab[(int)crcIn << 1 ^ (int)v];
        }

        public static byte CRC7(byte crcIn, ushort v)
        {
            //little endian
            byte b = CRC7(crcIn, (byte)(v & 0xff));
            return CRC7(b, (byte)((v >> 8) & 0xff));
        }
        public static byte CRC7(byte crcIn, byte[] buf)
        {
            return CRC7(crcIn, buf, buf.Length);
        }
        public static byte CRC7(byte crcIn, byte[] buf, int Count)
        {
            byte b = crcIn;
            for (int i = 0; i < Count; i++)
            {
                b = CRC7(b, buf[i]);
            }
            return b;
        }
    }
}
