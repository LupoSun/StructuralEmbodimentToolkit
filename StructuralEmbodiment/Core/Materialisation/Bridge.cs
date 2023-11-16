using Rhino.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace StructuralEmbodiment.Core.Materialisation
{
    public class Bridge : Structure
    {
        public List<Curve> DeckOutlines { get; set; }
        public List<Point3d> DeckSupports { get; set; }
        public List<Point3d> NonDeckSupports { get; set; }
        public List<Point3d> TowerSupports { get; set; }

        public Bridge(List<Member> members, List<Point3d> supports, List<Point3d> deckSupport, List<Point3d> nonDeckSupport) : base(members, supports)
        {
            this.Members = members;
            this.Supports = supports;

            this.DeckSupports = deckSupport;
            this.NonDeckSupports = nonDeckSupport;

            if (supports.Count - deckSupport.Count - nonDeckSupport.Count == 1)
            {
                TowerSupports = (List<Point3d>)supports.Except(deckSupport).Except(nonDeckSupport);
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
                if (!Util.HaveSameDirection(this.DeckOutlines[0], this.DeckOutlines[1]))
                {
                    this.DeckOutlines[1].Reverse();
                }
            } else throw new System.Exception("Deck Outlines are not 2, try to increase the tolerance");


        }

        
        public override string ToString()
        {
          
        return this.GetType().ToString();
        }

    }
}
