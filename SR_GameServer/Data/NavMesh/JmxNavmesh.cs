namespace SR_GameServer.Data.NavMesh
{
    using System;
    using System.IO;
    using System.Globalization;

    using SCommon;

    using SharpDX;

    public static class JmxNavmesh
    {
        private static _nvm_data[] s_List;

        public static void Load()
        {
            JmxObj.Load();

            s_List = new _nvm_data[65535];

            string[] nvmFiles = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "data\\navmesh\\"));
            foreach (var nvmfile in nvmFiles)
            {
                if (nvmfile != null)
                {
                    if (nvmfile.Split('.')[1] == "nvm")
                    {
                        using (var reader = new BinaryReader(new FileStream(nvmfile, FileMode.Open, FileAccess.Read)))
                        {
                            FileInfo info = new FileInfo(nvmfile);
                            _nvm_data nvm = new _nvm_data();
                            nvm.company = reader.ReadInt32();
                            nvm.format = reader.ReadInt32();
                            nvm.version = reader.ReadInt32();

                            nvm.region = short.Parse(info.Name.Replace("nv_", "").Replace(".nvm", "").ToString(), NumberStyles.HexNumber);

                            nvm.entries = new _nvm_entry[reader.ReadUInt16()];
                            for(int i = 0; i < nvm.entries.Length; i++)
                            {
                                nvm.entries[i] = new _nvm_entry();
                                nvm.entries[i].model = reader.ReadUInt32();
                                nvm.entries[i].position = reader.ReadVector3();
                                nvm.entries[i].collisionflag = reader.ReadUInt16();
                                nvm.entries[i].yaw = reader.ReadSingle();
                                nvm.entries[i].uniqueid = reader.ReadUInt16();
                                nvm.entries[i].unk00 = reader.ReadUInt16();
                                nvm.entries[i].unk01 = reader.ReadUInt16();
                                nvm.entries[i].region = reader.ReadInt16();

                                nvm.entries[i].extra = new _nvm_entry_extra[reader.ReadUInt16()];
                                for(int j = 0; j < nvm.entries[i].extra.Length; j++)
                                {
                                    nvm.entries[i].extra[j] = new _nvm_entry_extra();
                                    nvm.entries[i].extra[j].unk00 = reader.ReadUInt32();
                                    nvm.entries[i].extra[j].unk01 = reader.ReadUInt16();
                                }

                                nvm.entries[i].resource = JmxRes.Load(nvm.entries[i].model);
                               //translate res bbox
                                nvm.entries[i].resource.BBox = new BoundingBox(
                                    nvm.entries[i].position + nvm.entries[i].resource.BBox.Minimum.ToVector2().Rotated(nvm.entries[i].yaw).ToVector3(nvm.entries[i].resource.BBox.Minimum.Y),
                                    nvm.entries[i].position + nvm.entries[i].resource.BBox.Maximum.ToVector2().Rotated(nvm.entries[i].yaw).ToVector3(nvm.entries[i].resource.BBox.Maximum.Y));

                                //correct bbox min/max & scale by 100
                                Vector3 v1 = nvm.entries[i].resource.BBox.Minimum, v2 = nvm.entries[i].resource.BBox.Maximum;
                                nvm.entries[i].resource.BBox.Minimum.X = Math.Min(v1.X, v2.X) - 50;
                                nvm.entries[i].resource.BBox.Maximum.X = Math.Max(v1.X, v2.X) + 50;
                                nvm.entries[i].resource.BBox.Minimum.Z = Math.Min(v1.Z, v2.Z) - 50;
                                nvm.entries[i].resource.BBox.Maximum.Z = Math.Max(v1.Z, v2.Z) + 50;

                                //re-create obbox for translated object
                                nvm.entries[i].resource.OBBox = new OrientedBoundingBox(nvm.entries[i].resource.BBox);

                                if (nvm.entries[i].resource.mesh != null && nvm.entries[i].resource.mesh != string.Empty)
                                {
                                    nvm.entries[i].mesh = JmxMesh.Load(nvm.entries[i].resource.mesh);
                                    nvm.entries[i].hasmesh = true;
                                    //translate mesh bbox
                                    nvm.entries[i].mesh.BBox = new BoundingBox(
                                        nvm.entries[i].position + nvm.entries[i].mesh.BBox.Minimum.ToVector2().Rotated(nvm.entries[i].yaw).ToVector3(nvm.entries[i].mesh.BBox.Minimum.Y),
                                        nvm.entries[i].position + nvm.entries[i].mesh.BBox.Maximum.ToVector2().Rotated(nvm.entries[i].yaw).ToVector3(nvm.entries[i].mesh.BBox.Maximum.Y));

                                    //translate mesh points
                                    for (int j = 0; j < nvm.entries[i].mesh.Points.Length; j++)
                                    {
                                        nvm.entries[i].mesh.Points[j].Position = 
                                            nvm.entries[i].position + nvm.entries[i].mesh.Points[j].Position.ToVector2().Rotated(nvm.entries[i].yaw).ToVector3(nvm.entries[i].mesh.Points[j].Position.Y);
                                    }
                                }
                            }

                            nvm.zone1 = new _nvm_zone1[reader.ReadUInt32()];
                            nvm.unk00 = reader.ReadUInt32();
                            for (int i = 0; i < nvm.zone1.Length; i++)
                            {
                                nvm.zone1[i] = new _nvm_zone1();
                                nvm.zone1[i].min = reader.ReadVector2();
                                nvm.zone1[i].max = reader.ReadVector2();

                                nvm.zone1[i].extra = new _nvm_zone1_extra[reader.ReadByte()];
                                for(int j = 0; j < nvm.zone1[i].extra.Length; j++)
                                {
                                    nvm.zone1[i].extra[j] = new _nvm_zone1_extra();
                                    nvm.zone1[i].extra[j].entryidx = reader.ReadUInt16();
                                }
                            }

                            nvm.zone2 = new _nvm_zone2[reader.ReadUInt32()];
                            for (int i = 0; i < nvm.zone2.Length; i++)
                            {
                                nvm.zone2[i] = new _nvm_zone2();
                                nvm.zone2[i].PointA = reader.ReadVector2();
                                nvm.zone2[i].PointB = reader.ReadVector2();
                                nvm.zone2[i].flag = reader.ReadByte();
                                nvm.zone2[i].source = reader.ReadByte();
                                nvm.zone2[i].destination = reader.ReadByte();
                                nvm.zone2[i].ZoneSource = reader.ReadUInt16();
                                nvm.zone2[i].ZoneDestination = reader.ReadUInt16();
                                nvm.zone2[i].RegionSource = reader.ReadInt16();
                                nvm.zone2[i].RegionDestination = reader.ReadInt16();
                            }

                            nvm.zone3 = new _nvm_zone3[reader.ReadUInt32()];
                            for (int i = 0; i < nvm.zone3.Length; i++)
                            {
                                nvm.zone3[i] = new _nvm_zone3();
                                nvm.zone3[i].PointA = reader.ReadVector2();
                                nvm.zone3[i].PointB = reader.ReadVector2();
                                nvm.zone3[i].flag = reader.ReadByte();
                                nvm.zone3[i].source = reader.ReadByte();
                                nvm.zone3[i].destination = reader.ReadByte();
                                nvm.zone3[i].ZoneSource = reader.ReadUInt16();
                                nvm.zone3[i].ZoneDestination = reader.ReadUInt16();
                            }

                            reader.BaseStream.Position += 0x9000; //skip texture map

                            nvm.heightmap = new float[9409];
                            for (int i = 0; i < 9409; i++)
                                nvm.heightmap[i] = reader.ReadSingle();

                            s_List[(ushort)nvm.region] = nvm;
                        }
                    }
                }
            }
            Logging.Log()("NavMesh loaded.");
        }

        public static float GetHeightAt(short region, float x, float z)
        {
            if (x > 1920)
            {
                region += (short)(x / 1920);
                x = x % 1920;
            }
            else if (x < 0)
            {
                region -= (short)((short)((-x - 1) / 1920) + 1);
                x = 1920 + (x % 1920);
            }

            if (z > 1920)
            {
                region += (short)(Formula.REGION_SCALE * z / 1920);
                z = z % 1920;
            }
            else if (z < 0)
            {
                region -= (short)(Formula.REGION_SCALE * ((short)((-z - 1) / 1920) + 1));
                z = 1920 + (z % 1920);
            }

            if (s_List[(ushort)region].heightmap == null)
                return 0;

            return s_List[(ushort)region].heightmap[Math.Min(9408, (int)((int)z / 20f) * 97 + (int)((int)x / 20f))];
        }

        public static _nvm_data[] Items => s_List;
    }
}
