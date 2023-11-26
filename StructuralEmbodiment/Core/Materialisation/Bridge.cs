using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace StructuralEmbodiment.Core.Materialisation
{
    public class Bridge : Structure
    {
        public List<Curve> DeckOutlines { get; set; }
        public List<Point3d> DeckSupports { get; set; }
        public List<Point3d> NonDeckSupports { get; set; }
        public List<Point3d> TowerSupports { get; set; }

        public Dictionary<Point3d, List<Member>> NonDeckConnectedMembersDict { get; set; }
        public Dictionary<Point3d, List<Plane>> NonDeckPlanesDict { get; set; }
        public Dictionary<Point3d, List<Curve>> NonDeckCrossSectionsDict { get; set; }

        public Bridge(List<Member> members, List<Point3d> supports, List<Point3d> deckSupport, List<Point3d> nonDeckSupport) : base(members, supports)
        {
            this.Members = members;
            this.Supports = supports;

            this.DeckSupports = deckSupport;
            this.NonDeckSupports = nonDeckSupport;

            this.NonDeckConnectedMembersDict = new Dictionary<Point3d, List<Member>>();
            this.NonDeckPlanesDict = new Dictionary<Point3d, List<Plane>>();
            this.NonDeckCrossSectionsDict = new Dictionary<Point3d, List<Curve>>();

            if (supports.Count - deckSupport.Count - nonDeckSupport.Count == 1)
            {
                TowerSupports = (List<Point3d>)supports.Except(deckSupport).ToList().Except(nonDeckSupport).ToList();
                RegisterTowerMembers();
            }
            ComputeDeckOutlines();

        }

        public void ComputeDeckOutlines()
        {
            var filteredMembers = this.Members.Where(m => m.MemberType == MemberType.Deck && m.EdgeType == EdgeType.TrailEdge).ToList();
            var lines = new List<LineCurve>();
            //Connect the individual members to form continuous outlines
            foreach (Member member in filteredMembers)
            {
                LineCurve line = new LineCurve(member.EdgeAsPoints[0], member.EdgeAsPoints[1]);
                lines.Add(line);
            }
            List<Curve> outlines = Curve.JoinCurves(lines, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance * 10).ToList();
            this.DeckOutlines = outlines;

            if (this.DeckOutlines.Count == 2)
            {
                //TODO Rework on this logic
                if (!Util.HaveSameDirection(this.DeckOutlines[0], this.DeckOutlines[1]))
                {
                    this.DeckOutlines[1].Reverse();
                }
            }
            else throw new System.Exception("Deck Outlines are not 2, try to increase the tolerance");

        }
        public void RegisterTowerMembers()
        {
            var towerMembers = new List<Member>();
            FindConnectedMembersRecursive(
                this.TowerSupports.First(),
                this.Members.Where(m => m.EdgeType == EdgeType.TrailEdge).ToList(),
                null,
                ref towerMembers);
            foreach (Member member in towerMembers)
            {
                member.MemberType = MemberType.Tower;
            }
        }

        public static Dictionary<Point3d, List<Member>> FindConncectedTrails(List<Point3d> searchPoints, List<Member> members)
        {

            var connectedMembersDict = new Dictionary<Point3d, List<Member>>();

            // Initialize dictionary with support points
            var supports = new List<Point3d>(searchPoints);
            //Util.RemoveClosestToCenter(supports);

            foreach (Point3d support in supports)
            {
                var connectedMembers = new List<Member>();
                FindConnectedMembersRecursive(support, members, null, ref connectedMembers);
                connectedMembersDict[support] = connectedMembers;
            }

            return connectedMembersDict;
        }

        private static void FindConnectedMembersRecursive(Point3d currentPoint, List<Member> membersPool, Member previousMember, ref List<Member> connectedMembers)
        {
            double tolerance = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
            foreach (var member in membersPool)
            {
                if (member.EdgeType == EdgeType.DeviationEdge) continue;
                if (member == previousMember) continue;

                if (Util.IsPointConnectedToMember(currentPoint, member, tolerance))
                {
                    connectedMembers.Add(member);
                    var nextPoint = member.EdgeAsPoints[0].EpsilonEquals(currentPoint, tolerance) ? member.EdgeAsPoints[1] : member.EdgeAsPoints[0];
                    FindConnectedMembersRecursive(nextPoint, membersPool, member, ref connectedMembers);
                    break; // Assuming one member doesn't connect to multiple members at the same point
                }
            }
        }

        public List<Plane> ComputePerpendicularPlanesAtPoints(List<Member> connectedMembers, double tolerance)
        {
            var points = new List<Point3d>();
            foreach (Member member in connectedMembers)
            {
                points.Add(member.EdgeAsPoints[0]);
                points.Add(member.EdgeAsPoints[1]);
            }

            // By increasing the tolerance by 10 times, we can remove the duplicate points as well as the ones in the middle
            points = Point3d.SortAndCullPointList(points, tolerance).ToList();

            var polyline = new Polyline(points);

            var planes = new List<Plane>();

            for (int i = 0; i < polyline.Count; i++)
            {
                Vector3d tangent = Util.ComputePolylineTangentAt(polyline, i);

                // The Y-axis vector
                Vector3d yAxis = Vector3d.YAxis;

                // The cross product gives a vector perpendicular to both tangent and Y-axis,
                // and it will be parallel to the XZ plane
                Vector3d xAxis = Vector3d.CrossProduct(tangent, yAxis);
                xAxis.Unitize();

                planes.Add(new Plane(polyline[i], xAxis, yAxis));
            }

            return planes;

        }

        public List<Brep> LoftTrails(Dictionary<Point3d, List<Member>> connectedMembersDict, int crossSection, double multiplier, Interval range, bool IncludeDeckTrails, double tolerance)
        {
            var breps = new List<Brep>();
            double trailThicknessCoeff = 0.7;

            //Post processing the dictionary bridge the trail gap
            Dictionary<Point3d, List<Member>> newDict = BridgeTrailGaps(connectedMembersDict, tolerance);
            connectedMembersDict = newDict;

            foreach (KeyValuePair<Point3d, List<Member>> kvp in connectedMembersDict)
            {
                var crossSectinos = new List<Curve>();
                var connectedMembers = kvp.Value;
                if (!IncludeDeckTrails && connectedMembers.First().MemberType == MemberType.Deck)
                {
                    continue;
                }
                var planes = ComputePerpendicularPlanesAtPoints(connectedMembers, tolerance);
                var forces = ComputeConnectedMemberCrosssection(connectedMembers);
                for (int i = 0; i < planes.Count; i++)
                {
                    var plane = planes[i];
                    var force = forces[i];
                    //Remap the unsigned force from the unsigned force range to the new range
                    double crossSectionSize = Util.ValueRemap(Math.Abs(force), this.ForceRangeUnsigned, range) * multiplier * trailThicknessCoeff;
                    if (crossSection == 0)
                    {
                        crossSectinos.Add(new Circle(plane, crossSectionSize / 2).ToNurbsCurve());
                    }
                    else if (crossSection == 1)
                    {
                        crossSectinos.Add(new Rectangle3d(plane, new Interval(-crossSectionSize / 2, crossSectionSize / 2), new Interval(-crossSectionSize / 2, crossSectionSize / 2)).ToNurbsCurve());
                    }
                }

                // Register the planes and cross sections in to the object's attributes
                if(connectedMembers.First().MemberType != MemberType.Deck) {
                    this.NonDeckConnectedMembersDict[kvp.Key] = connectedMembers;
                    this.NonDeckPlanesDict[kvp.Key] = planes;
                    this.NonDeckCrossSectionsDict[kvp.Key] = crossSectinos;
                }
                

                var brep = Brep.CreateFromLoft(crossSectinos, Point3d.Unset, Point3d.Unset, LoftType.Loose, false)[0];
                breps.Add(brep.CapPlanarHoles(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance));


            }
            return breps;
        }

        /*
         * convert the connectivity dictionary of trail pairs into a dictionary of continuous trails
         */
        public Dictionary<Point3d, List<Member>> BridgeTrailGaps(Dictionary<Point3d, List<Member>> connectedMembersDict, double tolerance)
        {
            Dictionary<Point3d, List<Member>> newDict = new Dictionary<Point3d, List<Member>>();
            List<Point3d> examined = new List<Point3d>();
            foreach (KeyValuePair<Point3d, List<Member>> kvp in connectedMembersDict)
            {
                List<Member> toAdd = new List<Member>();
                var startPt1 = kvp.Key;
                var members1 = kvp.Value;
                var endMember1 = members1.Last();
                if (examined.Contains(startPt1)) continue;
                else examined.Add(startPt1);

                foreach (KeyValuePair<Point3d, List<Member>> kvp2 in connectedMembersDict)
                {
                    var startPt2 = kvp2.Key;
                    var members2 = kvp2.Value;
                    var endMember2 = members2.Last();
                    if (Util.AreMembersConnected(endMember1, endMember2, tolerance * 10))
                    {
                        if (!examined.Contains(startPt2))
                        {
                            toAdd.AddRange(members1);
                            var temp = new List<Member>(members2);
                            temp.Reverse();
                            toAdd.AddRange(temp);
                            examined.Add(startPt2);
                        }
                    }
                }
                if (toAdd.Count > 0)
                {
                    newDict.Add(startPt1, toAdd);
                }

            }

            /* to be deleted after checking running
            //Second loop for the ending half of the outlines
            foreach (Curve deckOutline in this.DeckOutlines)
            {
                var startPoint = deckOutline.PointAtStart;
                var endPoint = deckOutline.PointAtEnd;
                foreach (KeyValuePair<Point3d, List<Member>> kvp in connectedMembersDict)
                {
                    var connectedMembers = kvp.Value;
                    var support = kvp.Key;
                    if (endPoint.DistanceTo(support) < tolerance)
                    {
                        List<Member> temp = new List<Member>(connectedMembers);
                        temp.Reverse();

                        foreach(Point3d pt in connectedMembersDict.Keys)
                        {
                            if (pt.DistanceTo(startPoint) < tolerance)
                            {
                                newDict[pt].AddRange(temp);
                            }
                        }

                        //newDict[startPoint].AddRange(temp);
                    }

                }

            }
            */
            return newDict;
        }

        public List<List<Brep>> LoftDeviations(int crossSection, double multiplier, Interval range, double tolerance)
        {
            var cables = new List<Brep>();
            var bars = new List<Brep>();
            double cableThicknessCoeff = 0.2;
            double compressionMemberCoeff = 3 * cableThicknessCoeff;
            foreach (Member m in this.Members)
            {
                //make sure the deck outlines are at their original position
                this.ComputeDeckOutlines();
                //if the member is connecting the deck outlines, skip it
                if (Util.IsMemberConnectingOutlines(m, this.DeckOutlines, tolerance)) continue;

                if (m.EdgeType == EdgeType.DeviationEdge && m.EdgeAsPoints[0].DistanceTo(m.EdgeAsPoints[1])>tolerance*10)
                {
                    //If a deviation edge is in tension
                    double crossSectionSize;
                    if (m.Force > 0)
                    {
                        m.MemberType = MemberType.Cabel;
                        crossSectionSize = Util.ValueRemap(this.ForceRange.T1, this.ForceRangeUnsigned, range);
                        crossSectionSize *= (multiplier * cableThicknessCoeff);

                        cables.Add(Brep.CreatePipe(
                            new LineCurve(m.EdgeAsPoints[0], m.EdgeAsPoints[1]),
                            crossSectionSize / 2,
                            false,
                            PipeCapMode.Round,
                            true,
                            RhinoDoc.ActiveDoc.ModelAbsoluteTolerance,
                            RhinoDoc.ActiveDoc.ModelAngleToleranceRadians)[0]);


                    } //If a deviation edge is in compression
                    else
                    {

                        //If the compresion members have smaller forces than the tension members
                        if (Math.Abs(m.Force) < Math.Abs(this.ForceRange.T1))
                        {
                            crossSectionSize = Util.ValueRemap(Math.Abs(this.ForceRange.T1), this.ForceRangeUnsigned, range);
                        }
                        else crossSectionSize = Util.ValueRemap(Math.Abs(m.Force), this.ForceRangeUnsigned, range);

                        crossSectionSize *= (multiplier * compressionMemberCoeff);

                        bars.Add(Brep.CreatePipe(
                            new LineCurve(m.EdgeAsPoints[0], m.EdgeAsPoints[1]),
                            crossSectionSize / 2,
                            false,
                            PipeCapMode.Round,
                            true,
                            RhinoDoc.ActiveDoc.ModelAbsoluteTolerance,
                            RhinoDoc.ActiveDoc.ModelAngleToleranceRadians)[0]);
                    }


                }
            }
            return new List<List<Brep>>() { cables, bars };
        }

        public List<Brep> LoftDeck(double multiplier, Interval range)
        {
            //Compute the cross section value
            double deckThicknessCoeff = 0.5;
            double absMaxForce = this.Members
                .Where(m => m.MemberType == MemberType.Deck)
                .Select(m => Math.Abs(m.Force))
                .DefaultIfEmpty(0) // This ensures a default value if the collection is empty
                .Max();
            double crossSectionSize = Util.ValueRemap(absMaxForce, this.ForceRangeUnsigned, range);
            crossSectionSize *= (multiplier * deckThicknessCoeff);

            //Move and loft the deck
            Transform move = Transform.Translation(0, 0, (crossSectionSize / 2));
            List<Curve> outlines = new List<Curve>();
            foreach (Curve outline in this.DeckOutlines)
            {
                Curve copy = outline.DuplicateCurve();
                copy.Transform(move);
                outlines.Add(copy);
            }
            List<Brep> brepVolumes = new List<Brep>();
            // Convert polylines to curves
            var loftCurves = outlines.Select(polyline => polyline.ToNurbsCurve()).ToList();

            // Create the lofted surface
            var loftedSurface = Brep.CreateFromLoft(loftCurves, Point3d.Unset, Point3d.Unset, LoftType.Normal, false);

            // Check if loft was successful
            if (loftedSurface == null || loftedSurface.Length == 0)
                return brepVolumes;

            // Extrude the loft surface
            var extrusionPath = new LineCurve(new Point3d(0, 0, 0), new Point3d(0, 0, (-crossSectionSize)));
            foreach (var face in loftedSurface[0].Faces)
            {
                Brep faceBrep = face.DuplicateFace(false);
                Brep extrudedBrep = faceBrep.Faces[0].CreateExtrusion(extrusionPath, true);
                brepVolumes.Add(extrudedBrep);
            }

            return brepVolumes;
        }

        public List<Brep> LoftDeckSmooth(double multiplier, Interval range)
        {
            //Compute the cross section value
            double deckThicknessCoeff = 0.5;
            double absMaxForce = this.Members
                .Where(m => m.MemberType == MemberType.Deck)
                .Select(m => Math.Abs(m.Force))
                .DefaultIfEmpty(0) // This ensures a default value if the collection is empty
                .Max();
            double crossSectionSize = Util.ValueRemap(absMaxForce, this.ForceRangeUnsigned, range);
            crossSectionSize *= (multiplier * deckThicknessCoeff);

            //Move and loft the deck
            Transform move = Transform.Translation(0, 0, (crossSectionSize / 2));
            List<Curve> outlines = new List<Curve>();
            foreach (Curve outline in this.DeckOutlines)
            {
                Curve copy = outline.DuplicateCurve();
                copy.Transform(move);
                List<Point3d> pts = new List<Point3d>(((PolylineCurve)copy).ToPolyline());
                Curve smooth = Curve.CreateInterpolatedCurve(pts, 3);
                outlines.Add(smooth);
            }

            // Make the deck wider
            var Pt1 = outlines[0].PointAtStart;
            var Pt2 = outlines[1].PointAtStart;
            outlines[0].Transform(Transform.Translation(((Pt1 - Pt2)/((Pt1-Pt2).Length)) * crossSectionSize));
            outlines[1].Transform(Transform.Translation(-((Pt1 - Pt2) / ((Pt1 - Pt2).Length)) * crossSectionSize));

            List<Brep> brepVolumes = new List<Brep>();
            // Convert polylines to curves
            var loftCurves = outlines.Select(polyline => polyline.ToNurbsCurve()).ToList();

            // Create the lofted surface
            var loftedSurface = Brep.CreateFromLoft(loftCurves, Point3d.Unset, Point3d.Unset, LoftType.Normal, false);

            // Check if loft was successful
            if (loftedSurface == null || loftedSurface.Length == 0)
                return brepVolumes;

            // Extrude the loft surface
            var extrusionPath = new LineCurve(new Point3d(0, 0, 0), new Point3d(0, 0, (-crossSectionSize)));
            foreach (var face in loftedSurface[0].Faces)
            {
                Brep faceBrep = face.DuplicateFace(false);
                Brep extrudedBrep = faceBrep.Faces[0].CreateExtrusion(extrusionPath, true);
                brepVolumes.Add(extrudedBrep);
            }

            return brepVolumes;
        }

        public List<Brep> LoftTower(int crossSection, double multiplier, Interval range, double tolerance)
        {
            List<Brep> breps = new List<Brep>();
            List<Member> towerMembers = this.Members.Where(m => m.MemberType == MemberType.Tower).ToList();
            double towerThicknessCoeff = 1.5;

            List<Plane> planes = ComputePerpendicularPlanesAtPoints(towerMembers, tolerance);
            List<double> forces = ComputeConnectedMemberCrosssection(towerMembers);

            List<Curve> crossSectinos = new List<Curve>();
            for (int i = 0; i < planes.Count; i++)
            {
                var plane = planes[i];
                var force = forces[i];
                //Remap the unsigned force from the unsigned force range to the new range
                double crossSectionSize = Util.ValueRemap(Math.Abs(force), this.ForceRangeUnsigned, range) * multiplier * towerThicknessCoeff;
                if (crossSection == 0)
                {
                    crossSectinos.Add(new Circle(plane, crossSectionSize / 2).ToNurbsCurve());
                }
                else if (crossSection == 1)
                {
                    crossSectinos.Add(new Rectangle3d(plane, new Interval(-crossSectionSize / 2, crossSectionSize / 2), new Interval(-crossSectionSize / 2, crossSectionSize / 2)).ToNurbsCurve());
                }
            }

            var brep = Brep.CreateFromLoft(crossSectinos, Point3d.Unset, Point3d.Unset, LoftType.Tight, false)[0];
            breps.Add(brep.CapPlanarHoles(RhinoDoc.ActiveDoc.ModelAbsoluteTolerance));

            return breps;
        }

        /*
         * <summary>
         * [Needs to go private]Computes the cross-sectional forces for each member in the connected members list.
         * </summary>
         * <param name="connectedMembers">The list of connected members.</param>
         * <returns>A list of cross-sectional forces.</returns>
         */
        public List<double> ComputeConnectedMemberCrosssection(List<Member> connectedMembers)
        {
            var crossSectionalForces = new List<double>();

            if (connectedMembers == null || !connectedMembers.Any())
            {
                return crossSectionalForces; // Return empty list if there are no members
            }

            // Add the force of the first member
            crossSectionalForces.Add(connectedMembers.First().Force);

            // Handle intermediate members
            for (int i = 0; i < connectedMembers.Count - 1; i++)
            {
                double currentForce = connectedMembers[i].Force;
                double nextForce = connectedMembers[i + 1].Force;
                double maxForce = Math.Max(currentForce, nextForce);

                crossSectionalForces.Add(maxForce);
            }

            // Add the force of the last member
            crossSectionalForces.Add(connectedMembers.Last().Force);


            return crossSectionalForces;
        }


        public override string ToString()
        {

            return this.GetType().ToString();
        }

    }
}
