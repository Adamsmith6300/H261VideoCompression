using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assignment2
{
    class ShrunkFileIO
    {
        public InterFrame ioFrame { get; set; }
        public InterFrame iaFrame { get; set; }
        public InterFrame ieFrame { get; set; }
        public byte[] frameAsBytes { get; set; }
        public byte[] iaFrameBytes { get; set; }
        public byte[] ieFrameBytes { get; set; }
        public byte[] mvYBytes { get; set; }
        public byte[] mvCbBytes { get; set; }
        public byte[] mvCrBytes { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public double compressionRatio { get; set; }

        public ShrunkFileIO(InterFrame _iaFrame, InterFrame _ieFrame)
        {
            iaFrame = _iaFrame;
            ieFrame = _ieFrame;
            width = _ieFrame.width;
            height = _ieFrame.height;
        }
        public ShrunkFileIO(InterFrame f)
        {
            ioFrame = f;
            frameAsBytes = toUBytes(f.encoded);
            width = f.width;
            height = f.height;
        }
        public ShrunkFileIO(string fileName)
        {
            bool read = readFile(fileName);
            if (read)
            {
                sbyte[] iaFrameSignedBytes = Array.ConvertAll(iaFrameBytes, b => (sbyte)b);
                sbyte[] ieFrameSignedBytes = Array.ConvertAll(ieFrameBytes, b => (sbyte)b);
                iaFrame = new InterFrame(iaFrameSignedBytes, width, height);
                ieFrame = new InterFrame(iaFrame, ieFrameSignedBytes, width, height, mvYBytes, mvCbBytes);
                iaFrame.upsampleAndRGB();
            }
            
        }

        public byte[] toUBytes(SByte[] values)
        {
            return  (byte[])values.Cast<byte>();
        }

        public bool readFile(string fileName)
        {
            string path = @"" + fileName;

            try
            {
                byte[] fileData = File.ReadAllBytes(path);

                byte[] heightBytes = new byte[4];
                byte[] widthBytes = new byte[4];
                byte[] mvYBytesLength = new byte[4];
                byte[] ieBytesLength = new byte[4];
                byte[] iaBytesLength = new byte[4];
                Buffer.BlockCopy(fileData, fileData.Length - 4, heightBytes, 0, 4);
                Buffer.BlockCopy(fileData, fileData.Length - 8, widthBytes, 0, 4);
                Buffer.BlockCopy(fileData, fileData.Length - 12, mvYBytesLength, 0, 4);
                Buffer.BlockCopy(fileData, fileData.Length - 16, ieBytesLength, 0, 4);
                Buffer.BlockCopy(fileData, fileData.Length - 20, iaBytesLength, 0, 4);

                height = BitConverter.ToInt32(heightBytes, 0);
                width = BitConverter.ToInt32(widthBytes, 0);
                int mvYLength = BitConverter.ToInt32(mvYBytesLength, 0);
                int ieLength = BitConverter.ToInt32(ieBytesLength, 0);
                int iaLength = BitConverter.ToInt32(iaBytesLength, 0);

                iaFrameBytes = new byte[iaLength];
                ieFrameBytes = new byte[ieLength];
                mvYBytes = new byte[mvYLength];
                mvCbBytes = new byte[mvYLength/4];
                //mvCrBytes = new byte[mvYLength/4];
                int offset = 0;
                Buffer.BlockCopy(fileData, offset, iaFrameBytes, 0, iaFrameBytes.Length);
                offset += iaFrameBytes.Length;
                Buffer.BlockCopy(fileData, offset, ieFrameBytes, 0, ieFrameBytes.Length);
                offset += ieFrameBytes.Length;
                Buffer.BlockCopy(fileData, offset, mvYBytes, 0, mvYBytes.Length);
                offset += mvYBytes.Length;
                Buffer.BlockCopy(fileData, offset, mvCbBytes, 0, mvCbBytes.Length);
                //offset += mvCbBytes.Length;
                //Buffer.BlockCopy(fileData, offset, mvCrBytes, 0, mvCrBytes.Length);

                //Debug.WriteLine("rwidth" + width);
                //Debug.WriteLine("rheight" + height);
                //Debug.WriteLine("iaLength" + iaLength);
                //Debug.WriteLine("ieLength" + ieLength);
                //Debug.WriteLine("mvY" + mvYLength);
                //Debug.WriteLine("mvCb/Cr" + mvYLength/4);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception caught in process: {0}", ex);
                return false;
            }
        }
        public bool writeFile(string fileName)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    iaFrameBytes = toUBytes(iaFrame.encoded);
                    ieFrameBytes = toUBytes(ieFrame.encoded);

                    fs.Write(iaFrameBytes, 0, iaFrameBytes.Length);
                    fs.Write(ieFrameBytes, 0, ieFrameBytes.Length);

                    fs.Write(ieFrame.mvYBytes, 0, ieFrame.mvYBytes.Length);
                    fs.Write(ieFrame.mvCbBytes, 0, ieFrame.mvCbBytes.Length);
                    //fs.Write(ieFrame.mvCrBytes, 0, ieFrame.mvCrBytes.Length);

                    byte[] iaBytesLength = BitConverter.GetBytes(iaFrameBytes.Length);
                    byte[] ieBytesLength = BitConverter.GetBytes(ieFrameBytes.Length);
                    byte[] mvYBytesLength = BitConverter.GetBytes(ieFrame.mvYBytes.Length);
                    byte[] widthBytes = BitConverter.GetBytes(width);
                    byte[] heightBytes = BitConverter.GetBytes(height);
                    //Debug.WriteLine("width" + width);
                    //Debug.WriteLine("height" + height);
                    //Debug.WriteLine("iaLength" + iaFrameBytes.Length);
                    //Debug.WriteLine("ieLength" + ieFrameBytes.Length);
                    //Debug.WriteLine("mvY" + ieFrame.mvYBytes.Length);
                    //Debug.WriteLine("mvCb/Cr" + ieFrame.mvYBytes.Length / 4);
                    fs.Write(iaBytesLength, 0, 4);
                    fs.Write(ieBytesLength, 0, 4);
                    fs.Write(mvYBytesLength, 0, 4);
                    fs.Write(widthBytes, 0, 4);
                    fs.Write(heightBytes, 0, 4);

                    double originalSize = (height * width * 3) * 2;
                    double compressedSize = iaFrameBytes.Length
                        + ieFrameBytes.Length
                        + ieFrame.mvYBytes.Length
                        + ieFrame.mvCbBytes.Length;
                        //+ ieFrame.mvCrBytes.Length;
                    compressionRatio = originalSize / compressedSize;
                    Debug.WriteLine("Original: "+ originalSize);
                    Debug.WriteLine("Compressed: "+ compressedSize);
                    Debug.WriteLine("Ratio: " + compressionRatio);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception caught in process: {0}", ex);
                return false;
            }
        }
    }
}
