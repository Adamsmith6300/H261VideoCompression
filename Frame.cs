using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        public int width { get; set; }
        public int height { get; set; }
        public List<List<double>> yDoubles { get; set; }
        public List<List<double>> cbDoubles { get; set; }
        public List<List<double>> crDoubles { get; set; }
        public List<List<List<double>>> yBlocks { get; set; }
        public List<List<List<double>>> cbBlocks { get; set; }
        public List<List<List<double>>> crBlocks { get; set; }
        public List<sbyte> yCoded { get; set; }
        public List<sbyte> cbCoded { get; set; }
        public List<sbyte> crCoded { get; set; }
        public sbyte[] encoded { get; set; }

        public Frame(Bitmap bmp)
        {
            this.ogBmp = bmp;
            this.width = bmp.Width;
            this.height = bmp.Height;
            this.cbBmp = new Bitmap(bmp.Width,bmp.Height);
            this.crBmp = new Bitmap(bmp.Width,bmp.Height);
            this.yDoubles = new List<List<double>>();
            this.cbDoubles = new List<List<double>>();
            this.crDoubles = new List<List<double>>();
        }

        public Frame(sbyte[] sbytes, int w, int h){
            encoded = sbytes;
            width = w;
            height = h;
            decompressFrame();
        }
        public Frame() { }

        public void decompressFrame()
        {
            List<sbyte> decoded = runLengthDecode(encoded);
            int yCount = (decoded.Count * 2)/ 3;
            int chromCount = yCount / 4;
            this.ogBmp = new Bitmap(width, height);
            yCoded = decoded.GetRange(0, yCount);
            cbCoded = decoded.GetRange(yCount, chromCount);
            crCoded = decoded.GetRange(yCount + chromCount, chromCount);
            yBlocks = reverseZigZagAndBuildBlocks(yCoded);
            cbBlocks = reverseZigZagAndBuildBlocks(cbCoded);
            crBlocks = reverseZigZagAndBuildBlocks(crCoded);
            yBlocks = reverseQuantizeAllBlocks(yBlocks, lumQTable);
            cbBlocks = reverseQuantizeAllBlocks(cbBlocks, chromQTable);
            crBlocks = reverseQuantizeAllBlocks(crBlocks, chromQTable);
            yBlocks = reverseDctAllBlocks(yBlocks);
            cbBlocks = reverseDctAllBlocks(cbBlocks);
            crBlocks = reverseDctAllBlocks(crBlocks);
            yDoubles = getPixelsFromBlocks(yBlocks, width, height);
            cbDoubles = getPixelsFromBlocks(cbBlocks, width / 2, height / 2);
            crDoubles = getPixelsFromBlocks(crBlocks, width / 2, height / 2);
        }
        public void upsampleAndRGB()
        {
            upsampleChrominance();
            convertToRGB();
        }
        public void subSampleAndYCbCr()
        {
            this.yDoubles = new List<List<double>>();
            this.cbDoubles = new List<List<double>>();
            this.crDoubles = new List<List<double>>();
            convertToYCbCr();
            subsampleChrominance();
        }
        public void compressFrame()
        {
            
            buildYBlocks();
            buildCbBlocks();
            buildCrBlocks();
            yBlocks = dctAllBlocks(yBlocks);
            cbBlocks = dctAllBlocks(cbBlocks);
            crBlocks = dctAllBlocks(crBlocks);
            yBlocks = quantizeAllBlocks(yBlocks, lumQTable);
            cbBlocks = quantizeAllBlocks(cbBlocks, chromQTable);
            crBlocks = quantizeAllBlocks(crBlocks, chromQTable);
            yCoded = zigZagAllBlocks(yBlocks);
            cbCoded = zigZagAllBlocks(cbBlocks);
            crCoded = zigZagAllBlocks(crBlocks);
            yCoded = runLengthEncode(yCoded);
            cbCoded = runLengthEncode(cbCoded);
            crCoded = runLengthEncode(crCoded);
            List<sbyte> YCbCr = new List<sbyte>();
            YCbCr.AddRange(yCoded);
            YCbCr.AddRange(cbCoded);
            YCbCr.AddRange(crCoded);
            encoded = YCbCr.ToArray();
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

        public void upsampleChrominance()
        {
            List<List<double>> newCb = new List<List<double>>();
            List<List<double>> newCr = new List<List<double>>();
            for (int row = 0; row < height; row++)
            {
                if ((row + 1) % 2 != 0)
                {
                    newCb.Add(new List<double>());
                    newCr.Add(new List<double>());
                    for (int col = 0; col < width; col++)
                    {
                        if ((col + 1) % 2 != 0)
                        {
                            newCb[row].Add(this.cbDoubles[row/2][col/2]);
                            newCr[row].Add(this.crDoubles[row/2][col/2]);
                        }
                        else
                        {
                            newCb[row].Add(newCb[row][col - 1]);
                            newCr[row].Add(newCr[row][col - 1]);
                        }
                    }
                } else
                {
                    newCb.Add(newCb[row - 1]);
                    newCr.Add(newCr[row - 1]);
                }
            }
            this.cbDoubles = newCb;
            this.crDoubles = newCr;
        }

        public List<List<double>> getPixelsFromBlocks(List<List<List<double>>> blocks, int width, int height)
        {
            List<List<double>> pixels = new List<List<double>>();
            int numBlocksAcross = (int)Math.Ceiling((double)width / 8);
            for(int row = 0; row < height; row++)
            {
                pixels.Add(new List<double>());
                for (int col = 0; col < width; col++)
                {
                    int blockNum = ((row / 8) * (numBlocksAcross)) + (col / 8);
                    int blockRowIndex = row % 8;
                    int blockColIndex = col % 8;
                    double val = blocks[blockNum][blockRowIndex][blockColIndex];
                    pixels[row].Add(val);
                }
            }

            return pixels;
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

        public List<List<List<double>>> dctAllBlocks(List<List<List<double>>> blocks)
        {
            for(int i = 0; i < blocks.Count; i++){
                blocks[i] = dct(blocks[i]);
            }
            return blocks;
        }
        public List<List<List<double>>> reverseDctAllBlocks(List<List<List<double>>> blocks)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                blocks[i] = inverseDct(blocks[i]);
            }
            return blocks;
        }

        public List<List<double>> dct(List<List<double>> F)
        {
            List<List<double>> newF = new List<List<double>>();
            int M = 8;
            int N = M;
            for(int u = 0; u < F.Count; u++)
            {
                newF.Add(new List<double>());
                for(int v = 0; v < F[u].Count; v++)
                {
                    double uvVal = 0;
                    double lambdaU = u == 0 ? (1 / Math.Sqrt(2)) : 1;
                    double lambdaV = v == 0 ? (1 / Math.Sqrt(2)) : 1;
                    for (int i = 0; i < N; i++)
                    {
                        for(int j = 0; j < M; j++)
                        {
                            double cosI = cosCalc(u, i, N);
                            double cosJ = cosCalc(v, j, M);
                            double f = F[i][j];
                            uvVal +=  (cosI * cosJ * f);
                        }
                    }
                    uvVal = ((2 * lambdaU * lambdaV) / Math.Sqrt(M * N))*uvVal;
                    newF[u].Add(uvVal);
                }
            }
            return newF;
        }

        public List<List<double>> inverseDct(List<List<double>> F)
        {
            List<List<double>> newF = new List<List<double>>();
            int M = 8;
            int N = M;
            for (int i = 0; i < F.Count; i++)
            {
                newF.Add(new List<double>());
                for (int j = 0; j < F[i].Count; j++)
                {
                    double uvVal = 0;
                    for (int u = 0; u < N; u++)
                    {
                        for (int v = 0; v < M; v++)
                        {
                            double lambdaU = u == 0 ? (1 / Math.Sqrt(2)) : 1;
                            double lambdaV = v == 0 ? (1 / Math.Sqrt(2)) : 1;
                            double cosI = cosCalc(u, i, N);
                            double cosJ = cosCalc(v, j, M);
                            double f = F[u][v];
                            uvVal += ((2 * lambdaU * lambdaV) / Math.Sqrt(M * N))*(cosI * cosJ * f);
                        }
                    }
                    newF[i].Add(Math.Round(uvVal));
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

        public List<List<List<double>>> quantizeAllBlocks(List<List<List<double>>> blocks, int[,] qTable)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                blocks[i] = quantizeBlock(blocks[i], qTable);
            }
            return blocks;
        }
        public List<List<List<double>>> reverseQuantizeAllBlocks(List<List<List<double>>> blocks, int[,] qTable)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                blocks[i] = inverseQuantizeBlock(blocks[i], qTable);
            }
            return blocks;
        }

        public List<List<double>> quantizeBlock(List<List<double>> block, int[,] qTable)
        {
            for(int row = 0; row < block.Count; row++)
            {
                for(int col = 0; col < block[row].Count; col++)
                {
                    block[row][col] = Math.Round(block[row][col] / (1*qTable[row,col]));
                }
            }
            return block;
        }

        public List<List<double>> inverseQuantizeBlock(List<List<double>> block, int[,] qTable)
        {
            for (int row = 0; row < block.Count; row++)
            {
                for (int col = 0; col < block[row].Count; col++)
                {
                    block[row][col] = Math.Round(block[row][col] * (1*qTable[row, col]));
                }
            }
            return block;
        }

        public List<sbyte> zigZagAllBlocks(List<List<List<double>>> blocks)
        {
            List<sbyte> encoded = new List<sbyte>();
            for (int i = 0; i < blocks.Count; i++)
            {
                List<sbyte> temp = zigZag(blocks[i], 8, 8);
                //temp = runLengthEncode(temp);
                encoded.AddRange(temp);
            }
            return encoded;
        }


        public List<List<List<double>>> reverseZigZagAndBuildBlocks(List<sbyte> decoded)
        {
            int blockCount = decoded.Count / 64;
            List<List<List<double>>> blocks = new List<List<List<double>>>();
            for (int i = 0; i < blockCount; i++)
            {
                List<List<double>> temp = reverseZigZag(decoded.GetRange(i*64, 64), 8, 8);
                blocks.Add(temp);
            }
            return blocks;
        }

        public List<sbyte> zigZag(List<List<double>> arr, int n, int m)
        {
            List<sbyte> results = new List<sbyte>();
            int row = 0, col = 0;

            // Boolean variable that will 
            // true if we need to increment 
            // 'row' valueotherwise false- 
            // if increment 'col' value 
            bool row_inc = false;

            // Print matrix of lower half 
            // zig-zag pattern 
            int mn = Math.Min(m, n);
            for (int len = 1; len <= mn; ++len)
            {
                for (int i = 0; i < len; ++i)
                {
                    results.Add(Convert.ToSByte(Math.Max(-128,Math.Min(127,arr[row][col]))));

                    if (i + 1 == len)
                        break;

                    // If row_increment value is true 
                    // increment row and decrement col 
                    // else decrement row and increment 
                    // col 
                    if (row_inc)
                    {
                        ++row;
                        --col;
                    }
                    else
                    {
                        --row;
                        ++col;
                    }
                }

                if (len == mn)
                    break;

                // Update row or col valaue 
                // according to the last 
                // increment 
                if (row_inc)
                {
                    ++row;
                    row_inc = false;
                }
                else
                {
                    ++col;
                    row_inc = true;
                }
            }

            // Update the indexes of row 
            // and col variable 
            if (row == 0)
            {
                if (col == m - 1)
                    ++row;
                else
                    ++col;
                row_inc = true;
            }
            else
            {
                if (row == n - 1)
                    ++col;
                else
                    ++row;
                row_inc = false;
            }

            // Print the next half 
            // zig-zag pattern 
            int MAX = Math.Max(m, n) - 1;
            for (int len, diag = MAX; diag > 0; --diag)
            {

                if (diag > mn)
                    len = mn;
                else
                    len = diag;

                for (int i = 0; i < len; ++i)
                {
                    results.Add(Convert.ToSByte(Math.Max(-128,Math.Min(127,arr[row][col]))));

                    if (i + 1 == len)
                        break;

                    // Update row or col value 
                    // according to the last 
                    // increment 
                    if (row_inc)
                    {
                        ++row;
                        --col;
                    }
                    else
                    {
                        ++col;
                        --row;
                    }
                }

                // Update the indexes of 
                // row and col variable 
                if (row == 0 || col == m - 1)
                {
                    if (col == m - 1)
                        ++row;
                    else
                        ++col;

                    row_inc = true;
                }

                else if (col == 0 || row == n - 1)
                {
                    if (row == n - 1)
                        ++col;
                    else
                        ++row;

                    row_inc = false;
                }
            }
            return results;
        }

        public List<List<double>> reverseZigZag(List<sbyte> arr, int n, int m)
        {
            List<List<double>> results = new List<List<double>>();
            int counter = 0;
            int row = 0, col = 0;

            // Boolean variable that will 
            // true if we need to increment 
            // 'row' valueotherwise false- 
            // if increment 'col' value 
            bool row_inc = false;

            // Print matrix of lower half 
            // zig-zag pattern 
            int mn = Math.Min(m, n);
            for (int len = 1; len <= mn; ++len)
            {
                results.Add(new List<double>());
                for (int i = 0; i < len; ++i)
                {
                    results[row].Add(arr[counter]);
                    counter++;

                    if (i + 1 == len)
                        break;

                    // If row_increment value is true 
                    // increment row and decrement col 
                    // else decrement row and increment 
                    // col 
                    if (row_inc)
                    {
                        ++row;
                        --col;
                    }
                    else
                    {
                        --row;
                        ++col;
                    }
                }

                if (len == mn)
                    break;

                // Update row or col valaue 
                // according to the last 
                // increment 
                if (row_inc)
                {
                    ++row;
                    row_inc = false;
                }
                else
                {
                    ++col;
                    row_inc = true;
                }
            }

            // Update the indexes of row 
            // and col variable 
            if (row == 0)
            {
                if (col == m - 1)
                    ++row;
                else
                    ++col;
                row_inc = true;
            }
            else
            {
                if (row == n - 1)
                    ++col;
                else
                    ++row;
                row_inc = false;
            }

            // Print the next half 
            // zig-zag pattern 
            int MAX = Math.Max(m, n) - 1;
            for (int len, diag = MAX; diag > 0; --diag)
            {

                if (diag > mn)
                    len = mn;
                else
                    len = diag;

                for (int i = 0; i < len; ++i)
                {
                    results[row].Add(arr[counter]);
                    counter++;

                    if (i + 1 == len)
                        break;

                    // Update row or col value 
                    // according to the last 
                    // increment 
                    if (row_inc)
                    {
                        ++row;
                        --col;
                    }
                    else
                    {
                        ++col;
                        --row;
                    }
                }

                // Update the indexes of 
                // row and col variable 
                if (row == 0 || col == m - 1)
                {
                    if (col == m - 1)
                        ++row;
                    else
                        ++col;

                    row_inc = true;
                }

                else if (col == 0 || row == n - 1)
                {
                    if (row == n - 1)
                        ++col;
                    else
                        ++row;

                    row_inc = false;
                }
            }
            return results;
        }

        public List<sbyte> runLengthEncode(List<sbyte> arr)
        {
            List<sbyte> encodedValues = new List<sbyte>();
            int col = 0;
            while(col < arr.Count)
            {
                if(arr[col] == 0)
                {
                    sbyte length = 1;
                    while (col + 1 < arr.Count && arr[col + 1] == arr[col])
                    {
                        length++;
                        col++;
                        if (length == 127) break;
                    }
                    encodedValues.Add(0);
                    encodedValues.Add(length);
                } else
                {
                    encodedValues.Add(arr[col]);
                }
                col++;
                
            }
            return encodedValues;
        }
        public List<sbyte> runLengthDecode(sbyte[] arr)
        {
            List<sbyte> decodedValues = new List<sbyte>();
            int col = 0;
            while (col < arr.Length)
            {
                if(arr[col] == 0)
                {
                    sbyte length = arr[(col + 1)];
                    sbyte counter = length;
                    while (counter > 0)
                    {
                        decodedValues.Add(0);
                        counter--;
                    }
                    col += 2;
                } else
                {
                    sbyte val = arr[col];
                    decodedValues.Add(val);
                    col++;
                }

            }
            return decodedValues;
        }

        //public static int[,] diffQTable = new int[,] {
        //        { 8,8,8,8,8,8,8,8},
        //        { 8,8,8,8,8,8,8,8},
        //        { 8,8,8,8,8,8,8,8},
        //        { 8,8,8,8,8,8,8,8},
        //        { 8,8,8,8,8,8,8,8},
        //        { 8,8,8,8,8,8,8,8},
        //        { 8,8,8,8,8,8,8,8},
        //        { 8,8,8,8,8,8,8,8}
        //    };
        public static int[,] diffQTable = new int[,] {
                { 8,30,30,30,30,30,30,30},
                { 30,30,30,30,30,30,30,30},
                { 30,30,30,30,30,30,30,30},
                { 30,30,30,30,30,30,30,30},
                { 30,30,30,30,30,30,30,30},
                { 30,30,30,30,30,30,30,30},
                { 30,30,30,30,30,30,30,30},
                { 30,30,30,30,30,30,30,30}
            };
        public static int[,] lumQTable = new int[,] {
                { 16, 11,  10, 16, 24, 40, 51, 61},
                { 12, 12,  14, 19, 26, 58, 60, 55},
                { 14, 13,  16, 24, 40, 57, 69, 56},
                { 14, 17,  22, 29, 51, 87, 80, 62},
                { 18, 22,  37, 56, 68, 109, 103, 77},
                { 24, 35,  55, 64, 81, 104, 113, 92},
                { 49, 64,  78, 87, 103, 121, 120, 101},
                { 72, 92,  95, 98, 112, 100, 103, 99}
            };
        public static int[,]
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
}
