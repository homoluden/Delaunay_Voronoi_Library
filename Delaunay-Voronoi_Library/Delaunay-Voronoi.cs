﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Delaunay_Voronoi_Library
{
    /// <summary>
    /// Delaunay_Voronoi class constructs Delaunay triangulation and Voronoi diagram.
    /// </summary>
    public class Delaunay_Voronoi
    {

        #region _fields

        private List<Vertex> vinit = new List<Vertex>();
        private HashSet<Vertex> vertices = new HashSet<Vertex>();
        private HashSet<triangle> triangles = new HashSet<triangle>();
        private List<VoronoiCell> pol = new List<VoronoiCell>();
        private HashSet<triangle> newtriangles = new HashSet<triangle>();
        private List<triangle> pseuvotr = new List<triangle>();
        private List<Vertex> pseuvovert = new List<Vertex>();
        private HashSet<VoronoiCell> pseudopol = new HashSet<VoronoiCell>();

        #endregion _fields

        #region _constructors
        /// <summary>
        /// Initializes a new instance of the class Delaunay_Voronoi_Library.Delaunay_Voronoi with the specified initial Delaunay_Voronoi class.
        /// </summary>
        /// <param name="delaunay_voronoi">The Delaunay_Voronoi class.</param>
        public Delaunay_Voronoi(Delaunay_Voronoi delaunay_voronoi)
        {
            Dictionary<Vertex, Vertex> dictionary_parallel_copy = new Dictionary<Vertex, Vertex>();
            
            foreach (var w in delaunay_voronoi.GetVertices)
            {
                Vertex parallel_copy = new Vertex(w);
                dictionary_parallel_copy.Add(w, parallel_copy);
                this.GetVertices.Add(parallel_copy);
            }

            foreach (var w in delaunay_voronoi.triangles)
            {
                Vertex vertex1, vertex2, vertex3;
                
                dictionary_parallel_copy.TryGetValue(w.GetVertices[0], out vertex1);
                dictionary_parallel_copy.TryGetValue(w.GetVertices[1], out vertex2);
                dictionary_parallel_copy.TryGetValue(w.GetVertices[2], out vertex3);
                
                this.triangles.Add(new triangle(vertex1,vertex2,vertex3));
            }

            foreach (var w in delaunay_voronoi.pol)
            {
                Vertex vertex;
                dictionary_parallel_copy.TryGetValue(w.GetCellCentr, out vertex);
                VoronoiCell newvc = new VoronoiCell(w,vertex);
                vertex.Voronoi_Cell = newvc;
                pol.Add(newvc);
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the class Delaunay_Voronoi_Library.Delaunay_Voronoi with the specified vertices list.
        /// </summary>
        /// <param name="vertices">The vertices list.</param>
        public Delaunay_Voronoi(HashSet<Vertex> vertices)
        {
            vinit.Add(new Vertex(0,90));
            vinit.Add(new Vertex(0,-45));
            vinit.Add(new Vertex(120,-45));
            vinit.Add(new Vertex(240,-45));

            triangles.Add(new triangle(vinit[0], vinit[1], vinit[2]));
            triangles.Add(new triangle(vinit[0], vinit[1], vinit[3]));
            triangles.Add(new triangle(vinit[0], vinit[2], vinit[3]));
            triangles.Add(new triangle(vinit[1], vinit[2], vinit[3]));
            
            var df = DateTime.Now;
            foreach (var j in vertices)
            {
                addnewpoint(j);
            }
            Console.WriteLine("add {0}",DateTime.Now - df);
            //condition is sufficient but not necessary
            bool result = true;
            foreach (var w in vinit)
            foreach (var e in vinit)
            {
                foreach (var r in w.GetAdjacentTriangles)
                {
                    if ((r.GetVertices.Contains(e)) && (w != e))
                    {
                        result = false;
                    }
                }
            }
            if (result == false)
            {
                
                triangles = null;
                this.vertices = null;
                vinit = null;
                Console.WriteLine("Triangulation can not be built: Insufficient number of control points...");
                return;
            }
            //

            foreach (var e in vinit)
                DeletePoint(e);

            vinit.Clear();

            Voronoi();

        }

        #endregion _constructors

        #region _properties
        /// <summary>
        /// Gets the voronoi cells.
        /// </summary>
        /// <value>The voronoi cells.</value>
        public List<VoronoiCell> GetVoronoiCells
        {
            get
            {
                return pol;
            }
        }
        /// <summary>
        /// Gets the triangles list.
        /// </summary>
        /// <value>The triangles.</value>
        public HashSet<triangle> GetTriangles
        {
            get
            {
                return triangles;
            }
        }
        /// <summary>
        /// Gets the vertices list.
        /// </summary>
        /// <value>The vertices list.</value>
        public HashSet<Vertex> GetVertices
        {
            get
            {
                return vertices;
            }
        }

        #endregion _properties

        #region _private_methods
        /// <summary>
        /// Constructs voronoi diagram from delaunay triangulation.
        /// </summary>
        void Voronoi()
        {
            
            Vertex y0;
            foreach (var q in vertices)
            {
                List<Vertex> temp = new List<Vertex>();
                triangle t = q.GetAdjacentTriangles[0];
                Vertex y = t.GetVertices.First(a => a != q);
                
                for (int i = 0; i < q.GetAdjacentTriangles.Count; i++)
                {
                    temp.Add(t.GetOR);
                    y0 = y;
                    y = t.GetVertices.Single(a => (a != y) && (a != q));
                    t = q.GetAdjacentTriangles.Single(a => a.GetVertices.Contains(y) && (!a.GetVertices.Contains(y0)));
                }
                
                VoronoiCell p = new VoronoiCell(q, temp);
                pol.Add(p);
                q.Voronoi_Cell = p;
            }
        }

        int addnewpoint(Vertex x, bool p = true)
        {
            foreach (var w in vinit.Where(a => Math.Abs(x.X - a.X) < 0.0000001 && Math.Abs(x.Y - a.Y) < 0.0000001 && Math.Abs(x.Z - a.Z) < 0.0000001))
            {
                vertices.Add(w);
                w.SetNewPosition(x.Longitude, x.Latitude);
                vinit.Remove(w);
                return 0;
            }

            if (vertices.Count > 0)
            {
                var tm = vertices.ElementAt(0);
                double dis = (tm.X - x.X) * (tm.X - x.X) + (tm.Y - x.Y) * (tm.Y - x.Y) + (tm.Z - x.Z) * (tm.Z - x.Z), dis0 = 0;
                Vertex ind0 = tm;
                do
                {
                    tm = ind0;
                    foreach (var er in tm.GetAdjacentTriangles)
                    {
                        foreach (var t in er.GetVertices)
                        {
                            dis0 = (t.X - x.X) * (t.X - x.X) + (t.Y - x.Y) * (t.Y - x.Y) + (t.Z - x.Z) * (t.Z - x.Z);
                            if (dis0 < dis)
                            {
                                dis = dis0;
                                ind0 = t;
                            }
                        }
                    }
                } while (tm != ind0);

                if (dis < 0.00000001)
                {
                    if (p == false)
                    {
                        x.Value = ind0.Value;
                    }
                    else { Console.WriteLine("Warning: SingularPoint"); }
                    return -1;
                }
            }

            triangle z = null;
            triangle l = triangles.ElementAt(0);

            if (Math.Sign(l.SignVertex(x)) == l.GetSignCenter)
            {
                l.TempIndex = x;

                double e0 = (l.GetVertices[0].X - x.X) * (l.GetVertices[0].X - x.X) + (l.GetVertices[0].Y - x.Y) * (l.GetVertices[0].Y - x.Y) + (l.GetVertices[0].Z - x.Z) * (l.GetVertices[0].Z - x.Z);
                double e1 = (l.GetVertices[1].X - x.X) * (l.GetVertices[1].X - x.X) + (l.GetVertices[1].Y - x.Y) * (l.GetVertices[1].Y - x.Y) + (l.GetVertices[1].Z - x.Z) * (l.GetVertices[1].Z - x.Z);
                double e2 = (l.GetVertices[2].X - x.X) * (l.GetVertices[2].X - x.X) + (l.GetVertices[2].Y - x.Y) * (l.GetVertices[2].Y - x.Y) + (l.GetVertices[2].Z - x.Z) * (l.GetVertices[2].Z - x.Z);

                int u = 0;
                if ((e0 >= e1) && (e2 >= e1))
                {
                    u = 1;
                }

                if ((e0 >= e2) && (e1 >= e2))
                {
                    u = 2;
                }

                while (true)
                {
                    z = null;
                    foreach (var et in l.GetVertices[u].GetAdjacentTriangles)
                    {
                        if ((et.TempIndex != x) && (Math.Sign(et.SignVertex(x)) != et.GetSignCenter))
                        {
                            z = et;
                            break;
                        }
                    }

                    if (z != null) break;

                    triangle t0 = null;
                    int u0 = 0;
                    double e = 5;
                    foreach (var et in l.GetVertices[u].GetAdjacentTriangles)
                    {
                        if (et.TempIndex != x)
                        {
                            et.TempIndex = x;
                            e0 = (et.GetVertices[0].X - x.X) * (et.GetVertices[0].X - x.X) + (et.GetVertices[0].Y - x.Y) * (et.GetVertices[0].Y - x.Y) + (et.GetVertices[0].Z - x.Z) * (et.GetVertices[0].Z - x.Z);
                            e1 = (et.GetVertices[1].X - x.X) * (et.GetVertices[1].X - x.X) + (et.GetVertices[1].Y - x.Y) * (et.GetVertices[1].Y - x.Y) + (et.GetVertices[1].Z - x.Z) * (et.GetVertices[1].Z - x.Z);
                            e2 = (et.GetVertices[2].X - x.X) * (et.GetVertices[2].X - x.X) + (et.GetVertices[2].Y - x.Y) * (et.GetVertices[2].Y - x.Y) + (et.GetVertices[2].Z - x.Z) * (et.GetVertices[2].Z - x.Z);

                            if ((e >= e0) && (e1 >= e0) && (e2 >= e0))
                            {
                                e = e0;
                                t0 = et;
                                u0 = 0;
                            }

                            if ((e >= e1) && (e2 >= e1) && (e0 >= e1))
                            {
                                e = e1;
                                t0 = et;
                                u0 = 1;
                            }

                            if ((e >= e2) && (e0 >= e2) && (e1 >= e2))
                            {
                                e = e2;
                                t0 = et;
                                u0 = 2;
                            }
                        }
                    }
                    l = t0;
                    u = u0;
                }
                l = z;
            }

            HashSet<triangle> m = new HashSet<triangle>();

            anymore(m, l, x);

            if (p == false)
            {
                foreach (var d in m)
                    pseuvotr.Add(new triangle(d.GetVertices[0], d.GetVertices[1], d.GetVertices[2], false));
            }

            List<Edge> b = new List<Edge>();

            foreach (var t in m)
            {
                if (b.RemoveAll(a => a.GetVertexes.Contains(t.GetVertices[0]) && a.GetVertexes.Contains(t.GetVertices[1])) == 0)
                {
                    b.Add(new Edge(t.GetVertices[0], t.GetVertices[1]));
                }
                if (b.RemoveAll(a => a.GetVertexes.Contains(t.GetVertices[2]) && a.GetVertexes.Contains(t.GetVertices[1])) == 0)
                {
                    b.Add(new Edge(t.GetVertices[2], t.GetVertices[1]));
                }
                if (b.RemoveAll(a => a.GetVertexes.Contains(t.GetVertices[2]) && a.GetVertexes.Contains(t.GetVertices[0])) == 0)
                {
                    b.Add(new Edge(t.GetVertices[2], t.GetVertices[0]));
                }
                t.deltr();
                triangles.Remove(t);
            }
            foreach (var q in b)
            {
                var tem = new triangle(x, q.GetVertexes[0], q.GetVertexes[1]);
                if (p == false) newtriangles.Add(tem);
                triangles.Add(tem);
            }

            vertices.Add(x);
            return 1;
        }

        void anymore(HashSet<triangle> m, triangle l, Vertex x)
        {
            foreach (var w in l.GetVertices)
            {
                foreach (var e in w.GetAdjacentTriangles)
                {
                    if (Math.Sign(e.SignVertex(x)) != e.GetSignCenter)
                    {
                        if (!m.Contains(e))
                        {
                            m.Add(e);
                            anymore(m, e, x);
                        }
                    }
                }
            }
        }

        void DeletePoint(Vertex v, bool p = true)
        {
            List<Edge> edges = new List<Edge>();
            List<Vertex> verts = new List<Vertex>();
            List<triangle> triangls = new List<triangle>();

            vertices.Remove(v);
            foreach (var a in v.GetAdjacentTriangles)
            {
                triangls.Add(a);
                a.GetVertices.Remove(v);
                foreach (var q0 in a.GetVertices) q0.GetAdjacentTriangles.Remove(a);
                edges.Add(new Edge(a.GetVertices[0], a.GetVertices[1]));
                foreach (var l in a.GetVertices) if (!verts.Contains(l)) verts.Add(l);
            }

            v.GetAdjacentTriangles.Clear();

            pseuvovert = new List<Vertex>(verts);
            foreach (var r in triangls)
                triangles.Remove(r);

            triangls.Clear();

            Vertex v1 = verts[0],
                   v2 = edges.First(a => a.GetVertexes.Contains(v1)).GetVertexes.Single(b => b != v1),
                   v3 = edges.Single(a => (a.GetVertexes.Contains(v2)) && (!a.GetVertexes.Contains(v1))).GetVertexes.Single(b => b != v2);

            while (verts.Count > 3)
            {
                triangle t1 = new triangle(v1, v2, v3, true);
                bool y = true;
                foreach (var v4 in vertices)
                {
                    int c = Math.Sign(t1.SignVertex(v4));
                    if ((c != 0) && (c != t1.GetSignCenter))
                    {
                        y = false;
                        break;
                    }
                }

                if (y == true)
                {
                    var g = new triangle(v1, v3, new Vertex(new double[] { 0, 0, 0 }), false);
                    if (Math.Sign(g.SignVertex(v2)) == Math.Sign(g.SignVertex(v)))
                    {
                        t1.deltr();
                        v1 = v2;
                        v2 = v3;
                        v3 = edges.Single(a => (a.GetVertexes.Contains(v2)) && (!a.GetVertexes.Contains(v1))).GetVertexes.Single(b => b != v2);

                    }
                    else
                    {
                        verts.Remove(v2);
                        edges.RemoveAll(a => a.GetVertexes.Contains(v2));
                        edges.Add(new Edge(v1, v3));
                        triangles.Add(t1);
                        v2 = v3;
                        v3 = edges.Single(a => (a.GetVertexes.Contains(v2)) && (!a.GetVertexes.Contains(v1))).GetVertexes.Single(b => b != v2);
                    }
                }

                if (y == false)
                {
                    t1.deltr();
                    v1 = v2;
                    v2 = v3;
                    v3 = edges.Single(a => (a.GetVertexes.Contains(v2)) && (!a.GetVertexes.Contains(v1))).GetVertexes.Single(b => b != v2);
                }
            }
            var fgh = new triangle(verts[0], verts[1], verts[2]);
            triangles.Add(fgh);
        }


        void NatNearestInterpolation(Vertex vert)
        {
            pseudopol.Clear();
            pseuvotr.Clear();
            pseuvovert.Clear();
            newtriangles.Clear();

            if (addnewpoint(vert, false) == -1) return;
            List<Vertex> lv = new List<Vertex>();
            foreach (var e1 in pseuvotr)
                foreach (var e2 in e1.GetVertices)
                {
                    if (!lv.Contains(e2)) lv.Add(e2);
                }
            lv.Add(vert);

            foreach (var q in lv)
            {
                List<Vertex> temp = new List<Vertex>();
                triangle t = q.GetAdjacentTriangles[0];
                Vertex y = t.GetVertices.First(a => a != q);
                Vertex y0;
                for (int i = 0; i < q.GetAdjacentTriangles.Count; i++)
                {
                    temp.Add(t.GetOR);
                    y0 = y;
                    y = t.GetVertices.Single(a => (a != y) && (a != q));
                    t = q.GetAdjacentTriangles.Single(a => a.GetVertices.Contains(y) && (!a.GetVertices.Contains(y0)));
                }
                VoronoiCell p = new VoronoiCell(q, temp);
                pseudopol.Add(p);
            }

            lv.Remove(vert);

            double sq = pseudopol.Single(a => a.GetCellCentr == vert).GetSquare;
            foreach (var q in lv)
            {
                double sqss = q.Voronoi_Cell.GetSquare;
                double sqs = pseudopol.Single(a => a.GetCellCentr == q).GetSquare;
                vert.Value += (sqss - sqs) / sq * q.Value;
            }

            foreach (var n in newtriangles)
            {
                n.deltr();
                triangles.Remove(n);
            }
            foreach (var n in pseuvotr)
            {
                var tem = new triangle(n.GetVertices[0], n.GetVertices[1], n.GetVertices[2]);
                triangles.Add(tem);
            }
            vertices.Remove(vert);
        }

        #endregion _private_methods

        #region _public_methods
        
        public List<Vertex> NatNearestInterpolation(List<Vertex> verts,bool parallel = false)
        {

            if (parallel == false)
            {
                List<Vertex> res = new List<Vertex>();
                foreach (var w in verts)
                {
                    Vertex v =new Vertex(w);
                    NatNearestInterpolation(v);
                    res.Add(v);
                }
                return res;
            }
            else
            {
                int pc = Environment.ProcessorCount;
                Delaunay_Voronoi[] dvm = new Delaunay_Voronoi[pc];
                dvm[0] = this;
                for (int i = 1; i < pc; i++)
                    dvm[i] = new Delaunay_Voronoi(this);

                int[] h = new Int32[pc + 1];
                h[0] = 0;
                h[pc] = verts.Count;
                for (int i = 1; i < pc; i++)
                {
                    h[i] = (int)Math.Truncate((double)(i * verts.Count / pc));
                }
                List<Vertex> res = new List<Vertex>();
                Action<int> _ForAction = (i) =>
                {
                    int _h1, _h2;
                    lock (h) { _h1 = h[i]; _h2 = h[i + 1]; }
                    List<Vertex> d = new List<Vertex>();
                    for (int j = _h1; j < _h2; j++)
                    {
                        Vertex v = new Vertex(verts[i]);
                        NatNearestInterpolation(v);
                        d.Add(v);
                    }
                    lock (res)
                    {
                        res.AddRange(d);
                    }
                };

                Parallel.For(0, Environment.ProcessorCount, _ForAction);
                return res;
            }
        }

        public List<Vertex> NatNearestInterpolation(double fromlongitude, double fromlatitude, double tolongitude, double tolatitude, int Nlongitude, int Nlatitude, bool parallel = false)
        {
            List<Vertex> f = new List<Vertex>();
            double latd = (tolatitude - fromlatitude) / Nlatitude;
            double lond = (tolongitude - fromlongitude) / Nlongitude;

            for (int i = 0; i < Nlongitude; i++)
                for (int j = 0; j < Nlatitude; j++)
                    f.Add(new Vertex(fromlongitude+i*lond,fromlatitude+ j*latd));
            
            if (parallel == false)
            {
                List<Vertex> res = new List<Vertex>();
                foreach (var w in f)
                {
                    Vertex v = new Vertex(w);
                    NatNearestInterpolation(v);
                    res.Add(v);
                }
                return res;
            }
            else
            {
                int pc = Environment.ProcessorCount;
                
                Delaunay_Voronoi[] dvm = new Delaunay_Voronoi[pc];
                
                var c = DateTime.Now;
                dvm[0] = this;
                for (int i = 1; i < pc; i++)
                    dvm[i] = new Delaunay_Voronoi(this);
                Console.WriteLine("Copy {0}", DateTime.Now-c);

                int[] h = new Int32[pc + 1];
                h[0] = 0;
                h[pc] = f.Count;
                for (int i = 1; i < pc; i++)
                {
                    h[i] = (int)Math.Truncate((double)(i * f.Count / pc));
                }
                List<Vertex> res = new List<Vertex>();
                Action<int> _ForAction = (i) =>
                {
                    int _h1, _h2;
                    lock (h) { _h1 = h[i]; _h2 = h[i + 1]; }
                    List<Vertex> d = new List<Vertex>();
                    for (int j = _h1; j < _h2; j++)
                    {
                        Vertex v = new Vertex(f[j]);
                        dvm[i].NatNearestInterpolation(v);
                        d.Add(v);
                    }
                    lock (res)
                    {
                        res.AddRange(d);
                    }
                };

                Parallel.For(0, pc, _ForAction);
                    return res;
            }
        }

        public Vertex NatNearestInterpolation(double longitude, double latitude)
        {
            Vertex w = new Vertex(longitude, latitude);
            
            NatNearestInterpolation(w);
            
            return w;
        }
        
        #endregion _public_methods

    }
}