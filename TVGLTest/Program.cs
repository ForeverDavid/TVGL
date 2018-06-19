using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using StarMathLib;
using TVGL;
using TVGL.Boolean_Operations;
using TVGL.IOFunctions;
using TVGL.Voxelization;

namespace TVGL_Test
{
    internal class Program
    {
        private static readonly string[] FileNames = {
        //"../../../TestFiles/ABF.ply",
       // "../../../TestFiles/Beam_Boss.STL",
       // //"../../../TestFiles/bigmotor.amf",
       // //"../../../TestFiles/DxTopLevelPart2.shell",
       // //"../../../TestFiles/Candy.shell",
       // //"../../../TestFiles/amf_Cube.amf",
       // //"../../../TestFiles/train.3mf",
       // //"../../../TestFiles/Castle.3mf",
       // //"../../../TestFiles/Raspberry Pi Case.3mf",
       ////"../../../TestFiles/shark.ply",
     // "../../../TestFiles/bunnySmall.ply",
       // "../../../TestFiles/cube.ply",
       // //"../../../TestFiles/airplane.ply",
       // "../../../TestFiles/TXT - G5 support de carrosserie-1.STL.ply",
        //"../../../TestFiles/Tetrahedron.STL",
       // "../../../TestFiles/off_axis_box.STL",
       // "../../../TestFiles/Wedge.STL",
       // "../../../TestFiles/Mic_Holder_SW.stl",
       // "../../../TestFiles/Mic_Holder_JR.stl",
       // "../../../TestFiles/3_bananas.amf",
       // "../../../TestFiles/drillparts.amf",  //Edge/face relationship contains errors
       // "../../../TestFiles/wrenchsns.amf", //convex hull edge contains a concave edge outside of tolerance
       // "../../../TestFiles/Rook.amf",
        //"../../../TestFiles/hdodec.off",
        //"../../../TestFiles/tref.off",
        //"../../../TestFiles/mushroom.off",
        //"../../../TestFiles/vertcube.off",
        //"../../../TestFiles/trapezoid.4d.off",
        //"../../../TestFiles/ABF.STL",
        //"../../../TestFiles/Pump-1repair.STL",
        //"../../../TestFiles/Pump-1.STL",
        "../../../TestFiles/SquareSupportWithAdditionsForSegmentationTesting.STL",
        "../../../TestFiles/Beam_Clean.STL",
        "../../../TestFiles/Square_Support.STL",
        "../../../TestFiles/Aerospace_Beam.STL",


        "../../../TestFiles/piston.stl",
        "../../../TestFiles/Z682.stl",
        "../../../TestFiles/sth2.stl",
        "../../../TestFiles/Cuboide.stl", //Note that this is an assembly 
        "../../../TestFiles/new/5.STL",
       "../../../TestFiles/new/2.stl", //Note that this is an assembly 
        "../../../TestFiles/new/6.stl", //Note that this is an assembly  //breaks in slice at 1/2 y direction
       "../../../TestFiles/new/4.stl", //breaks because one of its faces has no normal
        "../../../TestFiles/radiobox.stl",
        "../../../TestFiles/brace.stl",  //Convex hull fails in MIconvexHull
        "../../../TestFiles/G0.stl",
        "../../../TestFiles/GKJ0.stl",
        "../../../TestFiles/testblock2.stl",
        "../../../TestFiles/Z665.stl",
        "../../../TestFiles/Casing.stl", //breaks because one of its faces has no normal
        "../../../TestFiles/mendel_extruder.stl",

       "../../../TestFiles/MV-Test files/holding-device.STL",
       "../../../TestFiles/MV-Test files/gear.STL"
        };

        [STAThread]
        private static void Main(string[] args)
        {
            //Difference2();
            var writer = new TextWriterTraceListener(Console.Out);
            Debug.Listeners.Add(writer);
            TVGL.Message.Verbosity = VerbosityLevels.OnlyCritical;

//#if AnyCPU
//            //Works for AnyCPU (x86) builds
//            var dir = new DirectoryInfo("../../../TestFiles");
//#else
//            //Works for x64 builds
//            var dir = new DirectoryInfo("../../../../TestFiles");
//#endif

            var dir = new DirectoryInfo("../../../../TestFiles");

            var fileNames = dir.GetFiles("*");
            //Casing = 17
            //SquareSupport = 70
            for (var i = 17; i < fileNames.Count(); i++)
            {
                //var filename = FileNames[i];
                var filename = fileNames[i].FullName;
                Console.WriteLine("Attempting: " + filename);
                List<TessellatedSolid> solids = IO.Open(filename);
                var silhouette = Silhouette.Run(solids[0], new[] { 0.2, 0.0, 0.8 }.normalize(), 0.1);
                //Presenter.ShowAndHang(silhouette);
                Presenter.ShowAndHang(solids[0]);
            }

            Console.WriteLine("Completed.");
            //  Console.ReadKey();
        }

        public static void TestVoxelizatoin(TessellatedSolid ts)
        {
            var stopWatch = new Stopwatch();
            //stopWatch.Restart();
            //var vs1 = new VoxelizedSolid(ts, VoxelDiscretization.ExtraCoarse);
            //stopWatch.Stop();
            //Console.WriteLine("Extra Coarse: tsvol:{0}\tvol:{1}\t#voxels:{2}\ttime{3}",
            //    ts.Volume, vs1.Volume,vs1.Count, stopWatch.Elapsed.TotalSeconds);
            //PresenterShowAndHang(new Solid[] { ts, vs1 });
            //stopWatch.Restart();
            // var vs2 = new VoxelizedSolid(ts, VoxelDiscretization.Coarse);
            //stopWatch.Stop();
            //Console.WriteLine("Extra Coarse: tsvol:{0}\tvol:{1}\t#voxels:{2}\ttime{3}",
            //    ts.Volume, vs2.Volume, vs2.Count, stopWatch.Elapsed.TotalSeconds);
            //var ts2 = (TessellatedSolid)ts1.TransformToNewSolid(new double[,]
            //  {
            //    {1,0,0,(ts1.XMax - ts1.XMin)/2},
            //    {0,1,0,(ts1.YMax-ts1.YMin)/2},
            //    {0,0,1,(ts1.ZMax-ts1.ZMin)/2},
            //  });
            //var bounds = new double[2][];
            //bounds[0] = ts1.Bounds[0];
            //bounds[1] = ts2.Bounds[1];
            var vs1 = new VoxelizedSolid(ts, VoxelDiscretization.Coarse);//, bounds);
            Console.WriteLine("displaying");
            Presenter.ShowAndHangVoxelization(ts, vs1);

            //var tsFromVS = vs1.ConvertToTessellatedSolid();
            //tsFromVS.SolidColor = new Color(KnownColors.Green) {Af = 0.5f};

            //Presenter.ShowAndHang(new [] { tsFromVS });

            //PresenterShowAndHang(new Solid[] { ts, vs2 });
            //vs1.Intersect(vs2);

            //stopWatch.Restart();
            //vs1.Draft(VoxelDirections.XPositive);
            //stopWatch.Stop();
            //Console.WriteLine(stopWatch.Elapsed.TotalSeconds);
            //Presenter.ShowAndHang(new TessellatedSolid[] { vs1.ConvertToTessellatedSolid() });

        }
        public static void TestSegmentation(TessellatedSolid ts)
        {
            var obb = MinimumEnclosure.OrientedBoundingBox(ts);
            var startTime = DateTime.Now;

            var averageNumberOfSteps = 500;

            //Do the average # of slices slices for each direction on a box (l == w == h).
            //Else, weight the average # of slices based on the average obb distance
            var obbAverageLength = (obb.Dimensions[0] + obb.Dimensions[1] + obb.Dimensions[2]) / 3;
            //Set step size to an even increment over the entire length of the solid
            var stepSize = obbAverageLength / averageNumberOfSteps;

            foreach (var direction in obb.Directions)
            {
                Dictionary<int, double> stepDistances;
                Dictionary<int, double> sortedVertexDistanceLookup;
                var segments = DirectionalDecomposition.UniformDirectionalSegmentation(ts, direction, 
                    stepSize, out stepDistances, out sortedVertexDistanceLookup);
                //foreach (var segment in segments)
                //{
                //    var vertexLists = segment.DisplaySetup(ts);
                //    Presenter.ShowVertexPathsWithSolid(vertexLists, new List<TessellatedSolid>() { ts });
                //}
            }

            // var segments = AreaDecomposition.UniformDirectionalSegmentation(ts, obb.Directions[2].multiply(-1), stepSize);
            var totalTime = DateTime.Now - startTime;
            Debug.WriteLine(totalTime.TotalMilliseconds + " Milliseconds");
            //CheckAllObjectTypes(ts, segments);
        }

        private static void CheckAllObjectTypes(TessellatedSolid ts, IEnumerable<DirectionalDecomposition.DirectionalSegment> segments)
        {
            var faces = new HashSet<PolygonalFace>(ts.Faces);
            var vertices = new HashSet<Vertex>(ts.Vertices);
            var edges = new HashSet<Edge>(ts.Edges);


            foreach (var face in faces)
            {
                face.Color = new Color(KnownColors.Gray);
            }

            foreach (var segment in segments)
            {
                foreach (var face in segment.ReferenceFaces)
                {
                    faces.Remove(face);
                }
                foreach (var edge in segment.ReferenceEdges)
                {
                    edges.Remove(edge);
                }
                foreach (var vertex in segment.ReferenceVertices)
                {
                    vertices.Remove(vertex);
                }
            }

            ts.HasUniformColor = false;
            //Turn the remaining faces red
            foreach (var face in faces)
            {
                face.Color = new Color(KnownColors.Red);
            }
            Presenter.ShowAndHang(ts);

            //Make sure that every face, edge, and vertex is accounted for
            Assert.That(!edges.Any(), "edges missed");
            Assert.That(!faces.Any(), "faces missed");
            Assert.That(!vertices.Any(), "vertices missed");
        }

        public static void TestSilhouette(TessellatedSolid ts)
        {
            var silhouette = TVGL.Silhouette.Run(ts, new[] {0.5, 0.0, 0.5});
            Presenter.ShowAndHang(silhouette);
        }

        private static void TestPolygon(TessellatedSolid ts)
        {
            ContactData contactData;
            Slice.GetContactData(ts, new Flat(10, new[] { 1.0, 0, 0 }),
                out contactData);
            throw new NotImplementedException();
        }

        private static void TestOBB(string InputDir)
        {
            var di = new DirectoryInfo(InputDir);
            var fis = di.EnumerateFiles();
            var numVertices = new List<int>();
            var data = new List<double[]>();
            foreach (var fileInfo in fis)
            {
                try
                {
                    var ts = IO.Open(fileInfo.Open(FileMode.Open), fileInfo.Name);
                    foreach (var tessellatedSolid in ts)
                    {
                        List<double> times, volumes;
                        MinimumEnclosure.OrientedBoundingBox_Test(tessellatedSolid, out times, out volumes);//, out VolumeData2);
                        data.Add(new[] { tessellatedSolid.ConvexHull.Vertices.Count(), tessellatedSolid.Volume,
                            times[0], times[1],times[2], volumes[0],  volumes[1], volumes[2] });
                    }
                }
                catch { }
            }
            // TVGLTest.ExcelInterface.PlotEachSeriesSeperately(VolumeData1, "Edge", "Angle", "Volume");
            TVGLTest.ExcelInterface.CreateNewGraph(new[] { data }, "", "Methods", "Volume", new[] { "PCA", "ChanTan" });
        }

        private static void TestSimplify(TessellatedSolid ts)
        {
            ts.Simplify(.9);
            Debug.WriteLine("number of vertices = " + ts.NumberOfVertices);
            Debug.WriteLine("number of edges = " + ts.NumberOfEdges);
            Debug.WriteLine("number of faces = " + ts.NumberOfFaces);
            TVGL.Presenter.ShowAndHang(ts);
        }

        //private static void TestClassification(TessellatedSolid ts)
        //{
        //    TesselationToPrimitives.Run(ts);
        //}

        //private static void TestOBB(TessellatedSolid ts, string filename)
        //{
        //    //var obb = MinimumEnclosure.Find_via_PCA_Approach(ts);
        //    //var obb = MinimumEnclosure.Find_via_ChanTan_AABB_Approach(ts);
        //    //var obb = MinimumEnclosure.Find_via_MC_ApproachOne(ts);\
        //    //MiscFunctions.IsConvexHullBroken(ts);
        //    List<List<double[]>> VolumeData1;
        //  //  List<List<double[]>> VolumeData2;
        //    var obb = MinimumEnclosure.OrientedBoundingBox_Test(ts, out VolumeData1);//, out VolumeData2);
        //    //var obb = MinimumEnclosure.Find_via_BM_ApproachOne(ts);
        //    //TVGLTest.ExcelInterface.PlotEachSeriesSeperately(VolumeData1, "Edge", "Angle", "Volume");
        ////   TVGLTest.ExcelInterface.CreateNewGraph(VolumeData1, "", "Methods", "Volume", new []{"PCA", "ChanTan"});
        //}

        private static void TestInsideSolid(TessellatedSolid ts1, TessellatedSolid ts2)
        {
            var now = DateTime.Now;
            Console.WriteLine("start...");
            List<Vertex> insideVertices1;
            List<Vertex> outsideVertices1;
            List<Vertex> insideVertices2;
            List<Vertex> outsideVertices2;
            MiscFunctions.FindSolidIntersections(ts2, ts1, out insideVertices1, out outsideVertices1, out insideVertices2, out outsideVertices2, true);
            //var vertexInQuestion = new Vertex(new[] {0.0, 0.0, 0.0});
            //var isVertexInside = MinimumEnclosure.IsVertexInsideSolid(ts, vertexInQuestion);
            //ToDo: Run test multiple times and look for vertices that change. Get visual and determine cause of error.
            //ToDo: Also, check if boundary function works 
            Console.WriteLine("Is the Solid inside the Solid?");
            Console.WriteLine();
            Console.WriteLine("end...Time Elapsed = " + (DateTime.Now - now));
            Console.ReadLine();
        }


        private static void TestXSections(TessellatedSolid ts)
        {
            var now = DateTime.Now;
            Debug.WriteLine("start...");
            var crossAreas = new double[3][,];
            var maxSlices = 100;
            var delta = Math.Max((ts.Bounds[1][0] - ts.Bounds[0][0]) / maxSlices,
                Math.Max((ts.Bounds[1][1] - ts.Bounds[0][1]) / maxSlices,
                    (ts.Bounds[1][2] - ts.Bounds[0][2]) / maxSlices));
            //Parallel.For(0, 3, i =>
            var greatestDeltas = new List<double>();
            var greatestDeltaLocations = new List<double>();
            var areaData = new List<List<double[]>>();
            for (int i = 0; i < 3; i++)
            {
                //var max = ts.Bounds[1][i];
                //var min = ts.Bounds[0][i];
                //var numSteps = (int)Math.Ceiling((max - min) / delta);
                var coordValues = ts.Vertices.Select(v => v.Position[i]).Distinct().OrderBy(x => x).ToList();
                var numValues = new List<double>();
                var offset = 0.000000001;
                foreach (var coordValue in coordValues)
                {
                    if (coordValues[0] == coordValue)
                    {
                        //Only Add increment forward
                        numValues.Add(coordValue + offset);
                    }
                    else if (coordValues.Last() == coordValue)
                    {
                        //Only Add increment back
                        numValues.Add(coordValue - offset);
                    }
                    else
                    {
                        //Add increment forward and back
                        numValues.Add(coordValue + offset);
                        numValues.Add(coordValue - offset);
                    }
                }
                coordValues = numValues.OrderBy(x => x).ToList();
                var numSteps = coordValues.Count;
                var direction = new double[3];
                direction[i] = 1.0;
                crossAreas[i] = new double[numSteps, 2];
                var greatestDelta = 0.0;
                var previousArea = 0.0;
                var greatestDeltaLocation = 0.0;
                var dataPoints = new List<double[]>();
                for (var j = 0; j < numSteps; j++)
                {
                    var dist = crossAreas[i][j, 0] = coordValues[j];
                    //Console.WriteLine("slice at Coord " + i + " at " + coordValues[j]);
                    var newArea = 0.0;// Slice.DefineContact(ts, new Flat(dist, direction), false).Area;
                    crossAreas[i][j, 1] = newArea;
                    if (j > 0 && Math.Abs(newArea - previousArea) > greatestDelta)
                    {
                        greatestDelta = Math.Abs(newArea - previousArea);
                        greatestDeltaLocation = dist;
                    }
                    var dataPoint = new double[] { dist, newArea };
                    dataPoints.Add(dataPoint);
                    previousArea = newArea;
                }
                areaData.Add(dataPoints);
                greatestDeltas.Add(greatestDelta);
                greatestDeltaLocations.Add(greatestDeltaLocation);
            }//);
            TVGLTest.ExcelInterface.CreateNewGraph(areaData, "Area Decomposition", "Distance From Origin", "Area");
            Debug.WriteLine("end...Time Elapsed = " + (DateTime.Now - now));

            //Console.ReadKey();
            //for (var i = 0; i < 3; i++)
            //{
            //    Debug.WriteLine("\nfor direction " + i);
            //    for (var j = 0; j < crossAreas[i].GetLength(0); j++)
            //    {
            //        Debug.WriteLine(crossAreas[i][j, 0] + ", " + crossAreas[i][j, 1]);
            //    }
            //}
        }
    }
}