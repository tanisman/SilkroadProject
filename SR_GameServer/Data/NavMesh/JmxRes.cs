namespace SR_GameServer.Data.NavMesh
{
    using System;
    using System.IO;
    using System.Collections.Generic;

    using SCommon;

    using SharpDX;

    public static class JmxRes
    {
        private static List<_bsr_data> s_List;

        static JmxRes()
        {
            s_List = new List<_bsr_data>();
        }

        public static _bsr_data Load(uint model)
        {
            if (s_List.Exists(p => p.model == model))
                return s_List.Find(p => p.model == model);

            _bsr_data bsr = new _bsr_data();
            bsr.model = model;
            bsr.directory = JmxObj.Items[model].directory;

            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "data",  bsr.directory)))
            {
                using (var reader = new BinaryReader(new FileStream(Path.Combine(Environment.CurrentDirectory, "data\\" + bsr.directory), FileMode.Open, FileAccess.Read)))
                {
                    reader.ReadBytes(12); //skip header
                    reader.ReadBytes(7 * 4); //skip pointers
                    int pointer_bbox = reader.ReadInt32();
                    reader.ReadBytes(5 * 4); //skip unknown dwords

                    bsr.type = reader.ReadUInt32();
                    bsr.name = reader.ReadAscii();

                    reader.ReadBytes(48); //skip unknown bytes;
                    
                    if (bsr.type == 0x20002 || bsr.type == 0x20003 || bsr.type == 0x20004)
                    {
                        reader.BaseStream.Position = pointer_bbox; //jump
                        bsr.mesh = reader.ReadAscii();
                        bsr.BBox = new BoundingBox(reader.ReadVector3(), reader.ReadVector3());
                        bsr.OBBox = new OrientedBoundingBox(reader.ReadVector3(), reader.ReadVector3());
                    }
                }
            }

            s_List.Add(bsr);
            return bsr;
        }

        public static List<_bsr_data> Items => s_List;
    }
}
