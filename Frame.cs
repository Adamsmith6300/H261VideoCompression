using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assignment2
{
    class Frame
    {
        public Bitmap ogBmp { get; set; }
        public Bitmap cbBmp { get; set; }
        public Bitmap crBmp { get; set; }
        public List<List<double>> yDoubles { get; set; }
        public List<List<double>> cbDoubles { get; set; }
        public List<List<double>> crDoubles { get; set; }
        public int[,] lumQTable { get; set; }
        public int[,] chromQTable { get; set; }
        public List<List<List<double>>> yBlocks { get; set; }
        public List<List<List<double>>> cbBlocks { get; set; }
        public List<List<List<double>>> crBlocks { get; set; }

        public Frame(Bitmap bmp)
        {
            this.ogBmp = bmp;
            this.cbBmp = new Bitmap(bmp.Width,bmp.Height);
            this.crBmp = new Bitmap(bmp.Width,bmp.Height);
            this.yDoubles = new List<List<double>>();
            this.cbDoubles = new List<List<double>>();
            this.crDoubles = new List<List<double>>();
            lumQTable = new int[,] {
                { 16, 11,  10, 16, 24, 40, 51, 61},
                { 12, 12,  14, 19, 26, 58, 60, 55},
                { 14, 13,  16, 24, 40, 57, 69, 56},
                { 14, 17,  22, 29, 51, 87, 80, 62},
                { 18, 22,  37, 56, 68, 109, 103, 77},
                { 24, 35,  55, 64, 81, 104, 113, 92},
                { 49, 64,  78, 87, 103, 121, 120, 101},
                { 72, 92,  95, 98, 112, 100, 103, 99}
            };
            chromQTable = new int[,] {
                { 17, 18, 24, 47, 99, 99, 99, 99},
                { 18, 21, 26, 66, 99, 99, 99, 99},
                { 24, 26, 56, 99, 99, 99, 99, 99},
                { 47, 66, 99, 99, 99, 99, 99, 99},
                { 99, 99, 99, 99, 99, 99, 99, 99},
                { 99, 99, 99, 99, 99, 99, 99, 99},
                { 99, 99, 99, 99, 99, 99, 99, 99},
                { 99, 99, 99, 99, 99, 99, 99, 99}
            };
        }

        public void convertToYCbCr()
        {
            for (int row = 0; row < this.ogBmp.Width; row++)
            {
                this.yDoubles.Add(new List<double>());
                this.cbDoubles.Add(new List<double>());
                this.crDoubles.Add(new List<double>());
                for (int col = 0; col < this.ogBmp.Height; col++)
                {
                    calcYCbCrValue(row, col);
                }
            }
        }
        private void calcYCbCrValue(int row, int col)
        {
            Color pixelColor = this.ogBmp.GetPixel(row, col);
            byte r = pixelColor.R;
            byte g = pixelColor.G;
            byte b = pixelColor.B;
            double y = calcY(r, g, b);
            double cb = calcCb(r, g, b);
            double cr = calcCr(r, g, b);
            this.yDoubles[row].Add(y);
            this.cbDoubles[row].Add(cb);
            this.crDoubles[row].Add(cr);
        }

        public void setCbCrBmp() {
            this.cbBmp = new Bitmap(cbDoubles.Count, cbDoubles[0].Count);
            this.crBmp = new Bitmap(crDoubles.Count, crDoubles[0].Count);
            for (int row = 0; row < cbDoubles.Count; row++)
            {
                for(int col = 0; col < cbDoubles[row].Count; col++)
                {
                    this.cbBmp.SetPixel(row, col,
                    Color.FromArgb(
                       255,
                       (byte)Math.Max(0, Math.Min(255, cbDoubles[row][col])),
                       (byte)Math.Max(0, Math.Min(255, cbDoubles[row][col])),
                       (byte)Math.Max(0, Math.Min(255, cbDoubles[row][col]))
                      )
                    );
                    this.crBmp.SetPixel(row, col,
                        Color.FromArgb(
                            255,
                            (byte)Math.Max(0, Math.Min(255, crDoubles[row][col])),
                            (byte)Math.Max(0, Math.Min(255, crDoubles[row][col])),
                            (byte)Math.Max(0, Math.Min(255, crDoubles[row][col]))
                        )
                    );
                }
            }
            
        }

        private double calcY(byte r, byte g, byte b)
        {
            return ((0.299 * r) + (0.587 * g) + (0.114 * b));
        }
        private double calcCb(byte r, byte g, byte b)
        {

            return 128 + ((-0.169 * r) + (-0.331 * g) + (0.5 * b));
        }
        private double calcCr(byte r, byte g, byte b)
        {
            return 128 + ((0.5 * r) + (-0.419 * g) + (-0.081 * b));
        }
        

        public void convertToRGB() 
        {
            for (int row = 0; row < this.ogBmp.Width; row++)
            {
                for (int col = 0; col < this.ogBmp.Height; col++)
                {
                    calcRGBValue(row, col);
                }
            }
        }
       
        private void calcRGBValue(int row, int col)
        {
            double Y = (double)this.yDoubles[row][col];
            double Cr = (double)this.crDoubles[row][col];
            double Cb = (double)this.cbDoubles[row][col];
            double R = calcR(Y, Cr, Cb);
            double G = calcG(Y, Cr, Cb);
            double B = calcB(Y, Cr, Cb);

            Color newColor = Color.FromArgb(
                255,
                (byte)Math.Max(0, Math.Min(255, R)),
                (byte)Math.Max(0, Math.Min(255, G)),
                (byte)Math.Max(0, Math.Min(255, B))
            );
            this.ogBmp.SetPixel(row, col, newColor);
        }

        private double calcR(double Y, double Cr, double Cb)
        {
            return (Y + (1.402 * (Cr - 128)));
        }
        private double calcG(double Y, double Cr, double Cb)
        {
            return (Y - (0.344136 * (Cb - 128)) - (0.714136 * (Cr - 128)));
        }
        private double calcB(double Y, double Cr, double Cb)
        {
            return (Y + (1.772 * (Cb - 128)));
        }

        public void subsampleChrominance()
        {
            List<List<double>> newCb = new List<List<double>>();
            List<List<double>> newCr = new List<List<double>>();
            for(int row = 0; row < cbDoubles.Count; row++)
            {
                if((row + 1) % 2 != 0)
                {
                    newCb.Add(new List<double>());
                    newCr.Add(new List<double>());
                    for (int col = 0; col < cbDoubles[row].Count; col++)
                    {
                        if ((col + 1) % 2 != 0)
                        {
                            newCb[row / 2].Add(this.cbDoubles[row][col]);
                            newCr[row / 2].Add(this.crDoubles[row][col]);
                        }
                    }
                }
            }
            this.cbDoubles = newCb;
            this.crDoubles = newCr;
        }

        public List<List<List<double>>> formBlocks(List<List<double>> oldMatrix, int size)
        {
            int numBlocksAcross = (int)Math.Ceiling((double)oldMatrix[0].Count / size);
            int numBlocksDown = (int)Math.Ceiling((double)oldMatrix.Count / size);
            int totalBlocks = numBlocksAcross * numBlocksDown;
            List<List<List<double>>> blocks = new List<List<List<double>>>();
            for(int blockInd = 0; blockInd < totalBlocks; blockInd++)
            {
                List<List<double>> sizeBySizeBlock = new List<List<double>>();
                for(int i = 0; i < size; i++)
                {
                    sizeBySizeBlock.Add(new List<double>(new double[size]));
                }
                blocks.Add(sizeBySizeBlock);
            }

            for(int row = 0; row < oldMatrix.Count; row++)
            {
                for(int col = 0; col < oldMatrix[row].Count; col++)
                {
                    double val = oldMatrix[row][col];
                    int blockNum = ((row / size) * (numBlocksAcross)) + (col / size);
                    int blockRowIndex = row % size;
                    int blockColIndex = col % size;
                    blocks[blockNum][blockRowIndex][blockColIndex] = val;
                }
            }

            return blocks;
        }

        public void buildYBlocks()
        {
            this.yBlocks = formBlocks(this.yDoubles, 8);
        }
        public void buildCbBlocks()
        {
            this.cbBlocks = formBlocks(this.cbDoubles, 8);
        }
        public void buildCrBlocks()
        {
            this.crBlocks = formBlocks(this.crDoubles, 8);
        }

        public void printBlocks(List<List<List<double>>> blocks)
        {
            foreach (var sublist in blocks)
            {
                Console.WriteLine("New Block");
                foreach (var obj in sublist)
                {
                    Console.Write("{");
                    foreach (var num in obj)
                    {
                        Console.Write(num + ",");
                    }
                    Console.WriteLine("}");
                }
            }
        }

        public List<List<double>> dct(List<List<double>> F)
        {
            List<List<double>> newF = new List<List<double>>();
            int M = F.Count;
            int N = M;
            for(int u = 0; u < F.Count; u++)
            {
                newF.Add(new List<double>());
                for(int v = 0; v < F[u].Count; v++)
                {
                    double uvVal = 0;
                    for(int i = 0; i < N; i++)
                    {
                        for(int j = 0; j < M; j++)
                        {
                            double lamdaI = i == 0 ? (1 / Math.Sqrt(2)) : 1;
                            double lamdaJ = j == 0 ? (1 / Math.Sqrt(2)) : 1;
                            double cosI = cosCalc(u, i, N);
                            double cosJ = cosCalc(v, j, M);
                            double f = F[i][j];
                            uvVal += (lamdaI * lamdaJ * cosI * cosJ * f);
                        }
                    }
                    uvVal = uvVal * (Math.Pow(2 / N, 1 / 2)) * (Math.Pow(2 / M, 1 / 2));
                    newF[u].Add(uvVal);
                }
            }
            return newF;
        }

        public double cosCalc(int u, int i, int N)
        {
            double val = (Math.PI * u)/(2*N);
            val = val * (2 * i + 1);
            return Math.Cos(val);
        }

        public void printDctValues(List<List<double>> F)
        {
                Console.WriteLine("New Block");
                foreach (var obj in F)
                {
                    Console.Write("{");
                    foreach (var num in obj)
                    {
                        Console.Write(num + ",");
                    }
                    Console.WriteLine("}");
                }
        }

    }
}
