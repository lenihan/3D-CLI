using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace TestDotNet3d
{
    class Program
    {
        private static void TestPoint()
        {
            // DotNet3d.Point code coverage
            {
                DotNet3d.Point p0 = new DotNet3d.Point();
                p0.X = 10;
                p0.Y = 11;
                p0.Z = 12;
                var str = p0.ToString();
                Trace.Assert(str == "(10 11 12)\n");
            }

            DotNet3d.Point p1 = new DotNet3d.Point();
            Trace.Assert(p1.ToString() == "(0 0 0)\n");

            DotNet3d.Point p2 = new DotNet3d.Point
            {
                X = 1,
                Y = 2,
                Z = 3
            };
            Trace.Assert(p2.ToString() == "(1 2 3)\n");

            DotNet3d.Point p3 = new DotNet3d.Point(p2);
            Trace.Assert(p3.ToString() == "(1 2 3)\n");

            DotNet3d.Point p4 = new DotNet3d.Point(4, 5, 6);
            Trace.Assert(p4.ToString() == "(4 5 6)\n");
            double[] arr = { 7, 8, 9 };

            DotNet3d.Point p5 = new DotNet3d.Point(arr);
            Trace.Assert(p5.ToString() == "(7 8 9)\n");

            DotNet3d.Point p6 = p2 + p4;
            Trace.Assert(p6.ToString() == "(5 7 9)\n");

            DotNet3d.Point p7 = 2 * p2;
            Trace.Assert(p7.ToString() == "(2 4 6)\n");

            DotNet3d.Point p8 = p2 * 2;
            Trace.Assert(p8.ToString() == "(2 4 6)\n");

            DotNet3d.Point p9 = p2 / 2;
            Trace.Assert(p9.ToString() == "(0.5 1 1.5)\n");
        }
        private static void TestVector()
        {
            {
                DotNet3d.Vector v0 = new DotNet3d.Vector();
                v0.X = 10;
                v0.Y = 11;
                v0.Z = 12;
                Trace.Assert(v0.ToString() == "[10 11 12]\n");

                DotNet3d.Vector v1 = new DotNet3d.Vector();
                Trace.Assert(v1.ToString() == "[0 0 0]\n");

                DotNet3d.Vector v2 = new DotNet3d.Vector
                {
                    X = 1,
                    Y = 2,
                    Z = 3
                };
                Trace.Assert(v2.ToString() == "[1 2 3]\n");

                DotNet3d.Vector v3 = new DotNet3d.Vector(v2);
                Trace.Assert(v3.ToString() == "[1 2 3]\n");

                DotNet3d.Vector v4 = new DotNet3d.Vector(4, 5, 6);
                Trace.Assert(v4.ToString() == "[4 5 6]\n");
                double[] arr2 = { 7, 8, 9 };

                DotNet3d.Vector v5 = new DotNet3d.Vector(arr2);
                Trace.Assert(v5.ToString() == "[7 8 9]\n");

                DotNet3d.Vector v6 = v2 + v4;
                Trace.Assert(v6.ToString() == "[5 7 9]\n");

                DotNet3d.Vector v7 = 2 * v2;
                Trace.Assert(v7.ToString() == "[2 4 6]\n");

                DotNet3d.Vector v8 = v2 * 2;
                Trace.Assert(v8.ToString() == "[2 4 6]\n");

                DotNet3d.Vector v9 = v2 / 2;
                Trace.Assert(v9.ToString() == "[0.5 1 1.5]\n");

                DotNet3d.Vector v10 = new DotNet3d.Vector(1, 2, 2);
                Trace.Assert(v10.Length() == 3);
            }

            // Normalize
            {
                DotNet3d.Vector v1 = new DotNet3d.Vector(1, 2, 3);
                var v1n = v1.Normalize();
                var str = v1n.ToString();
                Trace.Assert(str ==           "[0.27 0.53 0.8]\n");
                Trace.Assert(v1.ToString() == "[0.27 0.53 0.8]\n");
                Trace.Assert(v1.X == 0.2672612419124244);
                Trace.Assert(v1.Y == 0.53452248382484879);
                Trace.Assert(v1.Z == 0.80178372573727319);


                DotNet3d.Vector v2 = new DotNet3d.Vector(1, 2, 3);
                Trace.Assert(DotNet3d.Vector.Normalize(v2).ToString() == "[0.27 0.53 0.8]\n");
            }

            {
                DotNet3d.Vector v12 = new DotNet3d.Vector(1, 2, 3);
                DotNet3d.Point p10 = new DotNet3d.Point(1, 2, 3);
                Trace.Assert((v12 + p10).ToString() == "(2 4 6)\n");
                Trace.Assert((p10 + v12).ToString() == "(2 4 6)\n");

                DotNet3d.Vector v13 = new DotNet3d.Vector(4, 5, 6);
                DotNet3d.Vector v14 = new DotNet3d.Vector(3, 2, 1);
                Trace.Assert((v13 - v14).ToString() == "[1 3 5]\n");

                DotNet3d.Vector v15 = new DotNet3d.Vector(1, 2, 3);
                DotNet3d.Vector v16 = new DotNet3d.Vector(10, 11, 12);
                // Verified by http://onlinemschool.com/math/assistance/vector/multiply/
                Trace.Assert(DotNet3d.Vector.Dot(v15, v16) == 68);

                DotNet3d.Vector v17 = new DotNet3d.Vector(1, 2, 3);
                DotNet3d.Vector v18 = new DotNet3d.Vector(10, 11, 12);
                // Verified by https://www.symbolab.com/solver/Vector-cross-product-calculator
                Trace.Assert(DotNet3d.Vector.Cross(v17, v18).ToString() == "[-9 18 -9]\n");

                DotNet3d.Vector v19 = new DotNet3d.Vector(1, 2, 3);
                v19 = -v19;
                Trace.Assert(v19.ToString() == "[-1 -2 -3]\n");
            }
        }
        private static void TestMatrix()
        {
            {

                var txt = "";
                DotNet3d.Matrix m0 = new DotNet3d.Matrix();
                txt = "               1                0                0                0\n" +
                      "               0                1                0                0\n" +
                      "               0                0                1                0\n" +
                      "               0                0                0                1\n";
                var str = m0.ToString();
                Trace.Assert(str == txt);

                DotNet3d.Matrix m1 = new DotNet3d.Matrix(1, 2, 3, 4,
                                        5, 6, 7, 8,
                                        9, 10, 11, 12,
                                       13, 14, 15, 16);
                txt = "               1                2                3                4\n" +
                      "               5                6                7                8\n" +
                      "               9               10               11               12\n" +
                      "              13               14               15               16\n";
                Trace.Assert(m1.ToString() == txt);
                
                {
                    double[] _array = new double[9] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                    DotNet3d.Matrix _m = new DotNet3d.Matrix(_array);
                    var _txt = "               1                2                3                0\n" +
                               "               4                5                6                0\n" +
                               "               7                8                9                0\n" +
                               "               0                0                0                1\n";
                    Trace.Assert(_m.ToString() == _txt);
                }


                DotNet3d.Matrix m2 = new DotNet3d.Matrix(m1);
                Trace.Assert(m2.ToString() == txt);

                double[] array = new double[16] { 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
                DotNet3d.Matrix m3 = new DotNet3d.Matrix(array);
                txt = "              16               15               14               13\n" +
                      "              12               11               10                9\n" +
                      "               8                7                6                5\n" +
                      "               4                3                2                1\n";
                Trace.Assert(m3.ToString() == txt);

                DotNet3d.Vector vx = new DotNet3d.Vector(1, 1, 1);
                DotNet3d.Vector vy = new DotNet3d.Vector(2, 2, 2);
                DotNet3d.Vector vz = new DotNet3d.Vector(3, 3, 3);
                DotNet3d.Matrix m4 = new DotNet3d.Matrix(vx, vy, vz);
                txt = "               1                2                3                0\n" +
                      "               1                2                3                0\n" +
                      "               1                2                3                0\n" +
                      "               0                0                0                1\n";
                Trace.Assert(m4.ToString() == txt);

                DotNet3d.Matrix m5 = new DotNet3d.Matrix();
                m5.Translate(1, 2, 3);
                txt = "               1                0                0                1\n" +
                      "               0                1                0                2\n" +
                      "               0                0                1                3\n" +
                      "               0                0                0                1\n";
                Trace.Assert(m5.ToString() == txt);

                DotNet3d.Matrix m6 = new DotNet3d.Matrix();
                m6.Translate(vx);
                txt = "               1                0                0                1\n" +
                      "               0                1                0                1\n" +
                      "               0                0                1                1\n" +
                      "               0                0                0                1\n";
                Trace.Assert(m6.ToString() == txt);

                DotNet3d.Matrix m7 = new DotNet3d.Matrix();
                m7.Scale(2, 4, 8);
                txt = "               2                0                0                0\n" +
                      "               0                4                0                0\n" +
                      "               0                0                8                0\n" +
                      "               0                0                0                1\n";
                Trace.Assert(m7.ToString() == txt);

                DotNet3d.Matrix m8 = new DotNet3d.Matrix();
                m8.RotateX(90);
                str = m8.ToString();
                txt = "               1                0                0                0\n" +
                      "               0                0               -1                0\n" +
                      "               0                1                0                0\n" +
                      "               0                0                0                1\n";
                Trace.Assert(str == txt);

                DotNet3d.Matrix m9 = new DotNet3d.Matrix();
                m9.RotateY(90);
                txt = "               0                0                1                0\n" +
                      "               0                1                0                0\n" +
                      "              -1                0                0                0\n" +
                      "               0                0                0                1\n";
                Trace.Assert(m9.ToString() == txt);

                DotNet3d.Matrix m10 = new DotNet3d.Matrix();
                m10.RotateZ(90);
                txt = "               0               -1                0                0\n" +
                      "               1                0                0                0\n" +
                      "               0                0                1                0\n" +
                      "               0                0                0                1\n";
                Trace.Assert(m10.ToString() == txt);

                DotNet3d.Matrix m11 = new DotNet3d.Matrix(1, 2, 3, 4,
                                        5, 6, 7, 8,
                                        9, 10, 11, 12,
                                       13, 14, 15, 16);
                Trace.Assert(m11.Determinate() == 0);

                DotNet3d.Matrix m12 = new DotNet3d.Matrix(7, 6, 5, 4,
                                        1, 2, 3, 4,
                                        9, 8, 7, 6,
                                        2, 4, 6, 8);
                m12.Invert();   // determinate is zero (ie not invertable), divide by zero gives NaN
                txt = "             NaN              NaN              NaN              NaN\n" +
                      "             NaN              NaN              NaN              NaN\n" +
                      "             NaN              NaN              NaN              NaN\n" +
                      "             NaN              NaN              NaN              NaN\n";
                str = m12.ToString();
                Trace.Assert(str == txt);

                DotNet3d.Matrix m13 = new DotNet3d.Matrix(7, 3, 5, 4,
                                        3, 2, 3, 1,
                                        9, 3, 7, 6,
                                        1, 4, 6, 8);
                m13.Invert();   // Verified numbers are correct via http://matrix.reshish.com/inverse.php
                Trace.Assert(m13.M11 == 0.34090909090909094);
                Trace.Assert(m13.M12 == -0.22727272727272729);
                Trace.Assert(m13.M13 == -0.068181818181818177);
                Trace.Assert(m13.M14 == -0.090909090909090912);
                Trace.Assert(m13.M21 == 1.0340909090909092);
                Trace.Assert(m13.M22 == -0.022727272727272728);
                Trace.Assert(m13.M23 == -0.80681818181818188);
                Trace.Assert(m13.M24 == 0.090909090909090912);
                Trace.Assert(m13.M31 == -1.125);
                Trace.Assert(m13.M32 == 0.75);
                Trace.Assert(m13.M33 == 0.625);
                Trace.Assert(m13.M34 == 0);
                Trace.Assert(m13.M41 == 0.28409090909090912);
                Trace.Assert(m13.M42 == -0.52272727272727271);
                Trace.Assert(m13.M43 == -0.056818181818181823);
                Trace.Assert(m13.M44 == 0.090909090909090912);

                DotNet3d.Matrix m14 = DotNet3d.Matrix.Identity();
                m14 = 3.0 * m14;
                txt = "               3                0                0                0\n" +
                      "               0                3                0                0\n" +
                      "               0                0                3                0\n" +
                      "               0                0                0                3\n";
                Trace.Assert(m14.ToString() == txt);

                DotNet3d.Matrix m15 = new DotNet3d.Matrix(1, 2, 3, 4,
                                        5, 6, 7, 8,
                                        9, 10, 11, 12,
                                        13, 14, 15, 16);
                DotNet3d.Matrix m16 = new DotNet3d.Matrix(3, 11, -3, 2,
                                        -2, 2, 99, 18,
                                        59, 17, 121, 112,
                                        3, 4, -5, 106);
                // answer verified by http://matrix.reshish.com/multCalculation.php
                txt = "             188               82              538              798\n" +
                      "             440              218            1,386            1,750\n" +
                      "             692              354            2,234            2,702\n" +
                      "             944              490            3,082            3,654\n";
                str = (m15 * m16).ToString();
                Trace.Assert(str == txt);

                DotNet3d.Vector v1 = new DotNet3d.Vector(1, 2, 3);
                DotNet3d.Matrix m17 = DotNet3d.Matrix.Identity();
                txt = "[1 2 3]\n";
                str = (m17 * v1).ToString();
                Trace.Assert(str == txt);
                DotNet3d.Point p1 = new DotNet3d.Point(3, 2, 1);
                txt = "(3 2 1)\n";
                Trace.Assert((m17 * p1).ToString() == txt);
                txt = "               0                0                0                0\n" +
                      "               0                0                0                0\n" +
                      "               0                0                0                0\n" +
                      "               0                0                0                0\n";
                Trace.Assert(DotNet3d.Matrix.Zero().ToString() == txt);
                txt = "               1                0                0                1\n" +
                      "               0                1                0                2\n" +
                      "               0                0                1                3\n" +
                      "               0                0                0                1\n";
                Trace.Assert(DotNet3d.Matrix.Translation(1, 2, 3).ToString() == txt);
                Trace.Assert(DotNet3d.Matrix.Translation(new DotNet3d.Vector(1, 2, 3)).ToString() == txt);
                txt = "               2                0                0                0\n" +
                      "               0                3                0                0\n" +
                      "               0                0                4                0\n" +
                      "               0                0                0                1\n";
                Trace.Assert(DotNet3d.Matrix.Scaling(2, 3, 4).ToString() == txt);
                txt = "               1                0                0                0\n" +
                      "               0                0               -1                0\n" +
                      "               0                1                0                0\n" +
                      "               0                0                0                1\n";
                Trace.Assert(DotNet3d.Matrix.RotationX(90).ToString() == txt);
                txt = "               0                0                1                0\n" +
                      "               0                1                0                0\n" +
                      "              -1                0                0                0\n" +
                      "               0                0                0                1\n";
                Trace.Assert(DotNet3d.Matrix.RotationY(90).ToString() == txt);
                txt = "               0               -1                0                0\n" +
                      "               1                0                0                0\n" +
                      "               0                0                1                0\n" +
                      "               0                0                0                1\n";
                Trace.Assert(DotNet3d.Matrix.RotationZ(90).ToString() == txt);

                DotNet3d.Matrix m18 = new DotNet3d.Matrix(16, 15, 14, 13,
                                        12, 11, 10, 9,
                                        8, 7, 6, 5,
                                        4, 3, 2, 1);
                txt = "              16               12                8                4\n" +
                      "              15               11                7                3\n" +
                      "              14               10                6                2\n" +
                      "              13                9                5                1\n";
                Trace.Assert(DotNet3d.Matrix.Transpose(m18).ToString() == txt);

                DotNet3d.Matrix m19 = new DotNet3d.Matrix(7, 3, 5, 4,
                                       3, 2, 3, 1,
                                       9, 3, 7, 6,
                                       1, 4, 6, 8);
                // Verified numbers are correct via http://matrix.reshish.com/inverse.php
                txt = "            0.34            -0.23            -0.07            -0.09\n" +
                      "            1.03            -0.02            -0.81             0.09\n" +
                      "           -1.13             0.75             0.63                0\n" +
                      "            0.28            -0.52            -0.06             0.09\n";
                var m19i = DotNet3d.Matrix.Invert(m19);
                str = m19i.ToString();
                Trace.Assert(str == txt);
                Trace.Assert(m19i.M11 == 0.34090909090909094);
                Trace.Assert(m19i.M12 == -0.22727272727272729);
                Trace.Assert(m19i.M13 == -0.068181818181818177);
                Trace.Assert(m19i.M14 == -0.090909090909090912);
                Trace.Assert(m19i.M21 == 1.0340909090909092);
                Trace.Assert(m19i.M22 == -0.022727272727272728);
                Trace.Assert(m19i.M23 == -0.80681818181818188);
                Trace.Assert(m19i.M24 == 0.090909090909090912);
                Trace.Assert(m19i.M31 == -1.125);
                Trace.Assert(m19i.M32 == 0.75);
                Trace.Assert(m19i.M33 == 0.625);
                Trace.Assert(m19i.M34 == 0);
                Trace.Assert(m19i.M41 == 0.28409090909090912);
                Trace.Assert(m19i.M42 == -0.52272727272727271);
                Trace.Assert(m19i.M43 == -0.056818181818181823);
                Trace.Assert(m19i.M44 == 0.090909090909090912);

            }

            // Create matrix from 3x3 matrix
            {
                var m3x3 = new DotNet3d.Matrix(1, 2, 3, 4, 5, 6, 7, 8, 9);
                var txt = "               1                2                3                0\n" +
                          "               4                5                6                0\n" +
                          "               7                8                9                0\n" +
                          "               0                0                0                1\n";
                Trace.Assert(m3x3.ToString() == txt);
            }

            // Add two matrices
            {
                var a = new DotNet3d.Matrix(1, 2, 3, 4,
                                            5, 6, 7, 8,
                                            9, 10, 11, 12,
                                            13, 14, 15, 16);
                var b = new DotNet3d.Matrix(2, 4, 6, 8,
                                            10, 12, 14, 16,
                                            18, 20, 22, 24,
                                            26, 28, 30, 32);
                var txt = "               3                6                9               12\n" +
                          "              15               18               21               24\n" +
                          "              27               30               33               36\n" +
                          "              39               42               45               48\n";
                Trace.Assert((a+b).ToString() == txt);
            }
        }
        private static void TestPly()
        {
            System.IO.Directory.CreateDirectory(@"..\..\testData\Output");
            var plyAscii  = new DotNet3d.Ply(@"..\..\testData\CalibrationXml-Ascii-UnixLineEndings.ply");
            var PointCloudAscii = plyAscii.ToPointCloud();
            var asciiTxt = PointCloudAscii.ToString();
            
            var plyBinary = new DotNet3d.Ply(@"..\..\testData\CalibrationXml-Binary.ply");
            var PointCloudBinary = plyBinary.ToPointCloud();
            var binaryTxt = PointCloudBinary.ToString();
            
            Trace.Assert(asciiTxt == binaryTxt);

            var plyAsciiWinLineEndings  = new DotNet3d.Ply(@"..\..\testData\CalibrationXml-Ascii-WinLineEndings.ply");
            var PointCloudAsciiWinLineEndings = plyAsciiWinLineEndings.ToPointCloud();
            var asciiWinLineEndingsTxt = PointCloudAsciiWinLineEndings.ToString();
            Trace.Assert(asciiTxt == asciiWinLineEndingsTxt);

            plyAscii.SaveAscii( @"..\..\testData\Output\CalibrationXml-Ascii-ToAscii.ply");
            plyAscii.SaveBinary(@"..\..\testData\Output\CalibrationXml-Ascii-ToBinary.ply");

            plyBinary.SaveAscii( @"..\..\testData\Output\CalibrationXml-Binary-ToAscii.ply");
            plyBinary.SaveBinary(@"..\..\testData\Output\CalibrationXml-Binary-ToBinary.ply");

            // array of points
            {
                var ptArr = new DotNet3d.Point[3];
                ptArr[0] = new DotNet3d.Point(0, 0, 0);
                ptArr[1] = new DotNet3d.Point(1, 1, 1);
                ptArr[2] = new DotNet3d.Point(-1, -1, -1);

                var ply = new DotNet3d.Ply(ptArr);

                var txt =
                    "ply\n" +
                    "format ascii 1.0\n" +
                    "comment Created by DotNet3d\n" +
                    "element vertex 3\n" +
                    "property float x\n" +
                    "property float y\n" +
                    "property float z\n" +
                    "end_header\n" +
                    "0 0 0\n" +
                    "1 1 1\n" +
                    "-1 -1 -1\n";

                Trace.Assert(ply.ToString() == txt);
            }

            // point from strings
            {
                var pt = new DotNet3d.Point("1 2 3");
                Trace.Assert(pt.ToString() == "(1 2 3)\n");

                pt = new DotNet3d.Point("2, 4, 6");
                Trace.Assert(pt.ToString() == "(2 4 6)\n");
                
                pt = new DotNet3d.Point("3\t6\t9");
                Trace.Assert(pt.ToString() == "(3 6 9)\n");

                pt = new DotNet3d.Point("(4, 8, 12)");
                Trace.Assert(pt.ToString() == "(4 8 12)\n");
            }
            // Test loading binary (if this takes longer than a second to load, this consider this test a failure)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var ply = new DotNet3d.Ply(@"..\..\testData\BIN-color_to_depth_Frame1.ply");
                sw.Stop();
                long oneSec = 1000;
                // NOTE: This takes about 2 ms on my home pc
                Trace.Assert(sw.ElapsedMilliseconds < oneSec);
            }
            // Test loading ascii (if this takes longer than a second to load, this consider this test a failure)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var ply = new DotNet3d.Ply(@"..\..\testData\color_to_depth_Frame1.ply");
                sw.Stop();
                long tenSeconds = 10 * 1000;
                // NOTE: This once took 853414 ms (14 min), now it takes about 5 seconds on my home pc in debug mode
                Trace.Assert(sw.ElapsedMilliseconds < tenSeconds);
            }
        }
        private static void TestPointCloud()
        {
            System.IO.Directory.CreateDirectory(@"..\..\testData\Output");

            // TEST DotNet3d.ply from DotNet3d.Pointcloud
            {
                var ply  = new DotNet3d.Ply(@"..\..\testData\Frame_00600.ply");                 // ~200ms
                var pc = ply.ToPointCloud();                                                    // ~1ms


                var moveToOrigin   = DotNet3d.Matrix.Translation(-722.58, 123.87, -2805.85);    // ~1ms
                var rotateX90      = DotNet3d.Matrix.RotationX(90);                             // ~1ms
                var rotateY180     = DotNet3d.Matrix.RotationY(180);                            // ~1ms
                var rotateZ30      = DotNet3d.Matrix.RotationZ(30);                             // ~1ms
                var superMatrix    = rotateZ30 * rotateY180 * rotateX90 * moveToOrigin;         // ~1ms

                pc.Multiply(superMatrix);                                                       // ~1400ms 
                var ply2 = new DotNet3d.Ply(pc);                                                // ~1ms
                ply2.SaveBinary(@"..\..\testData\Output\Frame_00600-ONFLOOR-FACINGZ.ply");      // ~1100ms
            }

            // Test writing matrix as ply
            {
                var mat = new DotNet3d.Matrix();
                var ply = new DotNet3d.Ply(mat);
                ply.SaveAscii(@"..\..\testData\Output\IdentityMatrix.ply");
            }

            // Test writing array of matrices as ply
            {
                var mat1 = DotNet3d.Matrix.Identity();
                var mat2 = DotNet3d.Matrix.Identity();
                mat2.Translate(1000, 1000, 1000);
                var matArr = new DotNet3d.Matrix[2];

                matArr[0] = mat1;
                matArr[1] = mat2;
                var ply = new DotNet3d.Ply(matArr);
                ply.SaveBinary(@"..\..\testData\Output\2Matices.ply");
            }
            // Test seting, getting
            {
                float x = 1;
                float y = 2;
                float z = 3;
                var mat = DotNet3d.Matrix.Translation(x, y, z);
                var ply = new DotNet3d.Ply(mat);
                var pc = ply.ToPointCloud();
                Trace.Assert((float)pc.Get("x", 0) == x);
                Trace.Assert((float)pc.Get("y", 0) == y);
                Trace.Assert((float)pc.Get("z", 0) == z);
                Trace.Assert((float)pc.Get("x", 1) == (x + 500));
                Trace.Assert((float)pc.Get("y", 1) == y);
                Trace.Assert((float)pc.Get("z", 1) == z);
                Trace.Assert((float)pc.Get("x", 2) == x);
                Trace.Assert((float)pc.Get("y", 2) == (y + 500));
                Trace.Assert((float)pc.Get("z", 2) == z);
                Trace.Assert((float)pc.Get("x", 3) == x);
                Trace.Assert((float)pc.Get("y", 3) == y);
                Trace.Assert((float)pc.Get("z", 3) == (z + 1000));

                float dummy = 9;
                pc.Set("x", 0, dummy);
                Trace.Assert((float)pc.Get("x", 0) == dummy);   
                
                byte dummy2 = 32;
                pc.Set("red", 1, dummy2);
                Trace.Assert((byte)pc.Get("red", 1) == dummy2);   
            }
            // Test dividing by z
            {
                float x = 2;
                float y = 4;
                float z = 8;
                var mat = DotNet3d.Matrix.Translation(x, y, z);
                var ply = new DotNet3d.Ply(mat);
                var pc = ply.ToPointCloud();
                Trace.Assert((float)pc.Get("x", 0) == x);
                Trace.Assert((float)pc.Get("y", 0) == y);
                Trace.Assert((float)pc.Get("z", 0) == z);
                pc.DividePositionByZ();
                Trace.Assert((float)pc.Get("x", 0) == x/z);
                Trace.Assert((float)pc.Get("y", 0) == y/z);
                Trace.Assert((float)pc.Get("z", 0) == z/z);
            }
            // Test drawing on image
            {
                int size = 256;
                var pointArr = new DotNet3d.Point[size];
                for (int i = 0; i < size; i++)
                {
                    pointArr[i] = new DotNet3d.Point();
                    pointArr[i].X = i;
                    pointArr[i].Y = i;
                    pointArr[i].Z = i;
                } 
                var pc = new DotNet3d.PointCloud(pointArr);
                var newImg = new System.Drawing.Bitmap(size, size);
                var src = System.IO.Path.GetFullPath(@"..\..\testData\Output\TestDrawOnImage-EMPTY.png");
                var dst = System.IO.Path.GetFullPath(@"..\..\testData\Output\TestDrawOnImage-LINE.png");

                newImg.Save(src);                                       // src: black 256x256 image
                pc.DrawOnImage(src, dst);                               // dst: black 256x256 with red line from upper left to lower right
            }
            // Test drawing on image, 8-bit index
            {
                int size = 3840;
                var pointArr = new DotNet3d.Point[size];
                for (int i = 0; i < size; i++)
                {
                    pointArr[i] = new DotNet3d.Point();
                    pointArr[i].X = i;
                    pointArr[i].Y = i;
                    pointArr[i].Z = i;
                }
                var pc = new DotNet3d.PointCloud(pointArr);
                var src = System.IO.Path.GetFullPath(@"..\..\testData\Mask-Cam0000-F00001-8bitIndexed.png");
                var dst = System.IO.Path.GetFullPath(@"..\..\testData\Output\Mask-Cam0000-F00001-8bitIndexed-LINE.png");

                pc.DrawOnImage(src, dst);                               // dst: 5120x3840 mask with white image of person, with red line that starts at upper left and ends at bottom right (if bottom right were square)
            }

            // Test adding point clouds
            {
                float x1 = 2;
                float y1 = 4;
                float z1 = 8;
                var mat1 = DotNet3d.Matrix.Translation(x1, y1, z1);
                var ply1 = new DotNet3d.Ply(mat1);
                var one = ply1.ToPointCloud();
                
                float x2 = 3;
                float y2 = 6;
                float z2 = 9;
                var mat2 = DotNet3d.Matrix.Translation(x2, y2, z2);
                var ply2 = new DotNet3d.Ply(mat2);
                var two = ply2.ToPointCloud();
                DotNet3d.PointCloud three = one + two;
               
                Trace.Assert(three.ToString() == (one.ToString() + two.ToString()));

                Trace.Assert(three.Data.Length == (one.Data.Length + two.Data.Length));
                for (int i = 0; i < one.Data.Length; i++)
                {
                    Trace.Assert(three.Data[i] == one.Data[i]);
                    int j = i + one.Data.Length; // second half of data
                    Trace.Assert(three.Data[j] == two.Data[i]);
                }

                var ply3 = new DotNet3d.Ply(three);
                ply3.SaveAscii(@"..\..\testData\Output\AddingPointClouds-Ascii.ply");
            }
            // Test multiply point cloud by a matrix
            {
                DotNet3d.Ply ply = new DotNet3d.Ply(DotNet3d.Matrix.Identity());
                DotNet3d.PointCloud pc = ply.ToPointCloud();
                Trace.Assert(pc.ToString() == "0 0 0 0 0 0 255\n500 0 0 255 0 0 255\n0 500 0 0 255 0 255\n0 0 1000 0 0 255 255\n");
                pc.Multiply(DotNet3d.Matrix.Scaling(2, 2, 2));
                Trace.Assert(pc.ToString() == "0 0 0 0 0 0 255\n1000 0 0 255 0 0 255\n0 1000 0 0 255 0 255\n0 0 2000 0 0 255 255\n");

                // test point cloud with normals
                int numPoints = 1;
                var dotNetTypes = new List<string> {"System.Single", "System.Single", "System.Single", "System.Single", "System.Single", "System.Single"};
                var names = new List<string> {"x", "y", "z", "nx", "ny", "nz" };
                int numBytes = DotNet3d.Utilities.TotalBytes(dotNetTypes);
                var data = new byte[numBytes];

                int index = 0;
                index += DotNet3d.Utilities.NumToBytes((float)1, data, index);
                index += DotNet3d.Utilities.NumToBytes((float)2, data, index);
                index += DotNet3d.Utilities.NumToBytes((float)3, data, index);
                index += DotNet3d.Utilities.NumToBytes((float)4, data, index);
                index += DotNet3d.Utilities.NumToBytes((float)5, data, index);
                index += DotNet3d.Utilities.NumToBytes((float)6, data, index);
                var pcWithNormals = new DotNet3d.PointCloud(numPoints, dotNetTypes, names, data);

                var trans = DotNet3d.Matrix.Translation(10, 10, 10);
                var scale = DotNet3d.Matrix.Scaling(2, 2, 2);
                var super = scale * trans;
                pcWithNormals.Multiply(super);
                Trace.Assert(pcWithNormals.ToString() == "22 24 26 8 10 12\n");
            }
        }
        private static void TestUtilities()
        {
            DotNet3d.Utilities.RGBMask2BGMask(@"..\..\testData\RGBMask-Cam0000-F00066.png", @"..\..\testData\Output\BGMask-Cam0000-F00066.png");
            DotNet3d.Utilities.ClampScaleImage(@"..\..\testData\center_cam1_ir.0001.png", @"..\..\testData\Output\center_cam1_ir.0001.bmp", 0, 750, 0, 255);
            {
                int p = DotNet3d.Utilities.GetPixel_16bit(@"..\..\testData\center_cam1_ir.0001.png", 146, 0);
                Trace.Assert(p == 71);
            }
            {
                int p = DotNet3d.Utilities.GetPixel_16bit(@"..\..\testData\center_cam1_ir.0001.png", 384, 443);
                Trace.Assert(p == 65535);
            }
        }
        static void Main(string[] args)
        {
            TestPoint();
            TestVector();
            TestMatrix();
            TestPly();
            TestPointCloud();
            TestUtilities();
        }
    }
}
