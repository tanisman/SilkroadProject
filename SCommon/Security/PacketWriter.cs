namespace SCommon.Security
{
    using System.IO;

    public class PacketWriter : BinaryWriter
    {
        MemoryStream m_ms;
        
        public PacketWriter()
        {
            m_ms = new MemoryStream();
            this.OutStream = m_ms;
        }

        public byte[] GetBytes()
        {
            return m_ms.ToArray();
        }
    }
}
