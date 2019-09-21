using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.GZip;

namespace SharpZipLibUse
{
    class Class1
    {
        public static void Start()
        {
            //压缩文件

            //得到一个压缩文件,流
            FileStream zipFile = new FileStream("Demo.zip", FileMode.Create);

            //创建一个压缩流,写入压缩流中的内容，自动被压缩
            ZipOutputStream zos = new ZipOutputStream(zipFile);


            //当前目录
            DirectoryInfo di = new DirectoryInfo(".");

            FileInfo[] files = di.GetFiles("*.txt");

            byte[] buffer = new byte[10 * 1024];

            foreach (FileInfo fi in files)
            {
                //第一步，写入压缩的说明
                ZipEntry entry = new ZipEntry(fi.Name);

                entry.Size = fi.Length;

                //保存
                zos.PutNextEntry(entry);

                //第二步，写入压缩的文件内容

                int length = 0;

                Stream input = fi.Open(FileMode.Open);
                while ((length = input.Read(buffer, 0, 10 * 1024)) > 0)
                {
                    Console.WriteLine("length=" + length);
                    zos.Write(buffer, 0, length);
                }

                input.Close();
            }
            zos.Finish();
            zos.Close();

            Console.WriteLine("Ok!");
            Console.Read();
        }
    }
}

namespace Compression
{
    /**//// <summary>
    /// 压缩方式。
    /// </summary>
    public enum CompressionType
    {
        /**//// <summary>
        /// GZip 压缩格式
        /// </summary>
        GZip,
        /**//// <summary>
        /// BZip2 压缩格式
        /// </summary>
        BZip2,
        /**//// <summary>
        /// Zip 压缩格式
        /// </summary>
        Zip
    }

    /**//// <summary>
    /// 使用 SharpZipLib 进行压缩的辅助类，简化对字节数组和字符串进行压缩的操作。
    /// </summary>
    public class CompressionHelper
    {
        static public void ZipXORHeader(byte[] data, int dataLen, byte[] secretKey)
        {
            int keyLength = secretKey.Length;
            if (dataLen < keyLength)
            {
                keyLength = dataLen;
            }
            for (int i = 0; i < keyLength; ++i)
            {
                data[i] ^= secretKey[i];
            }
        }

        /**//// <summary>
        /// 压缩供应者，默认为 GZip。
        /// </summary>
        public static CompressionType CompressionProvider = CompressionType.GZip;

        //Public methods
        #region Public methods

        /**//// <summary>
            /// 从原始字节数组生成已压缩的字节数组。
            /// </summary>
            /// <param name="bytesToCompress">原始字节数组。</param>
            /// <returns>返回已压缩的字节数组</returns>
        public static byte[] Compress(byte[] bytesToCompress)
        {
            MemoryStream ms = new MemoryStream();
            Stream s = OutputStream(ms);

            s.Write(bytesToCompress, 0, bytesToCompress.Length);
            s.Close();

            var bs = ms.ToArray();
            ZipXORHeader(bs, bs.Length, GCommon.NetworkCryptologyUtil.TestKey);
            return bs;
        }

        /**//// <summary>
        /// 从原始字符串生成已压缩的字符串。
        /// </summary>
        /// <param name="stringToCompress">原始字符串。</param>
        /// <returns>返回已压缩的字符串。</returns>
        public static string Compress(string stringToCompress)
        {
            byte[] compressedData = CompressToByte(stringToCompress);
            string strOut = Convert.ToBase64String(compressedData);
            return strOut;
        }

        /**//// <summary>
        /// 从原始字符串生成已压缩的字节数组。
        /// </summary>
        /// <param name="stringToCompress">原始字符串。</param>
        /// <returns>返回已压缩的字节数组。</returns>
        public static byte[] CompressToByte(string stringToCompress)
        {
            byte[] bytData = Encoding.Unicode.GetBytes(stringToCompress);
            return Compress(bytData);
        }

        /**//// <summary>
        /// 从已压缩的字符串生成原始字符串。
        /// </summary>
        /// <param name="stringToDecompress">已压缩的字符串。</param>
        /// <returns>返回原始字符串。</returns>
        public string DeCompress(string stringToDecompress)
        {
            string outString = string.Empty;
            if (stringToDecompress == null)
            {
                throw new ArgumentNullException("stringToDecompress", "You tried to use an empty string");
            }

            try
            {
                byte[] inArr = Convert.FromBase64String(stringToDecompress.Trim());
                outString = Encoding.Unicode.GetString(DeCompress(inArr));
            }
            catch (NullReferenceException nEx)
            {
                return nEx.Message;
            }

            return outString;
        }

        /**//// <summary>
        /// 从已压缩的字节数组生成原始字节数组。
        /// </summary>
        /// <param name="bytesToDecompress">已压缩的字节数组。</param>
        /// <returns>返回原始字节数组。</returns>
        public static byte[] DeCompress(byte[] bytesToDecompress)
        {
            ZipXORHeader(bytesToDecompress, bytesToDecompress.Length, GCommon.NetworkCryptologyUtil.TestKey);

            byte[] writeData = new byte[4096];
            Stream s2 = InputStream(new MemoryStream(bytesToDecompress));
            MemoryStream outStream = new MemoryStream();

            while (true)
            {
                int size = s2.Read(writeData, 0, writeData.Length);
                if (size > 0)
                {
                    outStream.Write(writeData, 0, size);
                }
                else
                {
                    break;
                }
            }
            s2.Close();
            byte[] outArr = outStream.ToArray();

            outStream.Close();
            return outArr;
        }

        #endregion

        //Private methods
        #region Private methods

        /**//// <summary>
            /// 从给定的流生成压缩输出流。
            /// </summary>
            /// <param name="inputStream">原始流。</param>
            /// <returns>返回压缩输出流。</returns>
        private static Stream OutputStream(Stream inputStream)
        {
            switch (CompressionProvider)
            {
                case CompressionType.GZip:
                    return new GZipOutputStream(inputStream);

                case CompressionType.Zip:
                    return new ZipOutputStream(inputStream);

                default:
                    return new GZipOutputStream(inputStream);
            }
        }

        /**//// <summary>
        /// 从给定的流生成压缩输入流。
        /// </summary>
        /// <param name="inputStream">原始流。</param>
        /// <returns>返回压缩输入流。</returns>
        private static Stream InputStream(Stream inputStream)
        {
            switch (CompressionProvider)
            {
                case CompressionType.GZip:
                    return new GZipInputStream(inputStream);

                case CompressionType.Zip:
                    return new ZipInputStream(inputStream);

                default:
                    return new GZipInputStream(inputStream);
            }
        }

        #endregion
    }
}