using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace StructuralEmbodiment.Core.Materialisation
{
    public class Terrain
    {
        public List<Brep> TerrainBreps { get; set; }
        public List<Curve> TerrainSections { get; set; }
        public Terrain()
        {
            this.TerrainBreps = new List<Brep>();
            this.TerrainSections = new List<Curve>();
        }

        public List<Curve> GenerateTerrain(
            List<Point3d> deckStartPoints, List<Point3d> deckEndPoints,
            List<Point3d> notDeckStartPoints, List<Point3d> notDeckEndPoints,
            double terrainWidth, double terrainLength, double trenchDepth, double trenchSlope,
            Vector3d x, Vector3d y, Vector3d z)
        {
            // Determine the terrain type
            Point3d averageDeckStart = Util.AveragePoint(deckStartPoints);
            Point3d averageDeckEnd = Util.AveragePoint(deckEndPoints);
            Point3d averageNotDeckStart = Util.AveragePoint(notDeckStartPoints);

            //Check if the starts are on the same side
            if (averageDeckStart.DistanceTo(averageDeckEnd) < averageDeckStart.DistanceTo(averageNotDeckStart))
            {
                Point3d temp = new Point3d(averageDeckStart);
                averageDeckStart = new Point3d(averageDeckEnd);
                averageDeckEnd = new Point3d(temp);

            }

            int terrainType = averageDeckStart.Z > averageNotDeckStart.Z ? 1 : 2;

            // Reusable transforms
            Point3d midPoint = (averageDeckStart + averageDeckEnd) / 2;
            Plane midPlane = new Plane(midPoint, x);
            Transform transformMirror = Transform.Mirror(midPlane);

            // Construct terrain
            PolylineCurve section;
            if (terrainType == 1)
            {
                var pt1 = new Point3d(averageDeckStart);
                pt1.Transform(Transform.Translation(x * terrainWidth));

                var pt2 = new Point3d(averageDeckStart);

                var pt3 = new Point3d(averageNotDeckStart);

                var pt4 = new Point3d(averageNotDeckStart);
                pt4.Transform(Transform.Translation(-z * trenchDepth - x * trenchSlope));

                var pt5 = new Point3d(pt4);
                pt5.Transform(transformMirror);

                var pt6 = new Point3d(pt3);
                pt6.Transform(transformMirror);

                var pt7 = new Point3d(pt2);
                pt7.Transform(transformMirror);

                var pt8 = new Point3d(pt1);
                pt8.Transform(transformMirror);

                var points = new List<Point3d> { pt1, pt2, pt3, pt4, pt5, pt6, pt7, pt8 };
                section = new PolylineCurve(points);
            }
            else
            {

                var pt3 = new Point3d(averageDeckStart);
                var pt2 = pt3 + (x * terrainWidth / 7);
                var pt1 = pt2 + (2 * (averageNotDeckStart - averageDeckStart) + x * terrainWidth / 10);
                var pt4 = pt3 + (-z * trenchDepth * 5);
                var pt0 = pt1 + (x * terrainWidth / 7);

                var pt5 = new Point3d(pt4);
                pt5.Transform(transformMirror);

                var pt6 = new Point3d(pt3);
                pt6.Transform(transformMirror);

                var pt7 = new Point3d(pt2);
                pt7.Transform(transformMirror);

                var pt8 = new Point3d(pt1);
                pt8.Transform(transformMirror);

                var pt9 = new Point3d(pt0);
                pt9.Transform(transformMirror);

                var points = new List<Point3d> { pt0, pt1, pt2, pt3, pt4, pt5, pt6, pt7, pt8, pt9 };
                section = new PolylineCurve(points);
            }

            // Create mirrored and translated sections
            var transformTranslation1 = Transform.Translation(y * terrainLength);
            var transformTranslation2 = Transform.Translation(-y * terrainLength);

            var section1 = section.DuplicateCurve();
            var section2 = section.DuplicateCurve();
            section1.Transform(transformTranslation1);
            section2.Transform(transformTranslation2);

            this.TerrainSections.Add(section1);
            this.TerrainSections.Add(section2);
            return new List<Curve> { section1, section2 };
        }

        public void LoftTerrain(Vector3d downDistance, double tolerance)
        {
            this.TerrainBreps = Brep.CreateFromLoft(this.TerrainSections, Point3d.Unset, Point3d.Unset, LoftType.Normal, false).ToList();

            /***
            LineCurve l1 = new LineCurve(this.TerrainSections[0].PointAtStart, this.TerrainSections[1].PointAtStart);
            LineCurve l2 = new LineCurve(this.TerrainSections[0].PointAtEnd, this.TerrainSections[1].PointAtEnd);
            Curve closedCurve = Curve.JoinCurves(new List<Curve> { l1, l2, this.TerrainSections[0], this.TerrainSections[1] }, 0.1, false)[0];
            Curve projectedCurve = Curve.ProjectToPlane(closedCurve, projectionPlane);
            this.TerrainBreps.AddRange(Brep.CreateFromLoft(new Curve[] { closedCurve, projectedCurve }, Point3d.Unset, Point3d.Unset, LoftType.Normal, false).ToList());
            this.TerrainBreps.Add(Brep.CreatePlanarBreps(projectedCurve, tolerance)[0]);
            ***/

            Point3d t1 = this.TerrainSections[0].PointAtStart;
            Point3d t2 = this.TerrainSections[1].PointAtStart;
            Point3d t3 = this.TerrainSections[1].PointAtEnd;
            Point3d t4 = this.TerrainSections[0].PointAtEnd;

            Point3d b1 = t1 + downDistance;
            Point3d b2 = t2 + downDistance;
            Point3d b3 = t3 + downDistance;
            Point3d b4 = t4 + downDistance;

            LineCurve t1t2 = new LineCurve(this.TerrainSections[0].PointAtStart, this.TerrainSections[1].PointAtStart);
            LineCurve t4t3 = new LineCurve(this.TerrainSections[0].PointAtEnd, this.TerrainSections[1].PointAtEnd);
            LineCurve t1b1 = new LineCurve(t1, b1);
            LineCurve t2b2 = new LineCurve(t2, b2);
            LineCurve t3b3 = new LineCurve(t3, b3);
            LineCurve t4b4 = new LineCurve(t4, b4);
            LineCurve b1b2 = new LineCurve(b1, b2);
            LineCurve b3b4 = new LineCurve(b3, b4);
            LineCurve b2b3 = new LineCurve(b2, b3);
            LineCurve b1b4 = new LineCurve(b1, b4);

            Curve l1 = Curve.JoinCurves(new List<Curve> { t1t2, t1b1, t2b2, b1b2 }, tolerance, false)[0];
            Curve l2 = Curve.JoinCurves(new List<Curve> { this.TerrainSections[0], t1b1, b1b4, t4b4 }, tolerance, false)[0];
            Curve l3 = Curve.JoinCurves(new List<Curve> { t4t3, t4b4, t3b3, b3b4 }, tolerance, false)[0];
            Curve l4 = Curve.JoinCurves(new List<Curve> { this.TerrainSections[1], t2b2, t3b3, b2b3 }, tolerance, false)[0];
            Curve l5 = Curve.JoinCurves(new List<Curve> { b1b2, b2b3, b3b4, b1b4 }, tolerance, false)[0];
            this.TerrainBreps.AddRange(Brep.CreatePlanarBreps(l1,tolerance));
            this.TerrainBreps.AddRange(Brep.CreatePlanarBreps(l2, tolerance));
            this.TerrainBreps.AddRange(Brep.CreatePlanarBreps(l3, tolerance));
            this.TerrainBreps.AddRange(Brep.CreatePlanarBreps(l4, tolerance));
            this.TerrainBreps.AddRange(Brep.CreatePlanarBreps(l5, tolerance));
        }

    }
}
