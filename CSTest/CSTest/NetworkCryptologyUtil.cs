using System.Collections.Generic;
using System.Linq;

namespace GCommon
{
    public class TeaDecTempBuffer
    {
        public byte[] dec_dest_buff = new byte[8];
        public byte[] dec_zero_buff = new byte[8];
        public byte[] dec_iv_pre = new byte[8];
        public byte[] dec_iv_cur = new byte[8];
        public uint[] dec_k = new uint[4];
    }
    public class TeaEncTempBuffer
    {
        public byte[] enc_src_buf = new byte[8];
        public byte[] enc_iv_plain = new byte[8];
        public byte[] enc_iv_crypt = new byte[8];
        public uint[] enc_k = new uint[4];
    }

    public static class NetworkCryptologyUtil
    {
        public static void Test()
        {
            System.IO.MemoryStream m = new System.IO.MemoryStream();
            var b = new System.IO.BinaryWriter(m);
            for (int i = 0; i < 512; i++)
            {
                b.Write((byte)1);
                b.Write((byte)2);
            }
            var buffs = m.GetBuffer();
            var index = m.Position;
            byte[] bs = new byte[index];
            for (int i = 0; i < bs.Length; i++)
            {
                bs[i] = buffs[i];
            }
            int buffLen = bs.Length;
            int tempCount = 0;
            var encBuff = new TeaEncTempBuffer();
            var tempBuff = new byte[1420];

            NetworkCryptologyUtil.TeaEncrypt(encBuff, bs, buffLen, TestKey, tempBuff, ref tempCount);
            System.Console.WriteLine(tempCount);
            System.Console.WriteLine(tempBuff);

            var data1 = tempBuff.Take(tempCount).ToArray();
            tempCount = data1.Length;
            var decBuff = new TeaDecTempBuffer();
            var tempBuff1 = new byte[1420];
            NetworkCryptologyUtil.TeaDecrypt(decBuff, data1, tempCount, TestKey, tempBuff1, ref tempCount);
            System.Console.WriteLine(tempCount);
            System.Console.WriteLine(tempBuff1);
        }

        public static byte[] TestKey = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10 };

        public static readonly int SALT_LEN = 2;
        public static readonly int ZERO_LEN = 7;

        /*pKey为16byte*/
        /*
        输入:pInBuf为密文格式,nInBufLen为pInBuf的长度是8byte的倍数; *pOutBufLen为接收缓冲区的长度
        特别注意*pOutBufLen应预置接收缓冲区的长度!
        输出:pOutBuf为明文(Body),pOutBufLen为pOutBuf的长度,至少应预留nInBufLen-10;
        返回值:如果格式正确返回TRUE;
        */
        /*TEA解密算法,CBC模式*/
        /*密文格式:PadLen(1byte)+Padding(var,0-7byte)+Salt(2byte)+Body(var byte)+Zero(7byte)*/
        public static bool TeaDecrypt(TeaDecTempBuffer decTempBuffer, byte[] pInBuf, int nInBufLen, byte[] pKey, byte[] pOutBuf, ref int pOutBufLen)
        {

            int nPadLen, nPlainLen;
            byte[] dest_buf = decTempBuffer.dec_dest_buff;
            byte[] zero_buf = decTempBuffer.dec_zero_buff;
            byte[] iv_pre_crypt = decTempBuffer.dec_iv_pre;
            byte[] iv_cur_crypt = decTempBuffer.dec_iv_cur;

            int dest_i, i, j;
            //byte[] pInBufBoundary;
            int nBufPos = 0;

            int inBufPos = 0;

            if ((nInBufLen % 8) != 0 || (nInBufLen < 16))
            {
                return false;
            }

            TeaDecryptECB(decTempBuffer, pInBuf, inBufPos, pKey, dest_buf, nBufPos);

            nPadLen = dest_buf[0] & 0x7/*只要最低三位*/;

            /*密文格式:PadLen(1byte)+Padding(var,0-7byte)+Salt(2byte)+Body(var byte)+Zero(7byte)*/
            i = nInBufLen - 1/*PadLen(1byte)*/- nPadLen - SALT_LEN - ZERO_LEN; /*明文长度*/
            if (pOutBufLen < i || (i < 0))
            {
                return false;
            }
            pOutBufLen = i;

            //pInBufBoundary = pInBuf + nInBufLen; /*输入缓冲区的边界，下面不能pInBuf>=pInBufBoundary*/


            for (i = 0; i < 8; i++)
                zero_buf[i] = 0;

            System.Buffer.BlockCopy(zero_buf, 0, iv_pre_crypt, 0, 8);
            System.Buffer.BlockCopy(pInBuf, inBufPos, iv_cur_crypt, 0, 8);

            inBufPos += 8;
            nBufPos += 8;

            dest_i = 1; /*dest_i指向dest_buf下一个位置*/


            /*把Padding滤掉*/
            dest_i += nPadLen;

            /*dest_i must <=8*/

            /*把Salt滤掉*/
            for (i = 1; i <= SALT_LEN;)
            {
                if (dest_i < 8)
                {
                    dest_i++;
                    i++;
                }
                else if (dest_i == 8)
                {
                    /*解开一个新的加密块*/

                    /*改变前一个加密块的指针*/
                    System.Buffer.BlockCopy(iv_cur_crypt, 0, iv_pre_crypt, 0, 8);
                    System.Buffer.BlockCopy(pInBuf, inBufPos, iv_cur_crypt, 0, 8);

                    /*异或前一块明文(在dest_buf[]中)*/
                    for (j = 0; j < 8; j++)
                    {
                        if ((nBufPos + j) >= nInBufLen)
                        {
                            return false;
                        }
                        dest_buf[j] ^= pInBuf[inBufPos + j];
                    }

                    /*dest_i==8*/
                    TeaDecryptECB(decTempBuffer, dest_buf, 0, pKey, dest_buf, 0);

                    /*在取出的时候才异或前一块密文(iv_pre_crypt)*/


                    inBufPos += 8;
                    nBufPos += 8;

                    dest_i = 0; /*dest_i指向dest_buf下一个位置*/
                }
            }

            /*还原明文*/

            int outBufPos = 0;

            nPlainLen = pOutBufLen;
            while (nPlainLen > 0)
            {
                if (dest_i < 8)
                {
                    pOutBuf[outBufPos++] = (byte)(dest_buf[dest_i] ^ iv_pre_crypt[dest_i]);
                    dest_i++;
                    nPlainLen--;
                }
                else if (dest_i == 8)
                {
                    /*dest_i==8*/

                    /*改变前一个加密块的指针*/
                    System.Buffer.BlockCopy(iv_cur_crypt, 0, iv_pre_crypt, 0, 8);
                    System.Buffer.BlockCopy(pInBuf, inBufPos, iv_cur_crypt, 0, 8);

                    /*解开一个新的加密块*/

                    /*异或前一块明文(在dest_buf[]中)*/
                    for (j = 0; j < 8; j++)
                    {
                        if ((nBufPos + j) >= nInBufLen)
                        {
                            return false;
                        }
                        dest_buf[j] ^= pInBuf[j + inBufPos];
                    }

                    TeaDecryptECB(decTempBuffer, dest_buf, 0, pKey, dest_buf, 0);

                    /*在取出的时候才异或前一块密文(iv_pre_crypt)*/


                    inBufPos += 8;
                    nBufPos += 8;

                    dest_i = 0; /*dest_i指向dest_buf下一个位置*/
                }
            }

            /*校验Zero*/
            for (i = 1; i <= ZERO_LEN;)
            {
                if (dest_i < 8)
                {
                    if ((dest_buf[dest_i] ^ iv_pre_crypt[dest_i]) != 0)
                    {
                        return false;
                    }
                    dest_i++;
                    i++;
                }
                else if (dest_i == 8)
                {
                    /*改变前一个加密块的指针*/
                    System.Buffer.BlockCopy(iv_cur_crypt, 0, iv_pre_crypt, 0, 8);
                    System.Buffer.BlockCopy(pInBuf, inBufPos, iv_cur_crypt, 0, 8);

                    /*解开一个新的加密块*/

                    /*异或前一块明文(在dest_buf[]中)*/
                    for (j = 0; j < 8; j++)
                    {
                        if ((nBufPos + j) >= nInBufLen)
                        {
                            return false;
                        }
                        dest_buf[j] ^= pInBuf[j + inBufPos];
                    }

                    TeaDecryptECB(decTempBuffer, dest_buf, 0, pKey, dest_buf, 0);

                    /*在取出的时候才异或前一块密文(iv_pre_crypt)*/


                    inBufPos += 8;
                    nBufPos += 8;
                    dest_i = 0; /*dest_i指向dest_buf下一个位置*/
                }

            }
            return true;
        }

        static int rand()
        {
            return 0;
        }


        /*pKey为16char*/
        /*
        输入:nInBufLen为需加密的明文部分(Body)长度;
        输出:返回为加密后的长度(是8char的倍数);
        */
        /*TEA加密算法,CBC模式*/
        /*密文格式:PadLen(1char)+Padding(var,0-7char)+Salt(2char)+Body(var char)+Zero(7char)*/
        public static int TeaEncrypt_Len(int nInBufLen)
        {

            int nPadSaltBodyZeroLen/*PadLen(1char)+Salt+Body+Zero的长度*/;
            int nPadlen;

            /*根据Body长度计算PadLen,最小必需长度必需为8char的整数倍*/
            nPadSaltBodyZeroLen = nInBufLen/*Body长度*/+ 1 + SALT_LEN + ZERO_LEN/*PadLen(1char)+Salt(2char)+Zero(7char)*/;
            nPadlen = nPadSaltBodyZeroLen % 8;
            if (0 != nPadlen) /*len=nSaltBodyZeroLen%8*/
            {
                /*模8余0需补0,余1补7,余2补6,...,余7补1*/
                nPadlen = 8 - nPadlen;
            }

            return nPadSaltBodyZeroLen + nPadlen;
        }


        /*pKey为16char*/
        /*
            输入:pInBuf为需加密的明文部分(Body),nInBufLen为pInBuf长度;
            输出:pOutBuf为密文格式,pOutBufLen为pOutBuf的长度是8char的倍数;
        */
        /*TEA加密算法,CBC模式*/
        /*密文格式:PadLen(1char)+Padding(var,0-7char)+Salt(2char)+Body(var char)+Zero(7char)*/
        public static void TeaEncrypt(TeaEncTempBuffer encTempBuffer, byte[] pInBuf, int nInBufLen, byte[] pKey, byte[] pOutBuf, ref int pOutBufLen)
        {

            int nPadSaltBodyZeroLen/*PadLen(1char)+Salt+Body+Zero的长度*/;
            int nPadlen;
            byte[] src_buf = encTempBuffer.enc_src_buf;
            byte[] iv_plain = encTempBuffer.enc_iv_plain;
            byte[] iv_crypt = encTempBuffer.enc_iv_crypt;
            int src_i, i, j;

            int outBufPos = 0;

            /*根据Body长度计算PadLen,最小必需长度必需为8char的整数倍*/
            nPadSaltBodyZeroLen = nInBufLen/*Body长度*/+ 1 + SALT_LEN + ZERO_LEN/*PadLen(1char)+Salt(2char)+Zero(7char)*/;
            nPadlen = nPadSaltBodyZeroLen % 8;
            if (0 != nPadlen) /*len=nSaltBodyZeroLen%8*/
            {
                /*模8余0需补0,余1补7,余2补6,...,余7补1*/
                nPadlen = 8 - nPadlen;
            }

            /*srand( (unsigned)time( NULL ) ); 初始化随机数*/
            /*加密第一块数据(8char),取前面10char*/
            src_buf[0] = (byte)((((byte)(rand()) & 0x0f8/*最低三位存PadLen,清零*/) | nPadlen));
            src_i = 1; /*src_i指向src_buf下一个位置*/

            while (nPadlen-- > 0)
                src_buf[src_i++] = (byte)rand(); /*Padding*/

            /*come here, src_i must <= 8*/

            for (i = 0; i < 8; i++)
                iv_plain[i] = 0;

            System.Buffer.BlockCopy(iv_plain, 0, iv_crypt, 0, 8);

            pOutBufLen = 0; /*init OutBufLen*/

            for (i = 1; i <= SALT_LEN;) /*Salt(2char)*/
            {
                if (src_i < 8)
                {
                    src_buf[src_i++] = (byte)rand();
                    i++; /*i inc in here*/
                }

                if (src_i == 8)
                {
                    /*src_i==8*/

                    for (j = 0; j < 8; j++) /*加密前异或前8个char的密文(iv_crypt指向的)*/
                        src_buf[j] ^= iv_crypt[j];

                    /*pOutBuffer、pInBuffer均为8char, pKey为16char*/
                    /*加密*/
                    TeaEncryptECB(encTempBuffer, src_buf, 0, pKey, pOutBuf, outBufPos);

                    for (j = 0; j < 8; j++) /*加密后异或前8个char的明文(iv_plain指向的)*/
                        pOutBuf[j + outBufPos] ^= iv_plain[j];

                    /*保存当前的iv_plain*/
                    for (j = 0; j < 8; j++)
                        iv_plain[j] = src_buf[j];

                    /*更新iv_crypt*/
                    src_i = 0;
                    System.Buffer.BlockCopy(pOutBuf, outBufPos, iv_crypt, 0, 8);
                    pOutBufLen += 8;
                    outBufPos += 8;
                }
            }


            int inBufPos = 0;

            /*src_i指向src_buf下一个位置*/

            while (nInBufLen > 0)
            {
                if (src_i < 8)
                {
                    src_buf[src_i++] = pInBuf[inBufPos++];
                    nInBufLen--;
                }

                if (src_i == 8)
                {
                    /*src_i==8*/

                    for (j = 0; j < 8; j++) /*加密前异或前8个char的密文(iv_crypt指向的)*/
                        src_buf[j] ^= iv_crypt[j];

                    /*pOutBuffer、pInBuffer均为8char, pKey为16char*/
                    TeaEncryptECB(encTempBuffer, src_buf, 0, pKey, pOutBuf, outBufPos);

                    for (j = 0; j < 8; j++) /*加密后异或前8个char的明文(iv_plain指向的)*/
                        pOutBuf[outBufPos + j] ^= iv_plain[j];

                    /*保存当前的iv_plain*/
                    for (j = 0; j < 8; j++)
                        iv_plain[j] = src_buf[j];

                    src_i = 0;
                    System.Buffer.BlockCopy(pOutBuf, outBufPos, iv_crypt, 0, 8);
                    pOutBufLen += 8;
                    outBufPos += 8;
                }
            }

            /*src_i指向src_buf下一个位置*/

            for (i = 1; i <= ZERO_LEN;)
            {
                if (src_i < 8)
                {
                    src_buf[src_i++] = 0;
                    i++; /*i inc in here*/
                }

                if (src_i == 8)
                {
                    /*src_i==8*/

                    for (j = 0; j < 8; j++) /*加密前异或前8个char的密文(iv_crypt指向的)*/
                        src_buf[j] ^= iv_crypt[j];
                    /*pOutBuffer、pInBuffer均为8char, pKey为16char*/
                    TeaEncryptECB(encTempBuffer, src_buf, 0, pKey, pOutBuf, outBufPos);

                    for (j = 0; j < 8; j++) /*加密后异或前8个char的明文(iv_plain指向的)*/
                        pOutBuf[j + outBufPos] ^= iv_plain[j];

                    /*保存当前的iv_plain*/
                    for (j = 0; j < 8; j++)
                        iv_plain[j] = src_buf[j];

                    src_i = 0;
                    System.Buffer.BlockCopy(pOutBuf, outBufPos, iv_crypt, 0, 8);
                    pOutBufLen += 8;
                    outBufPos += 8;
                }
            }
        }
        //debug only
        static string bytesToString(byte[] p)
        {
            string bstr = "";
            foreach (byte b in p)
            {
                bstr += b + " ";
            }
            return bstr;
        }

        static readonly uint DELTA = 0x9e3779b9;

        static readonly int ROUNDS = 16;
        static readonly int LOG_ROUNDS = 4;


        /*pOutBuffer、pInBuffer均为8char, pKey为16char*/
        static void TeaEncryptECB(TeaEncTempBuffer encTempBuffer, byte[] pInBuf, int inBufPos, byte[] pKey, byte[] pOutBuf, int outBufIndex)
        {
            uint y, z;
            uint sum;
            uint[] k = encTempBuffer.enc_k;
            int i;

            y = (uint)System.BitConverter.ToInt32(pInBuf, 0);
            z = (uint)System.BitConverter.ToInt32(pInBuf, 4);

            for (i = 0; i < 4; i++)
            {
                k[i] = (uint)System.BitConverter.ToInt32(pKey, i * 4);
            }

            sum = 0;
            for (i = 0; i < ROUNDS; i++)
            {
                sum += DELTA;
                y += ((z << 4) + k[0]) ^ (z + sum) ^ ((z >> 5) + k[1]);
                z += ((y << 4) + k[2]) ^ (y + sum) ^ ((y >> 5) + k[3]);
            }

            IntToBytes((int)y, pOutBuf, outBufIndex);
            IntToBytes((int)z, pOutBuf, outBufIndex + 4);
        }
        static void TeaDecryptECB(TeaDecTempBuffer decTempBuffer, byte[] pInBuf, int inBufIndex, byte[] pKey, byte[] pOutBuf, int outBufIndex)
        {
            uint y, z, sum;
            uint[] k = decTempBuffer.dec_k;
            int i;

            y = (uint)System.BitConverter.ToInt32(pInBuf, 0);
            z = (uint)System.BitConverter.ToInt32(pInBuf, 4);

            for (i = 0; i < 4; i++)
            {
                k[i] = (uint)System.BitConverter.ToInt32(pKey, i * 4);
            }
            sum = DELTA << LOG_ROUNDS;
            for (i = 0; i < ROUNDS; i++)
            {
                z -= ((y << 4) + k[2]) ^ (y + sum) ^ ((y >> 5) + k[3]);
                y -= ((z << 4) + k[0]) ^ (z + sum) ^ ((z >> 5) + k[1]);
                sum -= DELTA;
            }
            IntToBytes((int)y, pOutBuf, outBufIndex);
            IntToBytes((int)z, pOutBuf, outBufIndex + 4);
        }
        //little endian
        static unsafe void IntToBytes(int value, byte[] array, int offset)
        {
            fixed (byte* ptr = &array[offset])
                *(int*)ptr = value;
        }
    }
}
