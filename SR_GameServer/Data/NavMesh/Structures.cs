namespace SR_GameServer.Data.NavMesh
{
    using SharpDX;

    public struct _bms_data
    {
        public string directory;
        public sPoint[] Points;
        public sTriangle[] ObjectGround;
        public sLine[] InLines;
        public sLine[] OutLines;
        public BoundingBox BBox;
        public string[] Events;
    }

    public struct _bsr_data
    {
        public uint model;
        public string directory;
        public uint type;
        public string name;
        public string mesh;
        public BoundingBox BBox;
        public OrientedBoundingBox OBBox;
    }

    public struct _nvm_link_bsr
    {
        public uint model;
        public int unk;
        public string directory;
    }

    public struct _nvm_data
    {
        public int company;
        public int format;
        public int version;
        public short region;
        public _nvm_entry[] entries;
        public _nvm_zone1[] zone1;
        public _nvm_zone2[] zone2;
        public uint unk00;
        public _nvm_zone3[] zone3;
        public float[] heightmap;
    }

    public struct _nvm_entry
    {
        public uint model;
        public Vector3 position;
        public ushort collisionflag;
        public float yaw;
        public ushort uniqueid;
        public ushort unk00;
        public ushort unk01;
        public short region;
        public _nvm_entry_extra[] extra;
        public _bsr_data resource;
        public bool hasmesh;
        public _bms_data mesh;
    }

    public struct _nvm_entry_extra
    {
        public uint unk00;
        public ushort unk01;
    }

    public struct _nvm_zone1
    {
        public Vector2 min;
        public Vector2 max;
        public _nvm_zone1_extra[] extra;
    }

    public struct _nvm_zone1_extra
    {
        public ushort entryidx;
    }

    public struct _nvm_zone2
    {
        public Vector2 PointA;
        public Vector2 PointB;
        public byte flag;
        public byte source;
        public byte destination;
        public ushort ZoneSource;
        public ushort ZoneDestination;
        public short RegionSource;
        public short RegionDestination;
    }

    public struct _nvm_zone3
    {
        public Vector2 PointA;
        public Vector2 PointB;
        public byte flag;
        public byte source;
        public byte destination;
        public ushort ZoneSource;
        public ushort ZoneDestination;
    }
    
    public struct sPoint
    {
        public Vector3 Position;
        public byte Flag;
    }

    public struct sLine
    {
        public ushort PointA;
        public ushort PointB;
        public ushort NeighbourA;
        public ushort NeighbourB;
        public byte Flag;
        public byte unk00;
    }

    public struct sTriangle
    {
        public int ID;
        public ushort PointA;
        public ushort PointB;
        public ushort PointC;
        public ushort unk00;
        public byte unk01;
    }
}
