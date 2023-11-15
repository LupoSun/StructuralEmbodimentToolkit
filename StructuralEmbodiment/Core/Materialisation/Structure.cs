using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructuralEmbodiment.Core.Materialisation
{
    public abstract class Structure
    {
        public List<Member> Members { get; set; }
        public List<Point3d> Supports { get; set; }
        public List<double> ForceRange { get; set; }

        public Structure(List<Member> members,List<Point3d> supports)
        {
            this.Members = members;
            this.Supports = supports;
            this.ForceRange = ComputeForceRange(this.Members);
        }

        private List<double> ComputeForceRange(List<Member> members)
        {
            if (members == null || members.Count == 0)
            {
                return new List<double> {0.0,0.0}; // Or handle empty list appropriately
            }

            double minForce = double.MaxValue;
            double maxForce = double.MinValue;

            foreach (Member member in members)
            { 
                minForce = Math.Min(minForce, member.Forces);
                maxForce = Math.Max(maxForce, member.Forces);
            }

            return new List<double> { minForce, maxForce}; 
        }


    }

    public class Member
    {
        public int CEMid { get; set; }
        public double Forces { get; set; }
        public List<Point3d> EdgeAsPoints { get; set; }
        public MemberType MemberType { get; set; }
        public EdgeType EdgeType { get; set; }

        public Member(int cemid, double forces, List<Point3d> edgeAsPoints, MemberType memberType, EdgeType edgeType)
        {
            this.CEMid = cemid;
            this.Forces = forces;
            this.EdgeAsPoints = edgeAsPoints;
            this.MemberType = memberType;
            this.EdgeType = edgeType;
        }
    }
}
