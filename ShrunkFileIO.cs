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
        public byte[] frameAsBytes { get; set; }
        public int width { get; set; }
        public int height { get; set; }

        public ShrunkFileIO(InterFrame f)
        {
            ioFrame = f;
            frameAsBytes = GetBytes(f.encoded);
            width = f.width;
            height = f.height;
        }
        public ShrunkFileIO(string fileName)
        {
            bool read = readFile(fileName);
            if (read)
            {
                sbyte[] sbytes = Array.ConvertAll(frameAsBytes, b => (sbyte)b);
                ioFrame = new InterFrame(sbytes, width, height);
                //for (int i = 0; i < sbytes.Length; i++)
                //{
                //    Debug.Write(sbytes[i] + ",");
                //}
            }
            
        }
        //public ShrunkFileIO()
        //{
        //    sbyte[] my_sByteArray = { -2, -1, 0, 1, 2, -2, -1, 0, 1, 2, -2, -1, 0, 1, 2, -2, -1, 0, 1, 2, -2, -1, 0, 1, 2 };
        //    frameAsBytes = GetBytes(my_sByteArray);
        //    bool write = writeFile("dog.shrunk");
        //    bool read = readFile("dog.shrunk");
        //    sbyte[] sbytes = Array.ConvertAll(frameAsBytes, b => (sbyte)b);
        //    for(int i = 0; i < sbytes.Length; i++)
        //    {
        //        Debug.Write(sbytes[i]+",");
        //    }
        //}


        public byte[] GetBytes(SByte[] values)
        {
            return  (byte[])values.Cast<byte>();
        }

        public bool readFile(string fileName)
        {
            string path = @"" + fileName;

            try
            {
                byte[] fileData = File.ReadAllBytes(path);
                frameAsBytes = new byte[fileData.Length - 8];
                byte[] widthBytes = new byte[4];
                byte[] heightBytes = new byte[4];
                Buffer.BlockCopy(fileData, 0, frameAsBytes, 0, frameAsBytes.Length);
                Buffer.BlockCopy(fileData, frameAsBytes.Length, widthBytes, 0, 4);
                Buffer.BlockCopy(fileData, frameAsBytes.Length+4, heightBytes, 0, 4);
                width = BitConverter.ToInt32(widthBytes, 0);
                height = BitConverter.ToInt32(heightBytes, 0);
                Debug.Write("rwidth" + width);
                Debug.Write("rheight" + height);
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
                string path = @"C:\Users\Adam\Documents\code\bcit\term4\COMP4932-DigiPro\Assignment2\" + fileName;
                using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(frameAsBytes, 0, frameAsBytes.Length);
                    byte[] widthBytes = BitConverter.GetBytes(width);
                    byte[] heightBytes = BitConverter.GetBytes(height);
                    //if (BitConverter.IsLittleEndian)
                    //{
                    //    Array.Reverse(widthBytes);
                    //    Array.Reverse(heightBytes);
                    //}
                    fs.Write(widthBytes, 0, 4);
                    fs.Write(heightBytes, 0, 4);
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
