using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace DotNet3d
{
    public class PointCloud
    {
        public int          NumPoints;
        int                 BytesPerPoint;
        public int          NumProperties;
        public List<string> DotNetTypes;
        public List<string> Names;
        public byte[]       Data;
        public PointCloud(int numPoints, List<string> dotNetTypes, List<string> names, byte[] data)
        {
            NumPoints        = numPoints;
            DotNetTypes      = dotNetTypes;
            Names            = names;
            Data             = data;
            BytesPerPoint    = Utilities.TotalBytes(dotNetTypes);
            NumProperties    = dotNetTypes.Count;
            Trace.Assert(NumProperties == names.Count);
        }
        public PointCloud(Point[] pointArr)
        {
            NumPoints = pointArr.Length;
            
            DotNetTypes = new List<string>();
            DotNetTypes.Add("System.Single");      // x
            DotNetTypes.Add("System.Single");      // y
            DotNetTypes.Add("System.Single");      // z
            
            Names = new List<string>();
            Names.Add("x");
            Names.Add("y");
            Names.Add("z");

            int numBytes = NumPoints * Utilities.TotalBytes(DotNetTypes);
            Data = new byte[numBytes];
            int index = 0;
            for (int i = 0; i < NumPoints; i++)
            {
                index += Utilities.NumToBytes((float)pointArr[i].X, Data, index);
                index += Utilities.NumToBytes((float)pointArr[i].Y, Data, index);
                index += Utilities.NumToBytes((float)pointArr[i].Z, Data, index);
            }

            BytesPerPoint = Utilities.TotalBytes(DotNetTypes);
            NumProperties = DotNetTypes.Count;
            Trace.Assert(NumProperties == Names.Count);
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < NumPoints; i++)
            {
                int index = i * BytesPerPoint;
                for (int j = 0; j < NumProperties; j++)
                {
                    if (j != 0)
                    {
                        sb.Append(" ");
                    }
                    int startIndex = index + Utilities.Offset(DotNetTypes, j);
                    var num = Utilities.BytesToNum(Data, startIndex, DotNetTypes[j]);
                    sb.Append(num);
                }
                sb.Append("\n");
            }
            return sb.ToString();
        }
        public void Multiply(Matrix m)
        {
            // position
            bool hasPosition = Names.IndexOf("x") != -1;
            int xIndex = Names.IndexOf("x");
            int yIndex = Names.IndexOf("y");
            int zIndex = Names.IndexOf("z");
            int xOffset = Utilities.Offset(DotNetTypes, xIndex);
            int yOffset = Utilities.Offset(DotNetTypes, yIndex);
            int zOffset = Utilities.Offset(DotNetTypes, zIndex);
            string xDotNetType = DotNetTypes[xIndex];
            string yDotNetType = DotNetTypes[yIndex];
            string zDotNetType = DotNetTypes[zIndex];
            Trace.Assert(xDotNetType == yDotNetType);
            Trace.Assert(xDotNetType == zDotNetType);

            // normals
            bool hasNormals = Names.IndexOf("nx") != -1;
            int nxOffset = 0;
            int nyOffset = 0;
            int nzOffset = 0;
            if (hasNormals)
            {
                int nxIndex = Names.IndexOf("nx");
                int nyIndex = Names.IndexOf("ny");
                int nzIndex = Names.IndexOf("nz");
                nxOffset = Utilities.Offset(DotNetTypes, nxIndex);
                nyOffset = Utilities.Offset(DotNetTypes, nyIndex);
                nzOffset = Utilities.Offset(DotNetTypes, nzIndex);
                string nxDotNetType = DotNetTypes[nxIndex];
                string nyDotNetType = DotNetTypes[nyIndex];
                string nzDotNetType = DotNetTypes[nzIndex];
                Trace.Assert(nxDotNetType == nyDotNetType);
                Trace.Assert(nxDotNetType == nzDotNetType);
            }
            Matrix noTrans = new Matrix(m);
            noTrans.M14 = 0;
            noTrans.M24 = 0;
            noTrans.M34 = 0;
            noTrans.M44 = 1;

            for (int i = 0; i < NumPoints; i++)
            {
                int pointIndex = i * BytesPerPoint;
                unsafe
                {
                    fixed (byte* b = &Data[pointIndex])
                    {
                        if (hasPosition)
                        {
                            double x = *(float*)(b + xOffset);
                            double y = *(float*)(b + yOffset);
                            double z = *(float*)(b + zOffset);

                            var p = new Point(x, y, z);
                            p = m * p;

                            *(float*)(b + xOffset) = (float)p.X;
                            *(float*)(b + yOffset) = (float)p.Y;
                            *(float*)(b + zOffset) = (float)p.Z;
                        }
                        if (hasNormals)
                        {
                            double nx = *(float*)(b + nxOffset);
                            double ny = *(float*)(b + nyOffset);
                            double nz = *(float*)(b + nzOffset);

                            var n = new Vector(nx, ny, nz);
                            n = noTrans * n;

                            *(float*)(b + nxOffset) = (float)n.X;
                            *(float*)(b + nyOffset) = (float)n.Y;
                            *(float*)(b + nzOffset) = (float)n.Z;
                        }
                    }
                }
            }
        }
        public int Count() => NumPoints;
        public ValueType Get(string Name, int PointNum)
        {
            int nameIndex = Names.IndexOf(Name);
            if (nameIndex == -1)
            {
                return null;
            }
            int offset        = Utilities.Offset(DotNetTypes, nameIndex);
            int index         = PointNum * BytesPerPoint + offset;
            string dotNetType = DotNetTypes[nameIndex];
            return Utilities.BytesToNum(Data, index, dotNetType);
        }
        public void Set(string Name, int PointNum, ValueType num)
        {
            int nameIndex = Names.IndexOf(Name);
            if (nameIndex == -1)
            {
                Environment.Exit(1);
            }
            int offset        = Utilities.Offset(DotNetTypes, nameIndex);
            int index         = PointNum * BytesPerPoint + offset;
            string dotNetType = DotNetTypes[nameIndex];
            Utilities.NumToBytes(num, Data, index);
        }
        public void DividePositionByZ()
        {
            Trace.Assert(Names.IndexOf("x") != -1);
            Trace.Assert(Names.IndexOf("y") != -1);
            Trace.Assert(Names.IndexOf("z") != -1);

            int xIndex = Names.IndexOf("x");
            int yIndex = Names.IndexOf("y");
            int zIndex = Names.IndexOf("z");

            int xOffset = Utilities.Offset(DotNetTypes, xIndex);
            int yOffset = Utilities.Offset(DotNetTypes, yIndex);
            int zOffset = Utilities.Offset(DotNetTypes, zIndex);
            
            string xDotNetType = DotNetTypes[xIndex];
            string yDotNetType = DotNetTypes[yIndex];
            string zDotNetType = DotNetTypes[zIndex];
            
            Trace.Assert(xDotNetType == yDotNetType);
            Trace.Assert(xDotNetType == zDotNetType);

            for (int i = 0; i < NumPoints; i++)
            {
                int pointIndex = i * BytesPerPoint;
                unsafe
                {
                    fixed (byte* b = &Data[pointIndex])
                    {
                        float x = *(float*)(b + xOffset);
                        float y = *(float*)(b + yOffset);
                        float z = *(float*)(b + zOffset);

                        x /= z;
                        y /= z;
                        z /= z;

                        *(float*)(b + xOffset) = x;
                        *(float*)(b + yOffset) = y;
                        *(float*)(b + zOffset) = z;
                    }
                }
            }
        }
        public void DrawOnImage(string inputImagePath, string outputImagePath)
        {
            Trace.Assert(Names.IndexOf("x") != -1);
            Trace.Assert(Names.IndexOf("y") != -1);

            int xIndex = Names.IndexOf("x");
            int yIndex = Names.IndexOf("y");

            int xOffset = Utilities.Offset(DotNetTypes, xIndex);
            int yOffset = Utilities.Offset(DotNetTypes, yIndex);
            
            string xDotNetType = DotNetTypes[xIndex];
            string yDotNetType = DotNetTypes[yIndex];
            Trace.Assert(xDotNetType == yDotNetType);

            // src
            Bitmap src = (Bitmap)Image.FromFile(inputImagePath);
            var rect = new Rectangle(0, 0, src.Width, src.Height);
            System.Drawing.Imaging.BitmapData srcData = src.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, src.PixelFormat);
            IntPtr srcPtr = srcData.Scan0;
            int srcBytes = Math.Abs(srcData.Stride) * src.Height;
            byte[] srcRgbValues = new byte[srcBytes];
            System.Runtime.InteropServices.Marshal.Copy(srcPtr, srcRgbValues, 0, srcBytes);
            int srcBytesPerPixel = Image.GetPixelFormatSize(src.PixelFormat)/8;

            // dst - rgb, 3 bytes per pixel
            var dst = new Bitmap(src.Width, src.Height, format: System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            System.Drawing.Imaging.BitmapData dstData = dst.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, dst.PixelFormat);
            IntPtr dstPtr = dstData.Scan0;
            int dstBytes = Math.Abs(dstData.Stride) * dst.Height;
            byte[] dstRgbValues = new byte[dstBytes];
            System.Runtime.InteropServices.Marshal.Copy(dstPtr, dstRgbValues, 0, dstBytes);
            int dstBytesPerPixel = Image.GetPixelFormatSize(dst.PixelFormat)/8;
            Debug.Assert(dstBytesPerPixel == 3);


            // Update bg mask
            int numPixels = dstRgbValues.Length / dstBytesPerPixel;
            for (int pixel = 0; pixel < numPixels; pixel++)
            {
                int s = pixel * srcBytesPerPixel;       // 1 bpp
                int d = pixel * dstBytesPerPixel;       // 3 bpp
                dstRgbValues[d + 0] = srcRgbValues[s + 0];
                dstRgbValues[d + 1] = srcRgbValues[s + 0];
                dstRgbValues[d + 2] = srcRgbValues[s + 0];
            }

            // draw point cloud
            int numBadPoints = 0;
            for (int i = 0; i < NumPoints; i++)
            {
                int pointIndex = i * BytesPerPoint;
                unsafe
                {
                    fixed (byte* b = &Data[pointIndex])
                    {
                        int x = (int)*(float*)(b + xOffset);
                        int y = (int)*(float*)(b + yOffset);
                        Debug.WriteLine("{0}, {1}, {2}", i, x, y);

                        if (x >= 0 && x < src.Width &&
                            y >= 0 && y < src.Height)
                        {
                            int pixel = y * src.Width + x;
                            int srcByte = pixel * srcBytesPerPixel;
                            int dstByte = pixel * dstBytesPerPixel;
                            if (srcRgbValues[srcByte] == 255)
                            {
                                // point cloud within mask
                                dstRgbValues[dstByte + 0] = Color.DarkGreen.B;
                                dstRgbValues[dstByte + 1] = Color.DarkGreen.G;
                                dstRgbValues[dstByte + 2] = Color.DarkGreen.R;
                            }
                            else
                            {
                                // point cloud outside of mask
                                numBadPoints++;
                                dstRgbValues[dstByte + 0] = Color.Red.B;
                                dstRgbValues[dstByte + 1] = Color.Red.G;
                                dstRgbValues[dstByte + 2] = Color.Red.R;
                            }
                        }
                        else
                        {
                            // point cloud outside of image
                            numBadPoints++;
                        }
                    }
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(dstRgbValues, 0, dstPtr, dstBytes);
            src.UnlockBits(srcData);
            dst.UnlockBits(dstData);




            //// draw point cloud
            //for (int i = 0; i < numPoints; i++)
            //{
            //    int pointIndex = i * bytesPerPoint;
            //    unsafe
            //    {
            //        fixed (byte* b = &data[pointIndex])
            //        {
            //            int x = (int)*(float*)(b + xOffset);
            //            int y = (int)*(float*)(b + yOffset);

            //            if (x > 0 && x < src.Width &&
            //                y > 0 && y < src.Height) 
            //            {
            //                if (src.GetPixel(x, y).A == 255)  
            //                {
            //                    // point cloud within mask
            //                    dst.SetPixel(x, y, Color.DarkGreen);
            //                }
            //                else 
            //                {
            //                    // point cloud outside of mask
            //                    dst.SetPixel(x, y, Color.Red);
            //                }
            //            }                                                               
            //        }
            //    }
            //}

            //// Replace transparent pixels with a dark color so white mask pixeles are visible
            //for (int y = 0; y < dst.Height; y++)
            //{
            //    for (int x = 0; x < dst.Width; x++)
            //    {
            //        if (dst.GetPixel(x, y).A == 0)
            //        {
            //            dst.SetPixel(x, y, Color.Black);
            //        }
            //    }
            //}

            dst.Save(outputImagePath);
        }
        public static PointCloud operator +(PointCloud one, PointCloud two) 
        {
            // Verify point cloud data formats match
            Trace.Assert(one.DotNetTypes.Count == two.DotNetTypes.Count);
            for (int i = 0; i < one.DotNetTypes.Count; i++)
            {
                Trace.Assert(one.DotNetTypes[i] == two.DotNetTypes[i]);
            }
            Trace.Assert(one.Names.Count == two.Names.Count);
            for (int i = 0; i < one.Names.Count; i++)
            {
                Trace.Assert(one.Names[i] == two.Names[i]);
            }

            int numPoints = one.Count() + two.Count();
            byte[] data = Utilities.AddArrays(one.Data, two.Data);
            
            return new PointCloud(numPoints, one.DotNetTypes, one.Names, data);
        }
    }
}
