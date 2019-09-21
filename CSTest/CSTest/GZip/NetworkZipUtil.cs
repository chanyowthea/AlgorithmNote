using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using System.IO;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;

namespace GCommon
{
    static class NetworkZipUtil
    {
		//public static long Compress(GZipInputStream gzipStream, byte[] data, int dataLen, byte[] secretKey, byte[] outputData, byte[] tmpBuffer)
  //      {
  //          if (outStream == null)
  //          {
  //              return -1;
  //          }
            
  //          gzipStream.Reset(new MemoryStream(data, 0, dataLen));
  //          using (GZipOutputStream gzipOutput = new GZipOutputStream(new MemoryStream(data), data.Length))
  //          {
  //              int bytesRead = gzipOutput.Read(data, 0, data.Length);
  //              StreamUtils.Copy(gzipStream, gzipOutput, data);
  //              ZipXORHeader(data, dataLen, secretKey);
  //              return gzipOutput.Position;
  //          }
  //      }

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

        static public long Unzip(GZipInputStream gzipStream, byte[] data, int dataLen, byte[] secretKey, byte[] outputData, byte[] tmpBuffer)
        {
            if (gzipStream == null)
            {
                return -1;
            }
            ZipXORHeader(data, dataLen, secretKey);
            gzipStream.Reset(new MemoryStream(data, 0, dataLen));

            using (MemoryStream outMs = new MemoryStream(outputData))
            {
                StreamUtils.CopyFromGzipInputStream(gzipStream, outMs, tmpBuffer, true);
                return outMs.Position;
            }

        }
    }
}
