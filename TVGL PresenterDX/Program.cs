using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using StarMathLib;
using TVGL;
using TVGL.Boolean_Operations;
using TVGL.IOFunctions;
using TVGL.Voxelization;


namespace TVGLPresenterDX
{
    internal class Program
    {
        private static readonly string[] FileNames = {
           //"../../../TestFiles/Binary.stl",
         //   "../../../TestFiles/ABF.ply",
          //   "../../../TestFiles/Beam_Boss.STL",
         "../../../TestFiles/Beam_Clean.STL",

        "../../../TestFiles/bigmotor.amf",
        "../../../TestFiles/DxTopLevelPart2.shell",
        "../../../TestFiles/Candy.shell",
        "../../../TestFiles/amf_Cube.amf",
        "../../../TestFiles/train.3mf",
        "../../../TestFiles/Castle.3mf",
        "../../../TestFiles/Raspberry Pi Case.3mf",
       //"../../../TestFiles/shark.ply",
       "../../../TestFiles/bunnySmall.ply",
        "../../../TestFiles/cube.ply",
        "../../../TestFiles/airplane.ply",
        "../../../TestFiles/TXT - G5 support de carrosserie-1.STL.ply",
        "../../../TestFiles/Tetrahedron.STL",
        "../../../TestFiles/off_axis_box.STL",
           "../../../TestFiles/Wedge.STL",
        "../../../TestFiles/Mic_Holder_SW.stl",
        "../../../TestFiles/Mic_Holder_JR.stl",
        "../../../TestFiles/3_bananas.amf",
        "../../../TestFiles/drillparts.amf",  //Edge/face relationship contains errors
        "../../../TestFiles/wrenchsns.amf", //convex hull edge contains a concave edge outside of tolerance
        "../../../TestFiles/hdodec.off",
        "../../../TestFiles/tref.off",
        "../../../TestFiles/mushroom.off",
        "../../../TestFiles/vertcube.off",
        "../../../TestFiles/trapezoid.4d.off",
        "../../../TestFiles/ABF.STL",
        "../../../TestFiles/Pump-1repair.STL",
        "../../../TestFiles/Pump-1.STL",
        "../../../TestFiles/SquareSupportWithAdditionsForSegmentationTesting.STL",
        "../../../TestFiles/Beam_Clean.STL",
        "../../../TestFiles/Square_Support.STL",
        "../../../TestFiles/Aerospace_Beam.STL",
        "../../../TestFiles/Rook.amf",
       "../../../TestFiles/bunny.ply",

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
            //var writer = new TextWriterTraceListener(Console.Out);
            //Debug.Listeners.Add(writer);
            //TVGL.Message.Verbosity = VerbosityLevels.OnlyCritical;
            var dir = new DirectoryInfo("../../../TestFiles");
            var fileNames = dir.GetFiles("*a*");
            for (var i = 0; i < fileNames.Count(); i++)
            {
                //var filename = FileNames[i];
                var filename = fileNames[i].FullName;
                Console.WriteLine("Attempting: " + filename);
                Stream fileStream;
                List<TessellatedSolid> ts;
                if (!File.Exists(filename)) continue;
                using (fileStream = File.OpenRead(filename))
                    ts = IO.Open(fileStream, filename);
                if (!ts.Any()) continue;
                ts[0].SolidColor = new Color(KnownColors.DeepPink);
                // PresenterShowAndHang(ts);
                TestVoxelization(ts[0]);
            }
            Console.WriteLine("Completed.");
        }


        //private static void PresenterShowAndHang(List<TessellatedSolid> ts)
        //{
        //    PresenterShowAndHang(ts.Cast<Solid>().ToList());
        //}

        private static void PresenterShowAndHang(IList<Solid> solids)
        {
            var mainWindow = new MainWindow();
            mainWindow.AddSolids(solids);
            mainWindow.ShowDialog();
        }




        public static void TestVoxelization(TessellatedSolid ts)
        {
            var stopWatch = new Stopwatch();
            //var ts2 = (TessellatedSolid)ts.TransformToNewSolid(new double[,]
            //  {
            //    {1,0,0,(ts.XMax - ts.XMin)/2},
            //    {0,1,0,(ts.YMax-ts.YMin)/2},
            //    {0,0,1,(ts.ZMax-ts.ZMin)/2},
            //  });
            //var bounds = new double[2][];
            //bounds[0] = ts.Bounds[0];//.multiply(3);
            //bounds[1] = ts2.Bounds[1];//.multiply(3);
            stopWatch.Restart();
            var vs1 = new VoxelizedSolid(ts, VoxelDiscretization.Coarse, false);  //, bounds);

            stopWatch.Stop();
            Console.WriteLine("Coarse: tsvol:{0}\tvol:{1}\t#voxels:{2}\ttime{3}",
                ts.Volume, vs1.Volume, vs1.Count, stopWatch.Elapsed.TotalSeconds);
            stopWatch.Restart();
            // PresenterShowAndHang(new Solid[] { ts, vs1 });
            // var vs2 = (VoxelizedSolid)vs1.Copy();
            //var vs2 = new VoxelizedSolid(ts2, VoxelDiscretization.Coarse, false, bounds);
            //vs1.Subtract(vs2);
            //PresenterShowAndHang(new Solid[] { vs1 });

            var vsPos = vs1.DraftToNewSolid(VoxelDirections.XPositive);
            //PresenterShowAndHang(new Solid[] { vsPos });
            var vsNeg = vs1.DraftToNewSolid(VoxelDirections.XNegative);
            //PresenterShowAndHang(new Solid[] { vsNeg });

            var vsInt = vsNeg.IntersectToNewSolid(vsPos);

            stopWatch.Stop();
            Console.WriteLine("Intersection: tsvol:{0}\tvol:{1}\ttime:{2}",
                ts.Volume, vsInt.Volume, stopWatch.Elapsed.TotalSeconds);
            PresenterShowAndHang(new Solid[] { vsInt });
        }
    }
}