using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace DotNet3d
{
    public class Utilities
    {
        public static int SizeOf(string dotNetType)
        {
            Type t = Type.GetType(dotNetType);
            return System.Runtime.InteropServices.Marshal.SizeOf(t);
        }

        public static int Offset(List<string> dotNetTypes, int index)
        {
            int offset = 0;
            for (int i = 0; i < index; i++)
            {
                offset += SizeOf(dotNetTypes[i]);
            }
            return offset;
        }
        public static int TotalBytes(List<string> dotNetTypes)
        {
            return Offset(dotNetTypes, dotNetTypes.Count);
        }
        unsafe public static int NumToBytes(ValueType num, byte * data, int index)
        {
            int bytesCopied = 0;
            string dotNetType = num.GetType().ToString();

            // Learned about how to do this from looking at source of BitConverter.GetBytes()
            //      https://referencesource.microsoft.com/#mscorlib/system/bitconverter.cs,76

            byte *b = data + index;
            switch (dotNetType)
            {
                case "System.SByte":  *(sbyte*)b  = (sbyte)num;  break; 
                case "System.Byte":   *b          = (byte)num;   break; 
                case "System.Int16":  *(short*)b  = (short)num;  break; 
                case "System.UInt16": *(ushort*)b = (ushort)num; break; 
                case "System.Int32":  *(int*)b    = (int)num;    break; 
                case "System.UInt32": *(uint*)b   = (uint)num;   break; 
                case "System.Single": *(float*)b  = (float)num;  break; 
                case "System.Double": *(double*)b = (double)num; break; 
            }
            bytesCopied = SizeOf(dotNetType);
            return bytesCopied;
        }
        public static int NumToBytes(ValueType num, byte [] byteArr, int index)
        {
            int bytesCopied = 0;
            unsafe
            {
                fixed (byte * b = byteArr)
                {
                    bytesCopied = NumToBytes(num, b, index);
                }
            }
            return bytesCopied;
        }
        unsafe public static int TextToBytes(string text, string dotNetType, byte * data, int index)
        {
            ValueType num = null;
            switch (dotNetType)
            {
                case "System.SByte":  num = sbyte.Parse(text);  break; 
                case "System.Byte":   num = byte.Parse(text);   break; 
                case "System.Int16":  num = short.Parse(text);  break; 
                case "System.UInt16": num = ushort.Parse(text); break; 
                case "System.Int32":  num = int.Parse(text);    break; 
                case "System.UInt32": num = uint.Parse(text);   break; 
                case "System.Single": num = float.Parse(text);  break; 
                case "System.Double": num = double.Parse(text); break; 
            }

            int bytesCopied = NumToBytes(num, data, index);
            return bytesCopied;
        }
        unsafe public static ValueType BytesToNum(byte * bytePtr, int index, string dotNetType)
        {
            // For speed, using parts of BitConverter code:
            // Code here: https://referencesource.microsoft.com/#mscorlib/system/bitconverter.cs,76
            byte * byteOffset = bytePtr + index;
            switch (dotNetType)
            {
                case "System.SByte":  return *(sbyte *) byteOffset;
                case "System.Byte":   return *          byteOffset;            
                case "System.Int16":  return *(short *) byteOffset;   
                case "System.UInt16": return *(ushort *)byteOffset;  
                case "System.Int32":  return *(int *)   byteOffset;     
                case "System.UInt32": return *(uint *)  byteOffset;    
                case "System.Single": return *(float *) byteOffset;   
                case "System.Double": return *(double *)byteOffset;  
            }
            return null;
        }
        public static ValueType BytesToNum(byte [] byteArr, int index, string dotNetType)
        {
            ValueType num = null;
            unsafe
            {
                fixed (byte * b = byteArr)
                {
                    num = BytesToNum(b, index, dotNetType);
                }
            }
            return num;
        }
        public static byte[] AddArrays(byte[] array1, byte[] array2)
        {
            byte[] newArray = new byte[array1.Length + array2.Length];
            array1.CopyTo(newArray, 0);
            array2.CopyTo(newArray, array1.Length);
            return newArray;
        }
        public static void RGBMask2BGMask(string rgbMaskPath, string bgMaskPath)
        {
            // Learned how to go from managed memory to pointers for image manipulation from this:
            // https://docs.microsoft.com/en-us/dotnet/api/system.drawing.imaging.bitmapdata?view=netframework-4.7.2

            // src
            var src = (Bitmap)Image.FromFile(rgbMaskPath);
            var rect = new Rectangle(0, 0, src.Width, src.Height);
            System.Drawing.Imaging.BitmapData srcData = src.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, src.PixelFormat);
            IntPtr srcPtr = srcData.Scan0;
            int srcBytes = Math.Abs(srcData.Stride) * src.Height;
            byte[] srcRgbValues = new byte[srcBytes];
            System.Runtime.InteropServices.Marshal.Copy(srcPtr, srcRgbValues, 0, srcBytes);

            // dst
            var dst = new Bitmap(src.Width, src.Height, format: System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            System.Drawing.Imaging.BitmapData dstData = dst.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, dst.PixelFormat);
            IntPtr dstPtr = dstData.Scan0;
            int dstBytes = Math.Abs(dstData.Stride) * dst.Height;
            byte[] dstRgbValues = new byte[dstBytes];
            System.Runtime.InteropServices.Marshal.Copy(dstPtr, dstRgbValues, 0, dstBytes);

            // dst pallete - set to gray scale
            System.Drawing.Imaging.ColorPalette pal = dst.Palette;
            for (int i = 0; i < 256; i++)
            {
                pal.Entries[i] = Color.FromArgb(i, i, i);
            }
            dst.Palette = pal;

            // Update bg mask
            for (int d = 0; d < dstRgbValues.Length; d++)
            {
                int s = d * 4;                          // src is 4 bytes (rgba) per pixel; dst is 1 byte per pixel; 
                dstRgbValues[d] = srcRgbValues[s + 3];  // set dst color to src alpha (4th byte)
            }

            System.Runtime.InteropServices.Marshal.Copy(dstRgbValues, 0, dstPtr, dstBytes);
            src.UnlockBits(srcData);
            dst.UnlockBits(dstData);

            dst.Save(bgMaskPath);
        }
        public static void ClampScaleImage(
            string srcImagePath, 
            string dstImagePath, 
            int clampMin, 
            int clampMax, 
            byte scaleMin = 0, 
            byte scaleMax = 255)
        {

            // Learned how to go from managed memory to pointers for image manipulation from this:
            // https://docs.microsoft.com/en-us/dotnet/api/system.drawing.imaging.bitmapdata?view=netframework-4.7.2

            // src
            BitmapDecoder decoder = null;
            Stream imageStreamSource = new FileStream(srcImagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            {
                string ext = Path.GetExtension(srcImagePath);
                switch (ext.ToLower())
                {
                    case ".png":  decoder = new PngBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default); break;
                    case ".tif":
                    case ".tiff": decoder = new TiffBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default); break;
                    case ".jpg":
                    case ".jpeg": decoder = new JpegBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default); break;
                    case ".bmp":  decoder = new BmpBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default); break;
                    default:      Trace.Assert(false); break;      // unsupported extention
                }
            }
            BitmapSource bitmapSource = decoder.Frames[0];

            int srcBytesPerPixel = bitmapSource.Format.BitsPerPixel / 8;
            int stride = (int)bitmapSource.PixelWidth * srcBytesPerPixel;
            byte[] pixels = new byte[(int)bitmapSource.PixelHeight * stride];
            bitmapSource.CopyPixels(pixels, stride, 0);
            int srcWidth = bitmapSource.PixelWidth;
            int srcHeight = bitmapSource.PixelHeight;
            int srcTotalPixels = srcWidth * srcHeight;

            // dst
            var dst = new Bitmap(srcWidth, srcHeight, format: System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var rect = new Rectangle(0, 0, srcWidth, srcHeight);
            System.Drawing.Imaging.BitmapData dstData = dst.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, dst.PixelFormat);
            IntPtr dstPtr = dstData.Scan0;
            int dstBytes = Math.Abs(dstData.Stride) * dst.Height;
            byte[] dstRgbValues = new byte[dstBytes];
            System.Runtime.InteropServices.Marshal.Copy(dstPtr, dstRgbValues, 0, dstBytes);
            int dstBytesPerPixel = Image.GetPixelFormatSize(dst.PixelFormat) / 8;
            Debug.Assert(dstBytesPerPixel == 3);

            for (int i = 0; i < srcTotalPixels; i++)
            {
                int s = i * srcBytesPerPixel;
                int d = i * dstBytesPerPixel;

                byte vLo = pixels[s + 0];
                byte vHi = pixels[s + 1];
                int v =  (vHi << 8) | vLo;   

                v = Math.Min(v, clampMax);
                v = Math.Max(v, clampMin);
                float ans = (v - clampMin) * (scaleMax - scaleMin) / (clampMax - clampMin);
                dstRgbValues[d + 0] = (byte)ans;
                dstRgbValues[d + 1] = (byte)ans;
                dstRgbValues[d + 2] = (byte)ans;
            }

            System.Runtime.InteropServices.Marshal.Copy(dstRgbValues, 0, dstPtr, dstBytes);
            dst.UnlockBits(dstData);

            System.Drawing.Imaging.ImageFormat format = null;
            {
                string ext = Path.GetExtension(dstImagePath);
            
                format = System.Drawing.Imaging.ImageFormat.Bmp;
                switch(ext.ToLower())
                {
                    case ".png":  format = System.Drawing.Imaging.ImageFormat.Png; break;
                    case ".tif":  
                    case ".tiff": format = System.Drawing.Imaging.ImageFormat.Tiff; break;
                    case ".jpg": 
                    case ".jpeg": format = System.Drawing.Imaging.ImageFormat.Jpeg; break;
                    case ".bmp":  format = System.Drawing.Imaging.ImageFormat.Bmp; break;
                    default: Trace.Assert(false); break;      // unsupported extention
                }
            }
            dst.Save(dstImagePath, format);
        }
        public static int GetPixel_16bit(string srcImagePath, int x, int y)
        {
            // src
            BitmapDecoder decoder = null;
            Stream imageStreamSource = new FileStream(srcImagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            {
                string ext = Path.GetExtension(srcImagePath);
                switch (ext.ToLower())
                {
                    case ".png": decoder = new PngBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default); break;
                    case ".tif":
                    case ".tiff": decoder = new TiffBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default); break;
                    case ".jpg":
                    case ".jpeg": decoder = new JpegBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default); break;
                    case ".bmp": decoder = new BmpBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default); break;
                    default: Trace.Assert(false); break;      // unsupported extention
                }


            }
            BitmapSource bitmapSource = decoder.Frames[0];

            int srcBytesPerPixel = bitmapSource.Format.BitsPerPixel / 8;

            int stride = (int)bitmapSource.PixelWidth * srcBytesPerPixel;
            byte[] pixels = new byte[(int)bitmapSource.PixelHeight * stride];
            bitmapSource.CopyPixels(pixels, stride, 0);
            int srcWidth = bitmapSource.PixelWidth;
            int srcHeight = bitmapSource.PixelHeight;
            int srcTotalPixels = srcWidth * srcHeight;
            
            int i = y * srcWidth + x;
            int s = i * srcBytesPerPixel;

            byte vLo = pixels[s + 0];
            byte vHi = pixels[s + 1];
            int v = (vHi << 8) | vLo;
            return v;
        }
    }
}