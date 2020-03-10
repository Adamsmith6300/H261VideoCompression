﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assignment2
{
    class InterFrame : Frame
    {
        public Frame rFrame { get; set; }
        public List<List<MV>> mvY { get; set; }
        public List<List<MV>> mvCb { get; set; }
        public List<List<MV>> mvCr { get; set; }
        public List<List<double>> yDiffs { get; set; }
        public List<List<double>> cbDiffs { get; set; }
        public List<List<double>> crDiffs { get; set; }
        public List<List<List<double>>> yDiffBlocks { get; set; }
        public List<List<List<double>>> cbDiffBlocks { get; set; }
        public List<List<List<double>>> crDiffBlocks { get; set; }

        public InterFrame(Bitmap bmp): base(bmp)
        {
        }

        public InterFrame(sbyte[] sbytes, int w, int h): base(sbytes, w, h)
        {

        }

        public void generateDiffBlocks()
        {
            yDiffs = generateDiffs(mvY, this.yDoubles, rFrame.yDoubles);
        }

        public List<List<double>> generateDiffs(List<List<MV>> mv, List<List<double>> target, List<List<double>> reference)
        {
            List<List<double>> diffs = new List<List<double>>();


            return diffs;
        }

        public void generateMv(Frame rFrame, int p)
        {
            this.rFrame = rFrame;
            Debug.Write(this.yDoubles.Count + ":" + rFrame.yDoubles.Count);
            mvY = mvSearch(p, 8, this.yDoubles, rFrame.yDoubles);
        }

        public List<List<MV>> mvSearch(int p, int n, List<List<double>> target, List<List<double>> reference)
        {
            //int blocksAcross = target.Count / n;
            //int blocksDown = target.Count / n;
            List<List<MV>> motionVectors = new List<List<MV>>();

            //loop through each block in target frame
            for(int row = 0; row < target.Count; row+=n)
            {
                motionVectors.Add(new List<MV>());
                for(int col = 0; col < target[row].Count; col+=n)
                {
                    int u = row, v = col;
                    double min_mad = Double.MaxValue;
                    for(int i = -p; i < p; i++)
                    {
                        for(int j = -p; j < p; j++)
                        {
                            double cur_mad = mad(i, j, row, col, n, target, reference);
                            if (cur_mad < min_mad)
                            {
                                min_mad = cur_mad;
                                u = i;
                                v = j;
                            }
                        }
                    }
                    motionVectors[row/n].Add(new MV(u, v));
                }
            }
            return motionVectors;

        }

        public double mad(int i, int j, int x, int y, int n, List<List<double>> target, List<List<double>> reference)
        {
            double diff = 0.0;
            for(int k = 0; k < n; k++)
            {
                for(int l = 0; l < n; l++)
                {
                    int targetX = x + k;
                    int targetY = y + l;
                    int refX = x + i + k;
                    int refY = y + j + l;
                    double targetValue = 0.0;
                    if((targetY >= 0 && targetY < target.Count) && (targetX >= 0 && targetX < target[0].Count))
                    {
                        targetValue = target[targetX][targetY];
                    }
                    double refValue = 0.0;
                    if ((refY >= 0 && refY < reference.Count) && (refX >= 0 && refX < reference[0].Count))
                    {
                        refValue = reference[refX][refY];
                    }
                    diff += Math.Abs(targetValue - refValue);
                }
            }
            return (1 / (Math.Pow(n, 2)) * diff);
        }

        public Bitmap drawMotionVectors()
        {
            Pen redPen = new Pen(Color.Red, 1);
            Brush brsh = new SolidBrush(ColorTranslator.FromHtml("#ff00ffff"));
            for (int row = 0; row < yDoubles.Count; row += 8)
            {
                for (int col = 0; col < yDoubles[row].Count; col += 8)
                {
                    int startX = row + 4;
                    int startY = col + 4;
                    int endX = startX + mvY[row/8][col/8].x;
                    int endY = startY + mvY[row/8][col/8].y;
                    using (var graphics = Graphics.FromImage(ogBmp))
                    {
                        if(startX == endX && startY == endY)
                        {
                            graphics.FillEllipse(brsh, startX, startY, 2, 2);
                        } else
                        {
                            graphics.DrawLine(redPen, startX, startY, endX, endY);
                        }
                    }
                }
            }
            return ogBmp;
        }
    }
}
