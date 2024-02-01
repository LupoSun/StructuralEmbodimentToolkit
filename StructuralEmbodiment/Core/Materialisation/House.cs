using Rhino.Geometry;
using StructuralEmbodiment.Core.Formfinding;
using System.Collections.Generic;
using System.Linq;

namespace StructuralEmbodiment.Core.Materialisation
{
    public class House
    {
        public Point3d AnchorPt { get; set; }
        public PolylineCurve Outline { get; set; }
        public List<Vector3d> MultiplyDirection { get; set; }
        public HouseType HouseType { get; set; }

        public House(Point3d anchorPt, PolylineCurve outline, List<Vector3d> multiplyDirection, HouseType houseType)
        {
            AnchorPt = anchorPt;
            Outline = outline;
            MultiplyDirection = multiplyDirection;
            HouseType = houseType;
        }

        public List<Brep> LoftHouse(double structureHeight, double siteHeight,bool saddleRoof,double tolerance,double angleTolerance)
        {
            var breps = new List<Brep>();
            var outlineBottom = Outline.DuplicateCurve();
            outlineBottom.Translate(Vector3d.ZAxis * -structureHeight);
            var outlineTop = outlineBottom.DuplicateCurve();
            outlineTop.Translate(Vector3d.ZAxis * siteHeight);
            var loft = Brep.CreateFromLoft(new List<Curve> { outlineBottom, outlineTop }, Point3d.Unset, Point3d.Unset, LoftType.Straight, false)[0];
            loft = loft.CapPlanarHoles(tolerance);
            breps.Add(loft);

            if (saddleRoof)
            {
                var outlineTop2 = outlineTop.DuplicateCurve();
                List<Point3d> pts = ((PolylineCurve)outlineTop2).ToPolyline().ToList();
                LineCurve line1 = new LineCurve(pts[1], pts[0]);
                LineCurve line2 = new LineCurve(pts[1], pts[2]);
                LineCurve loftBase;
                LineCurve antiLoftBase;
                if ((pts[0] - pts[1]).IsParallelTo(this.MultiplyDirection[0], tolerance) == 1 || (pts[0] - pts[1]).IsParallelTo(this.MultiplyDirection[0], tolerance) == -1)
                {
                    loftBase = line1;
                    antiLoftBase = line2;
                }else
                {
                    loftBase = line2;
                    antiLoftBase = line1;
                }
                var loftVect1 = antiLoftBase.PointAtEnd-antiLoftBase.PointAtStart;
                var loftVect2 = loftVect1/2 + Vector3d.ZAxis*siteHeight*0.35;

                var loftline1 = loftBase.DuplicateCurve();
                loftline1.Translate(loftVect1);
                var loftline2 = loftBase.DuplicateCurve();
                loftline2.Translate(loftVect2);
                var saddleRoofLoft = Brep.CreateFromLoft(new List<Curve> {loftBase, loftline1, loftline2,loftBase}, Point3d.Unset, Point3d.Unset, LoftType.Straight, false)[0];
                saddleRoofLoft = saddleRoofLoft.CapPlanarHoles(tolerance);
                breps.Add(saddleRoofLoft);
            }

            return breps;
        }

        /**
         * Multiply a certain amount of houses. Type P needs to be handled differently.
         */
        public static List<House> MultiplyHouse(House BaseHouse, int number, double distance)
        {
            var houses = new List<House>();
            foreach (var direction in BaseHouse.MultiplyDirection)
            {
                var unitDirection = new Vector3d(direction);
                unitDirection.Unitize();

                for (int i = 1; i < number; i++)
                {

                    var newHouse = BaseHouse.Clone();
                    var moveVector = direction * i + unitDirection * distance * i;
                    newHouse.AnchorPt = BaseHouse.AnchorPt + moveVector;
                    newHouse.Outline.Transform(Transform.Translation(moveVector));
                    houses.Add(newHouse);
                }
            }
            return houses;
        }

        public static List<House> CreateHousesFromBorders(List<LineCurve> borders, PolylineCurve boundary, double tolerance)
        {
            var houses = new List<House>();
            var sortedBorders = Core.Util.GroupConnectedLineCurves(borders, tolerance);
            foreach (var bordersGroup in sortedBorders)
            {
                //for the type I house, there should be only one border
                if (bordersGroup.Count == 1)
                {
                    var borderLine = bordersGroup[0];
                    var houseType = HouseType.I;
                    var multiplyDirection = new List<Vector3d> { borderLine.PointAtEnd - borderLine.PointAtStart, borderLine.PointAtStart - borderLine.PointAtEnd };

                    var tangent = borderLine.TangentAtStart;
                    var loftDirection = new Vector3d(tangent.Y, -tangent.X, 0);
                    var testVect = new Vector3d(loftDirection);
                    testVect.Unitize();
                    if (boundary.Contains(borderLine.PointAt(0.5) + testVect, Plane.WorldXY, tolerance) == PointContainment.Inside) loftDirection.Reverse();
                    loftDirection.Unitize();
                    
                    var corners = new List<Point3d> {
                        borderLine.PointAtStart,
                        borderLine.PointAtEnd,
                        borderLine.PointAtEnd + loftDirection * borderLine.GetLength()/3*2,
                        borderLine.PointAtStart + loftDirection * borderLine.GetLength()/3*2
                        };
                    var anchorPt = Core.Util.AveragePoint(corners);
                    corners.Add(borderLine.PointAtStart);
                    var outline = new PolylineCurve(corners);

                    houses.Add(new House(anchorPt, outline, multiplyDirection, houseType));
                    // for the type O house, there should be two borders
                }
                else if (bordersGroup.Count == 2)
                {
                    var joinedBorder = Curve.JoinCurves(bordersGroup)[0];
                    if (joinedBorder is PolylineCurve)
                    {
                        var polyline = ((PolylineCurve)joinedBorder).ToPolyline();
                        var pt0 = polyline[0];
                        var pt1 = polyline[1];
                        var pt2 = polyline[2];

                        var loftDirection1 = pt2 - pt1;
                        var testVect1 = new Vector3d(loftDirection1);
                        testVect1.Unitize();
                        if (boundary.Contains((pt0 + pt1) / 2 + testVect1, Plane.WorldXY, tolerance) == PointContainment.Inside) loftDirection1.Reverse();
                        var loftDirection2 = pt0 - pt1;
                        var testVect2 = new Vector3d(loftDirection2);
                        testVect2.Unitize();
                        if (boundary.Contains((pt2 + pt1) / 2 + testVect2, Plane.WorldXY, tolerance) == PointContainment.Inside) loftDirection2.Reverse();
                        var anchorPt1 = (pt0 + pt1 + (pt0 + loftDirection1) + (pt1 + loftDirection1)) / 4;
                        var anchorPt2 = (pt1 + pt2 + (pt1 + loftDirection2) + (pt2 + loftDirection2)) / 4;
                        var outline1 = new PolylineCurve(new List<Point3d> { pt0, pt1, pt1 + loftDirection1, pt0 + loftDirection1, pt0 });
                        var outline2 = new PolylineCurve(new List<Point3d> { pt1, pt2, pt2 + loftDirection2, pt1 + loftDirection2, pt1 });


                        var houseType = HouseType.O;

                        houses.Add(new House(anchorPt1, outline1, new List<Vector3d> { loftDirection1 }, houseType));
                        houses.Add(new House(anchorPt2, outline2, new List<Vector3d> { loftDirection2 }, houseType));

                    }

                }
            }
            return houses;
        }

        public House Clone()
        {
            // Creating a deep copy of the PolylineCurve if it's not null
            PolylineCurve newOutline = Outline?.DuplicateCurve() as PolylineCurve;

            // Creating a new List for MultiplyDirection
            List<Vector3d> newMultiplyDirection = new List<Vector3d>(MultiplyDirection);

            // Creating a new House object with copied values
            return new House(this.AnchorPt, newOutline, newMultiplyDirection, this.HouseType);
        }
    }
}
