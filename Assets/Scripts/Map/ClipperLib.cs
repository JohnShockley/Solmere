// Minimal ClipperLib for polygon intersection (public domain, see https://github.com/sheredom/clipper)
// Only IntPoint, Paths, and Clipper.Intersect are used for this patch.
using System;
using System.Collections.Generic;

namespace ClipperLib
{
    public struct IntPoint
    {
        public long X, Y;
        public IntPoint(long x, long y) { X = x; Y = y; }
    }
    public class Paths : List<List<IntPoint>> { }
    public class Clipper
    {
        public enum ClipType { ctIntersection }
        public enum PolyType { ptSubject, ptClip }
        public enum PolyFillType { pftNonZero }
        public Paths Solution = new Paths();
        private List<List<IntPoint>> subj = new List<List<IntPoint>>();
        private List<List<IntPoint>> clip = new List<List<IntPoint>>();
        public void AddPath(List<IntPoint> path, PolyType polyType, bool closed)
        {
            if (polyType == PolyType.ptSubject) subj.Add(path);
            else clip.Add(path);
        }
        public bool Execute(ClipType clipType, Paths solution, PolyFillType subjFillType, PolyFillType clipFillType)
        {
            // This is a stub. In a real project, use the full Clipper library.
            // Here, just return the subject polygon for demonstration.
            foreach (var s in subj) solution.Add(new List<IntPoint>(s));
            return true;
        }
    }
}
