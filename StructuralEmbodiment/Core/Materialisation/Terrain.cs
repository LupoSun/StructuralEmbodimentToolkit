using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using StructuralEmbodiment.Components.Materialisation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StructuralEmbodiment.Core.Materialisation
{
    public class Terrain
    {
        public List<Brep> TerrainBreps { get; set; }
        public List<Curve> TerrainSections { get; set; }
        public TerrainType TerrainType { get; set; }
        public List<Brep> TreeArea { get; set; }
        public List<Brep> River { get; set; }

        public Terrain()
        {
            this.TerrainBreps = new List<Brep>();
            this.TerrainSections = new List<Curve>();
            this.TerrainType = TerrainType.Unset;
            this.TreeArea = new List<Brep>();
            this.River = new List<Brep>();

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
                this.TerrainType = TerrainType.Platau;
                var pt0 = new Point3d(averageDeckStart);
                pt0.Transform(Transform.Translation(x * terrainWidth));

                var pt1 = new Point3d(averageDeckStart);

                double zOffset = Math.Abs(averageDeckStart.Z - averageNotDeckStart.Z)/5;
                var pt2 = new Point3d(averageNotDeckStart);
                pt2.Transform(Transform.Translation(-z * zOffset));

                var pt3 = new Point3d(pt2);
                pt3.Transform(Transform.Translation(-x * trenchDepth));
                //Ver1 Lagacy
                //var pt4 = new Point3d(averageNotDeckStart);
                //pt4.Transform(Transform.Translation(-z * trenchDepth - x * trenchSlope));

                var pt4 = new Point3d(pt3);
                pt4.Transform(transformMirror);

                var pt5 = new Point3d(pt2);
                pt5.Transform(transformMirror);

                var pt6 = new Point3d(pt1);
                pt6.Transform(transformMirror);

                var pt7 = new Point3d(pt0);
                pt7.Transform(transformMirror);

                var points = new List<Point3d> { pt0, pt1, pt2, pt3, pt4, pt5, pt6, pt7 };
                section = new PolylineCurve(points);
            }
            else
            {
                this.TerrainType = TerrainType.Valley;
                var pt3 = new Point3d(averageDeckStart);
                var pt2 = pt3 + (x * terrainWidth / 5);
                var pt1 = pt2 + (3 * (averageNotDeckStart - averageDeckStart) + x * terrainWidth / 10);
                var pt4 = pt3 + (-z * trenchDepth/10);
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

                //readjust the river bed to have the slope on the sides
                //pt4 += (pt3 - pt2);
                //pt5 += (pt6 - pt7);

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

        public void LoftTerrain(double baseThickness, double tolerance)
        {
            if (this.TerrainSections.Count == 0) throw new System.Exception("No terrain sections to begin with");

            this.TerrainBreps = Brep.CreateFromLoft(this.TerrainSections, Point3d.Unset, Point3d.Unset, LoftType.Straight, false).ToList();
            List<Point3d> sectionPts = new List<Point3d>(((PolylineCurve)this.TerrainSections[0]).ToPolyline());
            Point3d lowestPt = sectionPts.MinBy(pt => pt.Z);
            Point3d highestpt = sectionPts.MaxBy(pt => pt.Z);
            Vector3d downDistance = -Vector3d.ZAxis * (highestpt.Z - lowestPt.Z + baseThickness);



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
            this.TerrainBreps.AddRange(Brep.CreatePlanarBreps(l1, tolerance));
            this.TerrainBreps.AddRange(Brep.CreatePlanarBreps(l2, tolerance));
            this.TerrainBreps.AddRange(Brep.CreatePlanarBreps(l3, tolerance));
            this.TerrainBreps.AddRange(Brep.CreatePlanarBreps(l4, tolerance));
            this.TerrainBreps.AddRange(Brep.CreatePlanarBreps(l5, tolerance));

        }

        public List<Brep> BridgeNonDeckExtension(Bridge bridge, double tolerance)
        {
            var breps = new List<Brep>();


            foreach (KeyValuePair<Point3d, List<Member>> kvp in bridge.NonDeckConnectedMembersDict)
            {
                var crossSectionBreps = Brep.CreateFromLoft(this.TerrainSections, Point3d.Unset, Point3d.Unset, LoftType.Straight, false).ToList();

                Point3d pt = kvp.Key;
                List<Member> nonDeckMembers = kvp.Value;
                List<Plane> planes = bridge.NonDeckPlanesDict[pt];
                List<Curve> crossSections = bridge.NonDeckCrossSectionsDict[pt];

                var lines = new List<LineCurve>();
                //Connect the individual members to form continuous outlines
                foreach (Member member in nonDeckMembers)
                {
                    LineCurve line = new LineCurve(member.EdgeAsPoints[0], member.EdgeAsPoints[1]);
                    lines.Add(line);
                }
                List<Curve> nonDeckOutline = Curve.JoinCurves(lines, tolerance * 10).ToList();
                if (nonDeckOutline.Count > 1)
                {
                    throw new Exception("Non deck outline is not continuous, please increase the tolerance");
                }
                PolylineCurve nonDeckPolyline = (PolylineCurve)nonDeckOutline[0];


                // Get the first and last points of the polyline
                var firstPoint = nonDeckMembers.First().EdgeAsPoints.Last();
                var lastPoint = nonDeckMembers.Last().EdgeAsPoints.Last();

                // Get the second and the second-to-last points to calculate the tangents
                var secondPoint = nonDeckMembers.First().EdgeAsPoints.First();
                var secondToLastPoint = nonDeckMembers.Last().EdgeAsPoints.First();

                // Calculate tangent vectors
                var startTangent = firstPoint - secondPoint;
                startTangent.Unitize();
                var endTangent = lastPoint - secondToLastPoint;
                endTangent.Unitize();

                var firstPointEnd = Intersection.ProjectPointsToBreps(crossSectionBreps, new Point3d[] { firstPoint }, -startTangent, tolerance)[0];
                var lastPointEnd = Intersection.ProjectPointsToBreps(crossSectionBreps, new Point3d[] { lastPoint }, -endTangent, tolerance)[0];
                double fistExtensionLength = firstPoint.DistanceTo(firstPointEnd);
                double lastExtensionLength = lastPoint.DistanceTo(lastPointEnd);
                var firstCrossSection = crossSections.First();
                var lastCrossSection = crossSections.Last();


                breps.Add(Extrusion.Create(firstCrossSection, new Plane(firstPoint, startTangent), fistExtensionLength, true).ToBrep());
                breps.Add(Extrusion.Create(lastCrossSection, new Plane(lastPoint, endTangent), lastExtensionLength, true).ToBrep());

            }

            return breps;
        }
        public void AddTerrainAssets(Bridge bridge, double tolerance)
        {
            var deckSupports = bridge.DeckSupports;
            var riverSections = new List<Curve>();

            //Check the terrain type
            if (this.TerrainType == TerrainType.Unset) throw new Exception("Terrain type is not set");

            foreach (Curve section in this.TerrainSections)
            {
                //Compute the tree area
                var section_plc = section as PolylineCurve;
                Point3d tpt1 = Point3d.Unset;
                if (this.TerrainType == TerrainType.Platau) tpt1 = section_plc.Point(1);
                else if (this.TerrainType == TerrainType.Valley) tpt1 = section_plc.Point(3);

                Point3d tpt1_target = deckSupports.OrderBy(p => p.DistanceTo(tpt1)).FirstOrDefault();
                Vector3d vect1 = tpt1_target - tpt1;
                Transform transform = Transform.Translation(vect1);
                LineCurve line1 = null;
                LineCurve line2 = null;
                if (this.TerrainType == TerrainType.Platau) line1 = new LineCurve(section_plc.PointAtStart, tpt1);
                else if (this.TerrainType == TerrainType.Valley) line1 = new LineCurve(section_plc.Point(2), section_plc.Point(3));
                LineCurve line1_target = new LineCurve(line1);
                line1_target.Transform(transform);

                if (this.TerrainType == TerrainType.Platau) line2 = new LineCurve(section.PointAtEnd, section_plc.Point(section_plc.PointCount - 2));
                else if (this.TerrainType == TerrainType.Valley) line2 = new LineCurve(section_plc.Point(section_plc.PointCount - 3), section_plc.Point(section_plc.PointCount - 4));
                LineCurve line2_target = new LineCurve(line2);
                line2_target.Transform(transform);
                this.TreeArea.AddRange(Brep.CreateFromLoft(new List<Curve> { line1, line1_target }, Point3d.Unset, Point3d.Unset, LoftType.Straight, false).ToList());
                this.TreeArea.AddRange(Brep.CreateFromLoft(new List<Curve> { line2, line2_target }, Point3d.Unset, Point3d.Unset, LoftType.Straight, false).ToList());



                //Compute the river
                LineCurve crv1 = null;
                LineCurve crv2 = null;
                double lengthDivider = 0.5;
                if (this.TerrainType == TerrainType.Platau)
                {
                    lengthDivider = 10.0;
                    crv1 = new LineCurve(section_plc.Point(2), section_plc.Point(1));
                    crv2 = new LineCurve(section_plc.Point(section_plc.PointCount - 3), section_plc.Point(section_plc.PointCount - 2));
                }
                else if (this.TerrainType == TerrainType.Valley)
                {
                    lengthDivider = 2.0;
                    crv1 = new LineCurve(section_plc.Point(4), section_plc.Point(3));
                    crv2 = new LineCurve(section_plc.Point(section_plc.PointCount - 5), section_plc.Point(section_plc.PointCount - 4));
                }
                
                double t;
                if (crv1.LengthParameter(crv1.GetLength() * (1.0 / lengthDivider), out t))
                {
                    Point3d pointAtT1 = crv1.PointAt(t);
                    Point3d pointAtT2 = crv2.PointAt(t);

                    if (this.TerrainType == TerrainType.Platau) riverSections.Add(new PolylineCurve(new Point3d[] { pointAtT1,
                            section_plc.Point(2),section_plc.Point(3), section_plc.Point(4),
                            section_plc.Point(5),pointAtT2, pointAtT1}));
                    else if (this.TerrainType == TerrainType.Valley) riverSections.Add(new PolylineCurve(new Point3d[] { pointAtT1,
                        section_plc.Point(4), section_plc.Point(5), pointAtT2, pointAtT1}));
                }
            }
            var riverNotCapped = Brep.CreateFromLoft(riverSections, Point3d.Unset, Point3d.Unset, LoftType.Straight, false).ToList();
            //this.River.AddRange(riverNotCapped);
            
            foreach (Brep brep in riverNotCapped)
            {
                var cap = brep.CapPlanarHoles(tolerance);
                if (cap != null) this.River.Add(cap);
            }

        }

        public void AddTerrainAssetsOG(Bridge bridge, double tolerance)
        {
            var deckSupports = bridge.DeckSupports;
            var riverSections = new List<Curve>();

            //Check the terrain type
            if (this.TerrainType == TerrainType.Unset) throw new Exception("Terrain type is not set");
            if (this.TerrainType == TerrainType.Platau)
            {
                foreach (Curve section in this.TerrainSections)
                {
                    //Compute the tree area
                    var section_plc = section as PolylineCurve;
                    Point3d tpt1 = section_plc.Point(1);
                    Point3d tpt1_target = deckSupports.OrderBy(p => p.DistanceTo(tpt1)).FirstOrDefault();
                    Vector3d vect1 = tpt1_target - tpt1;
                    Transform transform = Transform.Translation(vect1);
                    LineCurve line1 = new LineCurve(section.PointAtStart, tpt1);
                    LineCurve line1_target = new LineCurve(line1);
                    line1_target.Transform(transform);

                    LineCurve line2 = new LineCurve(section.PointAtEnd, section_plc.Point(section_plc.PointCount - 2));
                    LineCurve line2_target = new LineCurve(line2);
                    line2_target.Transform(transform);
                    this.TreeArea.AddRange(Brep.CreateFromLoft(new List<Curve> { line1, line1_target }, Point3d.Unset, Point3d.Unset, LoftType.Straight, false).ToList());
                    this.TreeArea.AddRange(Brep.CreateFromLoft(new List<Curve> { line2, line2_target }, Point3d.Unset, Point3d.Unset, LoftType.Straight, false).ToList());

                    //Compute the river
                    LineCurve crv1 = new LineCurve(section_plc.Point(2), section_plc.Point(1));
                    LineCurve crv2 = new LineCurve(section_plc.Point(section_plc.PointCount - 3), section_plc.Point(section_plc.PointCount - 2));
                    double t;
                    if (crv1.LengthParameter(crv1.GetLength() * (1.0 / 4.0), out t))
                    {
                        Point3d pointAtOneThird1 = crv1.PointAt(t);
                        Point3d pointAtTwoThird2 = crv2.PointAt(t);

                        riverSections.Add(new PolylineCurve(new Point3d[] { pointAtOneThird1,
                            section_plc.Point(2),section_plc.Point(3), section_plc.Point(4),
                            section_plc.Point(5),pointAtTwoThird2, pointAtOneThird1}));
                    }
                }
                var riverNotCapped = Brep.CreateFromLoft(riverSections, Point3d.Unset, Point3d.Unset, LoftType.Straight, false).ToList();

                foreach (Brep brep in riverNotCapped)
                {
                    var cap = brep.CapPlanarHoles(tolerance);
                    if (cap != null) this.River.Add(cap);
                }

            }
            else if (this.TerrainType == TerrainType.Valley)
            {

            }
        }
    }
}
