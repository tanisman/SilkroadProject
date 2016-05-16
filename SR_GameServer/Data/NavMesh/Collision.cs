namespace SR_GameServer.Data.NavMesh
{
    using System;

    using SCommon;

    using SharpDX;

    public static class Collision
    {
        public static bool Test(short region, Vector3 from, Vector3 to, out Vector3 result)
        {
            if (JmxNavmesh.Items[(ushort)region].entries == null)
            {
                result = from;
                return true;
            }

            bool on_object = false;
            float object_height = 0f;

            foreach (var entry in JmxNavmesh.Items[(ushort)region].entries)
            {
                var box = entry.resource.BBox;
                if ((box.Minimum.X <= from.X && box.Maximum.X >= from.X && box.Minimum.Z <= from.Z && box.Maximum.Z >= from.Z)
                    || (box.Minimum.X <= to.X && box.Maximum.X >= to.X && box.Minimum.Z <= to.Z && box.Maximum.Z >= to.Z))
                {
                    if (entry.hasmesh)
                    {
                        //if (entry.mesh.BBox.Contains(from) > ContainmentType.Disjoint || entry.mesh.BBox.Contains(to) > ContainmentType.Disjoint)
                        {
                            //Console.WriteLine("bbox test ok");
                            foreach (var line in entry.mesh.OutLines)
                            {
                                if (from.Y >= Math.Min(entry.mesh.Points[line.PointA].Position.Y, entry.mesh.Points[line.PointB].Position.Y) - 100 && from.Y <= Math.Max(entry.mesh.Points[line.PointA].Position.Y, entry.mesh.Points[line.PointB].Position.Y) + 100)
                                {
                                    Vector2 res;
                                    if (LineIntersectsLine(
                                        entry.mesh.Points[line.PointA].Position.ToVector2(), entry.mesh.Points[line.PointB].Position.ToVector2(),
                                        from.ToVector2(), to.ToVector2(), out res))
                                    {
                                        if (line.Flag != 3) //passable
                                        {
                                            result = res.ToVector3(JmxNavmesh.GetHeightAt(region, to.X, to.Z));

                                            var triangle = FindTriangle(entry.mesh, line.NeighbourA);
                                            if (triangle.ID == line.NeighbourA && TriangleContainsPoint(entry.mesh.Points[triangle.PointA].Position, entry.mesh.Points[triangle.PointB].Position, entry.mesh.Points[triangle.PointC].Position, to))
                                                result = res.ToVector3((entry.mesh.Points[triangle.PointA].Position.Y + entry.mesh.Points[triangle.PointB].Position.Y + entry.mesh.Points[triangle.PointC].Position.Y) / 3f);
                                            else if (line.NeighbourB != 0xFFFF)
                                            {
                                                triangle = FindTriangle(entry.mesh, line.NeighbourB);
                                                if (triangle.ID == line.NeighbourB && TriangleContainsPoint(entry.mesh.Points[triangle.PointA].Position, entry.mesh.Points[triangle.PointB].Position, entry.mesh.Points[triangle.PointC].Position, to))
                                                    result = res.ToVector3((entry.mesh.Points[triangle.PointA].Position.Y + entry.mesh.Points[triangle.PointB].Position.Y + entry.mesh.Points[triangle.PointC].Position.Y) / 3f);
                                            }
                                            return false;
                                        }
                                        else
                                        {
                                            result = res.ToVector3(0);
                                            return true;
                                        }
                                    }
                                }
                            }

                            foreach (var line in entry.mesh.InLines)
                            {
                                if (from.Y >= Math.Min(entry.mesh.Points[line.PointA].Position.Y, entry.mesh.Points[line.PointB].Position.Y) - 50 && from.Y <= Math.Max(entry.mesh.Points[line.PointA].Position.Y, entry.mesh.Points[line.PointB].Position.Y) + 50)
                                {
                                    Vector2 res;
                                    if (LineIntersectsLine(
                                    entry.mesh.Points[line.PointA].Position.ToVector2(), entry.mesh.Points[line.PointB].Position.ToVector2(),
                                    from.ToVector2(), to.ToVector2(), out res))
                                    {
                                        if (line.Flag != 7) //passable
                                        {
                                            result = res.ToVector3(JmxNavmesh.GetHeightAt(region, to.X, to.Z));

                                            var triangle = FindTriangle(entry.mesh, line.NeighbourA);
                                            if (triangle.ID == line.NeighbourA && TriangleContainsPoint(entry.mesh.Points[triangle.PointA].Position, entry.mesh.Points[triangle.PointB].Position, entry.mesh.Points[triangle.PointC].Position, to))
                                                result = res.ToVector3((entry.mesh.Points[triangle.PointA].Position.Y + entry.mesh.Points[triangle.PointB].Position.Y + entry.mesh.Points[triangle.PointC].Position.Y) / 3f);
                                            else if (line.NeighbourB != 0xFFFF)
                                            {
                                                triangle = FindTriangle(entry.mesh, line.NeighbourB);
                                                if (triangle.ID == line.NeighbourB && TriangleContainsPoint(entry.mesh.Points[triangle.PointA].Position, entry.mesh.Points[triangle.PointB].Position, entry.mesh.Points[triangle.PointC].Position, to))
                                                    result = res.ToVector3((entry.mesh.Points[triangle.PointA].Position.Y + entry.mesh.Points[triangle.PointB].Position.Y + entry.mesh.Points[triangle.PointC].Position.Y) / 3f);
                                            }
                                            return false;
                                        }
                                        else
                                        {
                                            result = res.ToVector3(0);
                                            return true;
                                        }
                                    }
                                }
                            }

                            foreach (var tri in entry.mesh.ObjectGround)
                            {
                                if (from.Y >= (entry.mesh.Points[tri.PointA].Position.Y + entry.mesh.Points[tri.PointB].Position.Y + entry.mesh.Points[tri.PointC].Position.Y) / 3f - 50 && from.Y <= (entry.mesh.Points[tri.PointA].Position.Y + entry.mesh.Points[tri.PointB].Position.Y + entry.mesh.Points[tri.PointC].Position.Y) / 3f + 50)
                                {
                                    if (TriangleContainsPoint(
                                    entry.mesh.Points[tri.PointA].Position.ToVector2(), entry.mesh.Points[tri.PointB].Position.ToVector2(),
                                    entry.mesh.Points[tri.PointC].Position.ToVector2(), from.ToVector2()))
                                    {
                                        on_object = true;
                                        object_height = (entry.mesh.Points[tri.PointA].Position.Y + entry.mesh.Points[tri.PointB].Position.Y + entry.mesh.Points[tri.PointC].Position.Y) / 3f;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Vector2 res;
                        Vector2 min = entry.resource.BBox.Minimum.ToVector2();
                        Vector2 max = entry.resource.BBox.Maximum.ToVector2();
                        Vector2 cornerA = new Vector2(min.X, max.Y);
                        Vector2 cornerB = new Vector2(max.X, max.Y);
                        Vector2 cornerC = new Vector2(max.X, min.Y);
                        Vector2 cornerD = new Vector2(min.X, min.Y);

                        if (!LineIntersectsLine(cornerA, cornerB, from.ToVector2(), to.ToVector2(), out res))
                        {
                            if (!LineIntersectsLine(cornerB, cornerC, from.ToVector2(), to.ToVector2(), out res))
                            {
                                if (!LineIntersectsLine(cornerC, cornerD, from.ToVector2(), to.ToVector2(), out res))
                                {
                                    if (!LineIntersectsLine(cornerD, cornerA, from.ToVector2(), to.ToVector2(), out res))
                                    {
                                        result = from;
                                        Logging.Log()("wat ? imposubru should intersect at least 1 line of bbox because one of the points is inside the bbox, check plz", LogLevel.Error);
                                        return true;
                                    }
                                }
                            }
                        }
                        result = res.ToVector3(0);
                        return true;
                    }
                }
            }

            foreach (var zone in JmxNavmesh.Items[(ushort)region].zone3)
            {
                Vector2 res;
                if (LineIntersectsLine(zone.PointA, zone.PointB, from.ToVector2(), to.ToVector2(), out res))
                {
                    if (zone.ZoneDestination == 0xFFFF || zone.ZoneSource == 0xFFFF)
                    {
                        if (!on_object)
                        {
                            result = res.ToVector3(0);
                            return true;
                        }
                    }
                }
            }

            if (on_object)
                result = new Vector3(to.X, object_height, to.Z);
            else
                result = new Vector3(to.X, JmxNavmesh.GetHeightAt(region, to.X, to.Z), to.Z);
            return false;
        }

        public static bool IsValid(this Vector3 v, short region)
        {
            Vector3 res;
            return JmxNavmesh.Items[(ushort)region].entries != null
                && !Test(region, v, v + new Vector3((float)Math.Cos(0), 0, (float)Math.Sin(0)) * 100, out res)
                && !Test(region, v, v + new Vector3((float)Math.Cos(30), 0, (float)Math.Sin(30)) * 100, out res)
                && !Test(region, v, v + new Vector3((float)Math.Cos(60), 0, (float)Math.Sin(60)) * 100, out res)
                && !Test(region, v, v + new Vector3((float)Math.Cos(90), 0, (float)Math.Sin(90)) * 100, out res)
                && !Test(region, v, v + new Vector3((float)Math.Cos(120), 0, (float)Math.Sin(120)) * 100, out res)
                && !Test(region, v, v + new Vector3((float)Math.Cos(150), 0, (float)Math.Sin(150)) * 100, out res)
                && !Test(region, v, v + new Vector3((float)Math.Cos(180), 0, (float)Math.Sin(180)) * 100, out res)
                && !Test(region, v, v + new Vector3((float)Math.Cos(210), 0, (float)Math.Sin(120)) * 100, out res)
                && !Test(region, v, v + new Vector3((float)Math.Cos(240), 0, (float)Math.Sin(240)) * 100, out res)
                && !Test(region, v, v + new Vector3((float)Math.Cos(270), 0, (float)Math.Sin(270)) * 100, out res)
                && !Test(region, v, v + new Vector3((float)Math.Cos(300), 0, (float)Math.Sin(300)) * 100, out res)
                && !Test(region, v, v + new Vector3((float)Math.Cos(330), 0, (float)Math.Sin(330)) * 100, out res);
        }

        private static bool LineIntersectsLine(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2, out Vector2 pos)
        {
            pos = Vector2.Zero;
            float denom = ((end2.Y - start2.Y) * (end1.X - start1.X)) - ((end2.X - start2.X) * (end1.Y - start1.Y));
            if (denom == 0)
                return false;

            float invDenom = 1f / denom;

            float ua = (((end2.X - start2.X) * (start1.Y - start2.Y)) - ((end2.Y - start2.Y) * (start1.X - start2.X))) * invDenom;
            float ub = (((end1.X - start1.X) * (start1.Y - start2.Y)) - ((end1.Y - start1.Y) * (start1.X - start2.X))) * invDenom;

            if (ua < 0f || ua > 1f || ub < 0f || ub > 1f)
                return false;

            pos = new Vector2(start1.X + ua * (end1.X - start1.X), start1.Y + ua * (end1.Y - start1.Y));
            return true;
        }

        private static sTriangle FindTriangle(_bms_data mesh, int id)
        {
            for (int i = 0; i < mesh.ObjectGround.Length; i++)
            {
                if (mesh.ObjectGround[i].ID == id)
                    return mesh.ObjectGround[i];
            }
            return default(sTriangle);
        }

        private static bool TriangleContainsPoint(Vector3 PointA, Vector3 PointB, Vector3 PointC, Vector3 point)
        {
            return TriangleContainsPoint(PointA.ToVector2(), PointB.ToVector2(), PointC.ToVector2(), point.ToVector2());
        }

        private static bool TriangleContainsPoint(Vector2 PointA, Vector2 PointB, Vector2 PointC, Vector2 point)
        {
            float v0x = PointC.X - PointA.X, v0z = PointC.Y - PointA.Y;
            float v1x = PointB.X - PointA.X, v1z = PointB.Y- PointA.Y;
            float v2x = (int)point.X - PointA.X, v2z = (int)point.Y - PointA.Y;

            float dot00 = dot(v0x, v0z, v0x, v0z);
            float dot01 = dot(v0x, v0z, v1x, v1z);
            float dot02 = dot(v0x, v0z, v2x, v2z);
            float dot11 = dot(v1x, v1z, v1x, v1z);
            float dot12 = dot(v1x, v1z, v2x, v2z);

            float invDenom = 1f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            return u > 0f && v > 0f && u + v < 1f;
        }

        private static float dot(float ax, float az, float bx, float bz)
        {
            return ax * bx + az * bz;
        }
    }
}
