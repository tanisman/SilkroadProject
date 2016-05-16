namespace SR_GameServer.Data.NavMesh
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using SCommon;

    using SharpDX;

    public static class JmxMesh
    {
        public static List<_bms_data> s_List;

        static JmxMesh()
        {
            s_List = new List<_bms_data>();
        }

        public static _bms_data Load(string dir)
        {
            if (s_List.Exists(p => p.directory == dir))
                return s_List.Find(p => p.directory == dir);

            using (var reader = new BinaryReader(File.Open(Path.Combine(Environment.CurrentDirectory, "data", dir), FileMode.Open)))
            {
                reader.ReadBytes(12); //skip header
                reader.ReadBytes(5 * 4); //skip pointers
                int pointer_bbox = reader.ReadInt32();
                reader.ReadBytes(1 * 4); //skip pointers
                int pointer_hitbox = reader.ReadInt32();
                reader.ReadBytes(2 * 4); //skip pointers

                uint unk00 = reader.ReadUInt32();
                uint unk01 = reader.ReadUInt32(); //has special collision
                uint unk02 = reader.ReadUInt32();
                uint lightmap = reader.ReadUInt32(); //0 = none, 1024 = lightmap
                uint unk03 = reader.ReadUInt32();

                _bms_data bms = new _bms_data();
                bms.directory = dir;
                reader.BaseStream.Position = pointer_bbox;
                bms.BBox = new BoundingBox(reader.ReadVector3(), reader.ReadVector3());

                if (pointer_hitbox > 0)
                {
                    reader.BaseStream.Position = pointer_hitbox;

                    int point_count = reader.ReadInt32();
                    bms.Points = new sPoint[point_count];

                    for (int i = 0; i < point_count; i++)
                    {
                        var point = new sPoint();
                        point.Position = reader.ReadVector3();
                        point.Flag = reader.ReadByte();

                        bms.Points[i] = point;
                    }

                    int objectground_count = reader.ReadInt32();
                    bms.ObjectGround = new sTriangle[objectground_count];

                    for (int i = 0; i < objectground_count; i++)
                    {
                        var triangle = new sTriangle();
                        triangle.ID = i;
                        triangle.PointA = reader.ReadUInt16();
                        triangle.PointB = reader.ReadUInt16();
                        triangle.PointC = reader.ReadUInt16();
                        triangle.unk00 = reader.ReadUInt16();

                        if (unk01 == 6 || unk01 == 7 || unk01 == 14)
                            triangle.unk01 = reader.ReadByte();

                        bms.ObjectGround[i] = triangle;
                    }

                    int outlines_count = reader.ReadInt32();
                    bms.OutLines = new sLine[outlines_count];

                    for (int i = 0; i < outlines_count; i++)
                    {
                        var outline = new sLine();
                        outline.PointA = reader.ReadUInt16();
                        outline.PointB = reader.ReadUInt16();
                        outline.NeighbourA = reader.ReadUInt16();
                        outline.NeighbourB = reader.ReadUInt16();
                        outline.Flag = reader.ReadByte();
                        if (unk01 == 5 || unk01 == 7)
                            outline.unk00 = reader.ReadByte();

                        bms.OutLines[i] = outline;
                    }

                    int inline_count = reader.ReadInt32();
                    bms.InLines = new sLine[inline_count];

                    for (int i = 0; i < inline_count; i++)
                    {
                        var inline = new sLine();
                        inline.PointA = reader.ReadUInt16();
                        inline.PointB = reader.ReadUInt16();
                        inline.NeighbourA = reader.ReadUInt16();
                        inline.NeighbourB = reader.ReadUInt16();
                        inline.Flag = reader.ReadByte();
                        if (unk01 == 5 || unk01 == 7)
                            inline.unk00 = reader.ReadByte();

                        bms.InLines[i] = inline;
                    }

                    if (unk01 == 4 || unk01 == 5 || unk01 == 6 || unk01 == 7 || unk01 == 8)
                    {
                        int event_count = reader.ReadInt32();
                        bms.Events = new string[event_count];

                        for (int i = 0; i < event_count; i++)
                            bms.Events[i] = reader.ReadAscii();
                    }
                }
                return bms;
            }
        }
        public static List<_bms_data> Items => s_List;
    }
}
