using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Drawing;

namespace DotNet3d
{
    unsafe public class Ply
    {
        // PLY format: http://paulbourke.net/dataformats/ply/
        private class Property
        {
            public string type;                 // list, char, uchar, short, ushort, int, uint, float, double
            public string name;                 // x, y, z, red, green, blue, etc.
        }
        private class PlyList : Property
        {
            public string numIndicesType;       // char, uchar, short, ushort, int, uint, float, double
            public string listType;             // char, uchar, short, ushort, int, uint, float, double
        }
        private class Element
        {
            public string name;
            public int number;
            public List<Property> properties = new List<Property>();
        }
        private List<Element> Elements = new List<Element>();
        private byte[] Data = null;


        public Ply(string path)
        {
            string format        = null;        // ascii, binary_little_endian, binary_big_endian
            string formatVersion = null;
            int currentPosition  = 0;

            // load header
            using (var reader = new StreamReader(path))
            {
                // header starts with 'ply'
                int b1 = reader.BaseStream.ReadByte();
                int b2 = reader.BaseStream.ReadByte();
                int b3 = reader.BaseStream.ReadByte();
                Trace.Assert(b1 == 'p' &&
                             b2 == 'l' &&
                             b3 == 'y');

                // line ending can be \r\n (Windows) or \n (Unix)
                int numLineEndingBytes = 0;
                int b4 = reader.BaseStream.ReadByte();
                if (b4 == '\r')
                {
                    int b5 = reader.BaseStream.ReadByte();
                    Trace.Assert(b5 == '\n');
                    numLineEndingBytes = 2;
                }
                else
                {
                    Trace.Assert(b4 == '\n');
                    numLineEndingBytes = 1;
                }
                currentPosition += "ply".Length + numLineEndingBytes;


                string line = null;
                while ((line = reader.ReadLine()) != "end_header")
                {
                    currentPosition += line.Length + numLineEndingBytes;
                    string[] parts  = line.Split();
                    string keyword  = parts[0];
                    int i           = -1;
                    int j           = -1;
                    switch (keyword)
                    {
                        case "format":
                            format        = parts[1];
                            formatVersion = parts[2];
                            break;
                        case "element":
                            Elements.Add(new Element());
                            i = Elements.Count - 1;
                            Elements[i].name   = parts[1];
                            Elements[i].number = int.Parse(parts[2]);
                            break;
                        case "property":
                            i = Elements.Count - 1;
                            if (parts[1] == "list")
                            {
                                PlyList l        = new PlyList();
                                l.type           = parts[1];
                                l.numIndicesType = parts[2];
                                l.listType       = parts[3];
                                l.name           = parts[4];
                                Elements[i].properties.Add(l);
                            }
                            else
                            {
                                Property p = new Property();
                                p.type     = parts[1];
                                p.name     = parts[2];
                                Elements[i].properties.Add(p);
                            }
                            j = Elements[i].properties.Count - 1;
                            break;
                    }
                }
                currentPosition += line.Length + numLineEndingBytes;
            }

            // load data
            if (format == "ascii")
            {
#if OLDWAY
                Data = new byte[0];
                using (var reader = new StreamReader(path))
                {
                    reader.BaseStream.Seek(currentPosition, SeekOrigin.Begin);
                    string[] lines = reader.ReadToEnd().Split('\n');
                    int lineNum = 0;
                    foreach (var el in Elements)
                    {                                      
                        for (int i = 0; i < el.number; i++)
                        {
                            string[] parts = lines[lineNum].Trim().Split();
                            lineNum++;
                            int p = 0;

                            foreach(var pr in el.properties)
                            {
                                if (pr.type == "list")
                                {
                                    PlyList pl = pr as PlyList;
                                    int count = parts.Length - 1 - p;
                                    int listSize = Utilities.SizeOf(pl.numIndicesType) + Utilities.SizeOf(pl.listType) * count;
                                    byte[] listBytes = new byte[listSize];
                                    int index = 0;
                                    fixed (byte * b = listBytes)
                                    {
                                        index += Utilities.TextToBytes(parts[p], PlyToDotNetType(pl.numIndicesType), b, index);
                                    }
                                    for (int j = 0; j < count; j++)
                                    {
                                        fixed (byte * b = listBytes)
                                        {
                                            index += Utilities.TextToBytes(parts[p+j], PlyToDotNetType(pl.listType), b, index);
                                        }
                                    }
                                    p += count + 1;
                                    Data = Utilities.AddArrays(Data, listBytes);
                                }
                                else
                                {
                                    string propText = parts[p]; 
                                    p++;
                                    string dotNetType = PlyToDotNetType(pr.type);
                                    int propSize = Utilities.SizeOf(dotNetType);
                                    byte[] propBytes = new byte[propSize];
                                    fixed (byte * b = propBytes)
                                    {
                                        Utilities.TextToBytes(propText, dotNetType, b, 0);
                                    }
                                    Data = Utilities.AddArrays(Data, propBytes);
                                }
                            }
                        }
                    }
                }
#else
                Data = new byte[0];
                using (var reader = new StreamReader(path))
                {
                    reader.BaseStream.Seek(currentPosition, SeekOrigin.Begin);
                    string[] lines = reader.ReadToEnd().Split('\n');
                    
                    // calculate space needed
                    int lineNum = 0;
                    int numBytes = 0;
                    foreach (var el in Elements)
                    {
                        for (int i = 0; i < el.number; i++)
                        {
                            string[] parts = lines[lineNum].Trim().Split();
                            lineNum++;
                            int p = 0;

                            foreach (var pr in el.properties)
                            {
                                if (pr.type == "list")
                                {
                                    PlyList pl = pr as PlyList;
                                    int count = parts.Length - 1 - p;
                                    int listSize = Utilities.SizeOf(pl.numIndicesType) + Utilities.SizeOf(pl.listType) * count;
                                    numBytes += listSize;
                                    p += count + 1;
                                }
                                else
                                {
                                    string propText = parts[p];
                                    p++;
                                    string dotNetType = PlyToDotNetType(pr.type);
                                    int propSize = Utilities.SizeOf(dotNetType);
                                    numBytes += propSize;
                                }
                            }
                        }
                    }
                
                    Data = new byte[numBytes];

                    // fill Data with file contents
                    lineNum = 0;
                    int index = 0;
                    foreach (var el in Elements)
                    {
                        for (int i = 0; i < el.number; i++)
                        {
                            string[] parts = lines[lineNum].Trim().Split();
                            lineNum++;
                            int p = 0;

                            foreach (var pr in el.properties)
                            {
                                if (pr.type == "list")
                                {
                                    PlyList pl = pr as PlyList;
                                    int count = parts.Length - 1 - p;
                                    //int listSize = Utilities.SizeOf(pl.numIndicesType) + Utilities.SizeOf(pl.listType) * count;
                                    // byte[] listBytes = new byte[listSize];
                                    
                                    fixed (byte* b = Data)
                                    {
                                        index += Utilities.TextToBytes(parts[p], PlyToDotNetType(pl.numIndicesType), b, index);
                                    }
                                    for (int j = 0; j < count; j++)
                                    {
                                        fixed (byte* b = Data)
                                        {
                                            index += Utilities.TextToBytes(parts[p + j], PlyToDotNetType(pl.listType), b, index);
                                        }
                                    }
                                    p += count + 1;
                                    //Data = Utilities.AddArrays(Data, listBytes);
                                }
                                else
                                {
                                    string propText = parts[p];
                                    p++;
                                    string dotNetType = PlyToDotNetType(pr.type);
                                    //int propSize = Utilities.SizeOf(dotNetType);
                                    // byte[] propBytes = new byte[propSize];
                                    fixed (byte* b = Data)
                                    {
                                        index += Utilities.TextToBytes(propText, dotNetType, b, index);
                                    }
                                    //Data = Utilities.AddArrays(Data, propBytes);
                                }
                            }
                        }
                    }
                }
#endif

            }
            else if (format == "binary_little_endian")
            {
                using (var binReader = new BinaryReader(File.Open(path, FileMode.Open)))
                {
                    binReader.BaseStream.Seek(currentPosition, SeekOrigin.Begin);
                    int deltaBytes = (int)binReader.BaseStream.Length - currentPosition;
                
                    // Learned about byte[] to byte * here: https://referencesource.microsoft.com/#mscorlib/system/bitconverter.cs,76
                    Data = binReader.ReadBytes(deltaBytes);
                }
            }
            else if (format == "binary_big_endian")
            {
                // TODO             
            }
        }
        public Ply(Point[] ptArr)
        {
            int numVerts = ptArr.Length;  
            var ev = new Element { name = "vertex", number = numVerts };
            var pvx = new Property { name = "x", type = "float" };
            var pvy = new Property { name = "y", type = "float" };
            var pvz = new Property { name = "z", type = "float" };
            ev.properties.Add(pvx);
            ev.properties.Add(pvy);
            ev.properties.Add(pvz);
            Elements.Add(ev);

            int numBytes = 0;
            foreach (var el in Elements)
            {
                int bytesPerElement = 0;
                foreach (var pr in el.properties)
                {
                    bytesPerElement += Utilities.SizeOf(PlyToDotNetType(pr.type));
                }
                numBytes += bytesPerElement * el.number;
            }
            Data = new byte[numBytes];

            int index = 0;
            for (int i = 0; i < ptArr.Length; i++)
            {
                index += Utilities.NumToBytes((float)ptArr[i].X, Data, index);
                index += Utilities.NumToBytes((float)ptArr[i].Y, Data, index);
                index += Utilities.NumToBytes((float)ptArr[i].Z, Data, index);
            }
        }
        public Ply(PointCloud pointCloud)
        {
            var el = new Element
            {
                name   = "vertex",
                number = pointCloud.NumPoints
            };
            for (int i = 0; i < pointCloud.DotNetTypes.Count; i++)
            {
                var pr = new Property
                {
                    name = pointCloud.Names[i],
                    type = DotNetToPlyType(pointCloud.DotNetTypes[i])
                };
                el.properties.Add(pr);
            }
            Elements.Add(el);
            Data = pointCloud.Data;
        }
        public Ply(Matrix m)
        {
            var matArr = new Matrix[1];
            matArr[0] = m;
            Load(matArr);
        }
        public Ply(Matrix[] matArr)
        {
            Load(matArr);
        }
        
        private string getAscii()
        {
            string header = GetHeader("ascii");
            var sb = new StringBuilder();
            sb.Append(header);
            int offset = 0;
            foreach (var el in Elements)
            {
                for (int i = 0; i < el.number; i++)
                {
                    for (int j = 0; j < el.properties.Count; j++)
                    {
                        if (j != 0)
                        {
                            sb.Append(" ");
                        }
                        if (el.properties[j].type == "list")
                        {
                            var pl = el.properties[j] as PlyList;
                            int? numIndicies = Utilities.BytesToNum(Data, offset, PlyToDotNetType(pl.numIndicesType)) as int?;
                            sb.Append(numIndicies.ToString());
                            offset += Utilities.SizeOf(PlyToDotNetType(pl.numIndicesType));
                            for (int k = 0; k < numIndicies; k++)
                            {
                                sb.Append(" ");
                                var num = Utilities.BytesToNum(Data, offset, PlyToDotNetType(pl.listType));
                                sb.Append(num.ToString());
                                offset += Utilities.SizeOf(PlyToDotNetType(pl.listType));
                            }
                        }
                        else
                        {
                            string dotNetType = PlyToDotNetType(el.properties[j].type);
                            var num = Utilities.BytesToNum(Data, offset, dotNetType);
                            sb.Append(num.ToString());
                            offset += Utilities.SizeOf(dotNetType);
                        }
                    }
                    sb.Append("\n");
                }
            }
            return sb.ToString();
        }
        public override string ToString()
        {
            return getAscii();
        }
        public void SaveAscii(string path)
        {
            //string header = GetHeader("ascii");
            //var sb = new StringBuilder();
            //sb.Append(header);
            //int offset = 0;
            //foreach (var el in Elements)
            //{
            //    for (int i = 0; i < el.number; i++)
            //    {
            //        for (int j = 0; j < el.properties.Count; j++)
            //        {
            //            if (j != 0)
            //            {
            //                sb.Append(" ");
            //            }
            //            if (el.properties[j].type == "list")
            //            {
            //                var pl = el.properties[j] as PlyList;
            //                int? numIndicies = Utilities.BytesToNum(Data, offset, PlyToDotNetType(pl.numIndicesType)) as int?;
            //                sb.Append(numIndicies.ToString());
            //                offset += Utilities.SizeOf(PlyToDotNetType(pl.numIndicesType));
            //                for (int k = 0; k < numIndicies; k++)
            //                {
            //                    sb.Append(" ");
            //                    var num = Utilities.BytesToNum(Data, offset, PlyToDotNetType(pl.listType));
            //                    sb.Append(num.ToString());
            //                    offset += Utilities.SizeOf(PlyToDotNetType(pl.listType));
            //                }
            //            }
            //            else
            //            {   
            //                string dotNetType = PlyToDotNetType(el.properties[j].type);
            //                var num           = Utilities.BytesToNum(Data, offset, dotNetType);
            //                sb.Append(num.ToString());
            //                offset += Utilities.SizeOf(dotNetType);
            //            }
            //        }
            //        sb.Append("\n");
            //    }
            //}
            var txt = getAscii();
            File.WriteAllText(path, txt);  
        }
        public void SaveBinary(string path)
        {
            string header = GetHeader("binary_little_endian");
            byte [] headerBytes = Encoding.ASCII.GetBytes(header);
            using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                writer.Write(headerBytes);
                writer.Write(Data);
            }
        }
        public PointCloud ToPointCloud()
        {
            int offset = 0;
            foreach (var el in Elements)
            {
                if (el.name == "vertex")
                {
                    // Do not support "property list" for vertex. Point cloud must be of known size for speed.
                    List<string> names       = GetNames(el.properties);
                    List<string> dotNetTypes = GetDotNetTypes(el.properties);
                    int numBytes = Utilities.TotalBytes(dotNetTypes) * el.number;

                    byte[] pointCloudBytes = Data;
                    if (offset != 0 || numBytes != Data.Length)
                    {
                        // Create new array that only contains point cloud data
                        pointCloudBytes = new byte[numBytes];
                        Array.Copy(Data, offset, pointCloudBytes, 0, pointCloudBytes.Length);
                    }
                    return new PointCloud(el.number, dotNetTypes, names, pointCloudBytes);
                }
                else
                {
                    foreach (var pr in el.properties)
                    {
                        if (pr.type == "list")
                        {
                            var pl = pr as PlyList;
                            int numIndicies = (int)Utilities.BytesToNum(Data, offset, pl.numIndicesType);
                            offset += Utilities.SizeOf(PlyToDotNetType(pl.numIndicesType));
                            offset += numIndicies * Utilities.SizeOf(PlyToDotNetType(pl.listType));
                        }
                        else
                        {
                            offset += Utilities.SizeOf(PlyToDotNetType(pr.type));
                        }
                    }
                }
            }
            return null;
        }
        private static string PlyToDotNetType(string plyType)
        {
            string dotNetType = null;
            switch (plyType)
            {
                case "char":   dotNetType = "System.SByte";  break;
                case "uchar":  dotNetType = "System.Byte";   break;
                case "short":  dotNetType = "System.Int16";  break;
                case "ushort": dotNetType = "System.UInt16"; break;
                case "int":    dotNetType = "System.Int32";  break;
                case "uint":   dotNetType = "System.UInt32"; break;
                case "float":  dotNetType = "System.Single"; break;
                case "double": dotNetType = "System.Double"; break;
            }
            return dotNetType;
        }
        private static string DotNetToPlyType(string dotNetType)
        {
            string plyType = null;
            switch (dotNetType)
            {
                case "System.SByte":  plyType = "char";   break;
                case "System.Byte":   plyType = "uchar";  break;
                case "System.Int16":  plyType = "short";  break;
                case "System.UInt16": plyType = "ushort"; break;
                case "System.Int32":  plyType = "int";    break;
                case "System.UInt32": plyType = "uint";   break;
                case "System.Single": plyType = "float";  break;
                case "System.Double": plyType = "double"; break;
            }
            return plyType;
        }
        private static List<string> GetDotNetTypes(List<Property> properties)
        {
            List<string> dotNetTypes = new List<string>();
            foreach (var pr in properties)
            {
                dotNetTypes.Add(PlyToDotNetType(pr.type));
            }
            return dotNetTypes;
        }
        private static List<string> GetNames(List<Property> properties)
        {
            List<string> names = new List<string>();
            foreach (var pr in properties)
            {
                names.Add(pr.name);
            }
            return names;
        }
        private string GetHeader(string format)
        {
            var sb = new StringBuilder();
            sb.Append("ply\n");
            sb.Append($"format {format} 1.0\n");
            sb.Append("comment Created by DotNet3d\n");
            foreach (var el in Elements)
            {
                sb.Append($"element {el.name} {el.number}\n");
                foreach (var pr in el.properties)
                {
                    if (pr.type == "list")
                    {
                        var pl = pr as PlyList;
                        sb.Append($"property {pl.type} {pl.numIndicesType} {pl.listType} {pl.name}\n");
                    }
                    else
                    {
                        sb.Append($"property {pr.type} {pr.name}\n");
                    }
                }
            }
            sb.Append("end_header\n");
            return sb.ToString();
        }
        private void Load(Matrix [] matArr)
        {
            int numVerts = 4 * matArr.Length;  // 4 points: origin, plus x, y, z axis
            var ev  = new Element  {name = "vertex", number = numVerts};
            var pvx = new Property {name = "x",     type = "float"};
            var pvy = new Property {name = "y",     type = "float"};
            var pvz = new Property {name = "z",     type = "float"};
            var pvr = new Property {name = "red",   type = "uchar"};
            var pvg = new Property {name = "green", type = "uchar"};
            var pvb = new Property {name = "blue",  type = "uchar"};
            var pva = new Property {name = "alpha", type = "uchar"};
            ev.properties.Add(pvx);
            ev.properties.Add(pvy);
            ev.properties.Add(pvz);
            ev.properties.Add(pvr);
            ev.properties.Add(pvg);
            ev.properties.Add(pvb);
            ev.properties.Add(pva);
            
            int numEdges = 3 * matArr.Length; // 3 axises
            var ee  = new Element  {name = "edge", number = numEdges};
            var pe1 = new Property {name = "vertex1", type = "int"};
            var pe2 = new Property {name = "vertex2", type = "int"};
            var per = new Property {name = "red",     type = "uchar"};
            var peg = new Property {name = "green",   type = "uchar"};
            var peb = new Property {name = "blue",    type = "uchar"};
            var pea = new Property {name = "alpha",   type = "uchar"};
            ee.properties.Add(pe1);
            ee.properties.Add(pe2);
            ee.properties.Add(per);
            ee.properties.Add(peg);
            ee.properties.Add(peb);
            ee.properties.Add(pea);

            Elements.Add(ev);
            Elements.Add(ee);

            int numBytes = 0;
            foreach (var el in Elements)
            {
                int bytesPerElement = 0;
                foreach (var pr in el.properties)
                {
                    bytesPerElement += Utilities.SizeOf(PlyToDotNetType(pr.type));
                }
                numBytes += bytesPerElement * el.number;
            }
            Data = new byte[numBytes];

            Color xAxisColor  = Color.FromArgb(255, 0, 0);
            Color yAxisColor  = Color.FromArgb(0, 255, 0);
            Color zAxisColor  = Color.FromArgb(0, 0, 255);
            
            int index = 0;
            const byte alpha = 255;

            for (int i = 0; i < matArr.Length; i++)
            {
                /////////
                // vertex

                var mat      = matArr[i];
                int d        = 500;                           // distance from origin (number that shows up in meshlab)
                Point origin = mat * new Point(0, 0, 0);
                Point xAxis  = mat * new Point(d, 0, 0);
                Point yAxis  = mat * new Point(0, d, 0);
                Point zAxis  = mat * new Point(0, 0, 2*d);    // double z so it will be easier to pick from x and y

                // origin - color is monochrome and matches order of index (ie first camera matrix is 0,0,0; second is 1,1,1
                index += Utilities.NumToBytes((float)origin.X, Data, index);
                index += Utilities.NumToBytes((float)origin.Y, Data, index);
                index += Utilities.NumToBytes((float)origin.Z, Data, index);
                index += Utilities.NumToBytes((byte)i, Data, index);
                index += Utilities.NumToBytes((byte)i, Data, index);
                index += Utilities.NumToBytes((byte)i, Data, index);
                index += Utilities.NumToBytes(alpha, Data, index);
            
                // x axis
                index += Utilities.NumToBytes((float)xAxis.X, Data, index);
                index += Utilities.NumToBytes((float)xAxis.Y, Data, index);
                index += Utilities.NumToBytes((float)xAxis.Z, Data, index);
                index += Utilities.NumToBytes(xAxisColor.R, Data, index);
                index += Utilities.NumToBytes(xAxisColor.G, Data, index);
                index += Utilities.NumToBytes(xAxisColor.B, Data, index);
                index += Utilities.NumToBytes(alpha, Data, index);

                // y axis
                index += Utilities.NumToBytes((float)yAxis.X, Data, index);
                index += Utilities.NumToBytes((float)yAxis.Y, Data, index);
                index += Utilities.NumToBytes((float)yAxis.Z, Data, index);
                index += Utilities.NumToBytes(yAxisColor.R, Data, index);
                index += Utilities.NumToBytes(yAxisColor.G, Data, index);
                index += Utilities.NumToBytes(yAxisColor.B, Data, index);
                index += Utilities.NumToBytes(alpha, Data, index);

                // z axis
                index += Utilities.NumToBytes((float)zAxis.X, Data, index);
                index += Utilities.NumToBytes((float)zAxis.Y, Data, index);
                index += Utilities.NumToBytes((float)zAxis.Z, Data, index);
                index += Utilities.NumToBytes(zAxisColor.R, Data, index);
                index += Utilities.NumToBytes(zAxisColor.G, Data, index);
                index += Utilities.NumToBytes(zAxisColor.B, Data, index);
                index += Utilities.NumToBytes(alpha, Data, index);
            }
            for (int i = 0; i < matArr.Length; i++)
            {
                ///////
                // edge
            
                int matrixStartIdx = i * 4;
                int originIdx = 0 + matrixStartIdx;
                int xAxisIdx  = 1 + matrixStartIdx;
                int yAxisIdx  = 2 + matrixStartIdx;
                int zAxisIdx  = 3 + matrixStartIdx;

                // x axis
                index += Utilities.NumToBytes(originIdx, Data, index);
                index += Utilities.NumToBytes(xAxisIdx, Data, index);
                index += Utilities.NumToBytes(xAxisColor.R, Data, index);
                index += Utilities.NumToBytes(xAxisColor.G, Data, index);
                index += Utilities.NumToBytes(xAxisColor.B, Data, index);
                index += Utilities.NumToBytes(alpha, Data, index);

                // y axis
                index += Utilities.NumToBytes(originIdx, Data, index);
                index += Utilities.NumToBytes(yAxisIdx, Data, index);
                index += Utilities.NumToBytes(yAxisColor.R, Data, index);
                index += Utilities.NumToBytes(yAxisColor.G, Data, index);
                index += Utilities.NumToBytes(yAxisColor.B, Data, index);
                index += Utilities.NumToBytes(alpha, Data, index);

                // z axis
                index += Utilities.NumToBytes(originIdx, Data, index);
                index += Utilities.NumToBytes(zAxisIdx, Data, index);
                index += Utilities.NumToBytes(zAxisColor.R, Data, index);
                index += Utilities.NumToBytes(zAxisColor.G, Data, index);
                index += Utilities.NumToBytes(zAxisColor.B, Data, index);
                index += Utilities.NumToBytes(alpha, Data, index);
            }
        }
    }
}
