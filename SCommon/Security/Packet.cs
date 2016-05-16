namespace SCommon.Security
{
    using System;
    using System.Text;
    using System.IO;

    public struct Packet
    {
        private ushort m_opcode;
        private PacketWriter m_writer;
        private PacketReader m_reader;
        private bool m_encrypted;
        private bool m_massive;
        private bool m_locked;
        private byte[] m_reader_bytes;
        private object m_lock;
        public object m_data;
        public delegate object Reader(object a, bool b, PacketReader c);
        public delegate void Writer(object value, object a, bool b, PacketWriter c);

        public ushort Opcode
        {
            get { return m_opcode; }
        }
        public bool Encrypted
        {
            get { return m_encrypted; }
        }
        public bool Massive
        {
            get { return m_massive; }
        }
        public long Size
        {
            get
            {
                if (m_locked)
                    return m_reader_bytes.Length;
                else
                    return m_writer.BaseStream.Position + 6;
            }
        }

        public Packet(Packet rhs)
        {
            lock (rhs.m_lock)
            {
                m_lock = new object();

                m_opcode = rhs.m_opcode;
                m_encrypted = rhs.m_encrypted;
                m_massive = rhs.m_massive;

                m_locked = rhs.m_locked;
                if (!m_locked)
                {
                    m_writer = new PacketWriter();
                    m_reader = null;
                    m_reader_bytes = null;
                    m_writer.Write(rhs.m_writer.GetBytes());
                }
                else
                {
                    m_writer = null;
                    m_reader_bytes = rhs.m_reader_bytes;
                    m_reader = new PacketReader(m_reader_bytes);
                }
                m_data = null;
            }
        }
        public Packet(ushort opcode)
        {
            m_lock = new object();
            m_opcode = opcode;
            m_encrypted = false;
            m_massive = false;
            m_writer = new PacketWriter();
            m_reader = null;
            m_reader_bytes = null;
            m_locked = false;
            m_data = null;
        }

        public Packet(ushort opcode, bool encrypted)
        {
            m_lock = new object();
            m_opcode = opcode;
            m_encrypted = encrypted;
            m_massive = false;
            m_writer = new PacketWriter();
            m_reader = null;
            m_reader_bytes = null;
            m_locked = false;
            m_data = null;
        }

        public Packet(ushort opcode, bool encrypted, bool massive)
        {
            if (encrypted && massive)
            {
                throw new Exception("[Packet::Packet] Packets cannot both be massive and encrypted!");
            }
            m_lock = new object();
            m_opcode = opcode;
            m_encrypted = encrypted;
            m_massive = massive;
            m_writer = new PacketWriter();
            m_reader = null;
            m_reader_bytes = null;
            m_locked = false;
            m_data = null;
        }
        public Packet(ushort opcode, bool encrypted, bool massive, byte[] bytes)
        {
            if (encrypted && massive)
            {
                throw new Exception("[Packet::Packet] Packets cannot both be massive and encrypted!");
            }
            m_lock = new object();
            m_opcode = opcode;
            m_encrypted = encrypted;
            m_massive = massive;
            m_writer = new PacketWriter();
            m_writer.Write(bytes);
            m_reader = null;
            m_reader_bytes = null;
            m_locked = false;
            m_data = null;
        }
        public Packet(ushort opcode, bool encrypted, bool massive, byte[] bytes, int offset, int length)
        {
            if (encrypted && massive)
            {
                throw new Exception("[Packet::Packet] Packets cannot both be massive and encrypted!");
            }
            m_lock = new object();
            m_opcode = opcode;
            m_encrypted = encrypted;
            m_massive = massive;
            m_writer = new PacketWriter();
            m_writer.Write(bytes, offset, length);
            m_reader = null;
            m_reader_bytes = null;
            m_locked = false;
            m_data = null;
        }

        public byte[] GetBytes()
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    return m_reader_bytes;
                }
                return m_writer.GetBytes();
            }
        }

        public void Lock()
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    m_reader_bytes = m_writer.GetBytes();
                    m_reader = new PacketReader(m_reader_bytes);
                    m_writer.Close();
                    m_writer = null;
                    m_locked = true;
                }
            }
        }

        public void SkipBytes(uint amount)
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Skip from an unlocked Packet.");
                }
                m_reader.BaseStream.Position += amount;
            }
        }

        public long SeekRead(long offset, SeekOrigin orgin)
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot SeekRead on an unlocked Packet.");
                }
                return m_reader.BaseStream.Seek(offset, orgin);
            }
        }

        public int RemainingRead()
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot SeekRead on an unlocked Packet.");
                }
                return (int)(m_reader.BaseStream.Length - m_reader.BaseStream.Position);
            }
        }

        public byte ReadUInt8()
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                return m_reader.ReadByte();
            }
        }
        public byte ReadByte()
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                return m_reader.ReadByte();
            }
        }
        public sbyte ReadInt8()
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                return m_reader.ReadSByte();
            }
        }
        public UInt16 ReadUInt16()
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                return m_reader.ReadUInt16();
            }
        }
        public Int16 ReadInt16()
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                return m_reader.ReadInt16();
            }
        }
        public UInt32 ReadUInt32()
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                return m_reader.ReadUInt32();
            }
        }
        public Int32 ReadInt32()
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                return m_reader.ReadInt32();
            }
        }
        public UInt64 ReadUInt64()
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                return m_reader.ReadUInt64();
            }
        }
        public Int64 ReadInt64()
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                return m_reader.ReadInt64();
            }
        }
        public Single ReadSingle()
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                return m_reader.ReadSingle();
            }
        }
        public Double ReadDouble()
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                return m_reader.ReadDouble();
            }
        }
        public String ReadAscii()
        {
            return ReadAscii(1252);
        }
        public String ReadAscii(int codepage)
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }

                UInt16 length = m_reader.ReadUInt16();

                if (length > 0)
                {
                    byte[] bytes = m_reader.ReadBytes(length);
                    return Encoding.GetEncoding(codepage).GetString(bytes);
                }
                else
                {
                    return null;
                }
            }
        }
        public String ReadUnicode()
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }

                UInt16 length = m_reader.ReadUInt16();
                byte[] bytes = m_reader.ReadBytes(length * 2);
                return Encoding.Unicode.GetString(bytes);
            }
        }

        public byte[] ReadUInt8Array(int count)
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                byte[] values = new byte[count];
                for (int x = 0; x < count; ++x)
                {
                    values[x] = m_reader.ReadByte();
                }
                return values;
            }
        }
        public char[] ReadCharArray(int count)
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                char[] values = m_reader.ReadChars(count);
                return values;
            }
        }
        public sbyte[] ReadInt8Array(int count)
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                sbyte[] values = new sbyte[count];
                for (int x = 0; x < count; ++x)
                {
                    values[x] = m_reader.ReadSByte();
                }
                return values;
            }
        }
        public UInt16[] ReadUInt16Array(int count)
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                UInt16[] values = new UInt16[count];
                for (int x = 0; x < count; ++x)
                {
                    values[x] = m_reader.ReadUInt16();
                }
                return values;
            }
        }
        public Int16[] ReadInt16Array(int count)
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                Int16[] values = new Int16[count];
                for (int x = 0; x < count; ++x)
                {
                    values[x] = m_reader.ReadInt16();
                }
                return values;
            }
        }
        public UInt32[] ReadUInt32Array(int count)
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                UInt32[] values = new UInt32[count];
                for (int x = 0; x < count; ++x)
                {
                    values[x] = m_reader.ReadUInt32();
                }
                return values;
            }
        }
        public Int32[] ReadInt32Array(int count)
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                Int32[] values = new Int32[count];
                for (int x = 0; x < count; ++x)
                {
                    values[x] = m_reader.ReadInt32();
                }
                return values;
            }
        }
        public UInt64[] ReadUInt64Array(int count)
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                UInt64[] values = new UInt64[count];
                for (int x = 0; x < count; ++x)
                {
                    values[x] = m_reader.ReadUInt64();
                }
                return values;
            }
        }
        public Int64[] ReadInt64Array(int count)
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                Int64[] values = new Int64[count];
                for (int x = 0; x < count; ++x)
                {
                    values[x] = m_reader.ReadInt64();
                }
                return values;
            }
        }
        public Single[] ReadSingleArray(int count)
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                Single[] values = new Single[count];
                for (int x = 0; x < count; ++x)
                {
                    values[x] = m_reader.ReadSingle();
                }
                return values;
            }
        }
        public Double[] ReadDoubleArray(int count)
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                Double[] values = new Double[count];
                for (int x = 0; x < count; ++x)
                {
                    values[x] = m_reader.ReadDouble();
                }
                return values;
            }
        }
        public String[] ReadAsciiArray(int count)
        {
            return ReadAsciiArray(1252);
        }
        public String[] ReadAsciiArray(int codepage, int count)
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                String[] values = new String[count];
                for (int x = 0; x < count; ++x)
                {
                    UInt16 length = m_reader.ReadUInt16();
                    byte[] bytes = m_reader.ReadBytes(length);
                    values[x] = Encoding.UTF7.GetString(bytes);
                }
                return values;
            }
        }
        public String[] ReadUnicodeArray(int count)
        {
            lock (m_lock)
            {
                if (!m_locked)
                {
                    throw new Exception("Cannot Read from an unlocked Packet.");
                }
                String[] values = new String[count];
                for (int x = 0; x < count; ++x)
                {
                    UInt16 length = m_reader.ReadUInt16();
                    byte[] bytes = m_reader.ReadBytes(length * 2);
                    values[x] = Encoding.Unicode.GetString(bytes);
                }
                return values;
            }
        }

        public void CopyBytes(byte[] buffer, int index, int count)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Copy on a locked Packet.");
                }
                m_writer.Write(buffer, index, count);
            }
        }

        public long SeekWrite(long offset, SeekOrigin orgin)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot SeekWrite on a locked Packet.");
                }
                return m_writer.BaseStream.Seek(offset, orgin);
            }
        }
        public void GoBackAndWrite(long backlen, byte a)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.BaseStream.Position = m_writer.BaseStream.Position - backlen;
                m_writer.Write((byte)a);
                m_writer.BaseStream.Position = m_writer.BaseStream.Position + backlen - 1;
            }
        }
        public void GoBackAndWrite(long backlen, Int16 a)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.BaseStream.Position = m_writer.BaseStream.Position - backlen;
                m_writer.Write((Int16)a);
                m_writer.BaseStream.Position = m_writer.BaseStream.Position + backlen + 2;
            }
        }
        public void GoBackAndWrite(long backlen, UInt16 a)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.BaseStream.Position = m_writer.BaseStream.Position - backlen;
                m_writer.Write((UInt16)a);
                m_writer.BaseStream.Position = m_writer.BaseStream.Position + backlen + 2;
            }
        }
        public void GoBackAndWrite(long backlen, Int32 a)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.BaseStream.Position = m_writer.BaseStream.Position - backlen;
                m_writer.Write((Int32)a);
                m_writer.BaseStream.Position = m_writer.BaseStream.Position + backlen + 4;
            }
        }
        public void GoBackAndWrite(long backlen, UInt32 a)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.BaseStream.Position = m_writer.BaseStream.Position - backlen;
                m_writer.Write((UInt32)a);
                m_writer.BaseStream.Position = m_writer.BaseStream.Position + backlen + 4;
            }
        }

        public void ReWriteAt(long position, byte value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                long backup_position = m_writer.BaseStream.Position;
                m_writer.BaseStream.Position = position;
                m_writer.Write((byte)value);
                m_writer.BaseStream.Position = backup_position;
            }
        }

        public void WriteUInt8(byte value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.Write(value);
            }
        }
        public void WriteByte(object value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.Write(unchecked((byte)Convert.ToInt64(value)));
            }
        }
        public void WriteInt8(sbyte value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.Write(value);
            }
        }
        public void WriteUInt16(UInt16 value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.Write(value);
            }
        }
        public void WriteInt16(Int16 value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.Write(value);
            }
        }
        public void WriteUInt32(UInt32 value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.Write(value);
            }
        }
        public void WriteInt32(Int32 value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.Write(value);
            }
        }
        public void WriteUInt64(UInt64 value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.Write(value);
            }
        }
        public void WriteInt64(Int64 value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.Write(value);
            }
        }
        public void WriteSingle(Single value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.Write(value);
            }
        }
        public void WriteDouble(Double value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.Write(value);
            }
        }
        public void WriteAscii(String value)
        {
            WriteAscii(value, 1252);
        }
        public void WriteAscii(String value, int code_page)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }

                byte[] codepage_bytes = Encoding.GetEncoding(code_page).GetBytes(value);
                string utf7_value = Encoding.UTF7.GetString(codepage_bytes);
                byte[] bytes = Encoding.Default.GetBytes(utf7_value);

                m_writer.Write((ushort)bytes.Length);
                m_writer.Write(bytes);
            }
        }
        public void WriteUnicode(String value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }

                byte[] bytes = Encoding.Unicode.GetBytes(value);

                m_writer.Write((ushort)value.ToString().Length);
                m_writer.Write(bytes);
            }
        }

        public void WriteUInt8(object value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.Write(unchecked((byte)Convert.ToInt64(value)));
            }
        }
        public void WriteInt8(object value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.Write(unchecked((sbyte)Convert.ToInt64(value)));
            }
        }
        public void WriteUInt16(object value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.Write(unchecked((ushort)Convert.ToInt64(value)));
            }
        }
        public void WriteInt16(object value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.Write(unchecked((short)Convert.ToInt64(value)));
            }
        }
        public void WriteUInt32(object value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.Write(unchecked((uint)Convert.ToInt64(value)));
            }
        }
        public void WriteInt32(object value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.Write(unchecked((int)Convert.ToInt64(value)));
            }
        }
        public void WriteUInt64(object value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.Write(unchecked((ulong)Convert.ToInt64(value)));
            }
        }
        public void WriteInt64(object value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.Write(unchecked((long)Convert.ToInt64(value)));
            }
        }
        public void WriteSingle(object value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.Write(Convert.ToSingle(value));
            }
        }
        public void WriteDouble(object value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.Write(Convert.ToDouble(value));
            }
        }
        public void WriteAscii(object value)
        {
            WriteAscii(value, 1252);
        }
        public void WriteAscii(object value, int code_page)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }

                byte[] codepage_bytes = Encoding.GetEncoding(code_page).GetBytes(value.ToString());
                string utf7_value = Encoding.UTF7.GetString(codepage_bytes);
                byte[] bytes = Encoding.Default.GetBytes(utf7_value);

                m_writer.Write((ushort)bytes.Length);
                m_writer.Write(bytes);
            }
        }
        public void WriteUnicode(object value)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }

                byte[] bytes = Encoding.Unicode.GetBytes(value.ToString());

                m_writer.Write((ushort)value.ToString().Length);
                m_writer.Write(bytes);
            }
        }
        public void WriteUInt8Array(byte[] values)
        {
            if (m_locked)
            {
                throw new Exception("Cannot Write to a locked Packet.");
            }
            m_writer.Write(values);
        }
        public void WriteUInt8Array(byte[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                for (int x = index; x < index + count; ++x)
                {
                    m_writer.Write(values[x]);
                }
            }
        }
        public void WriteUInt16Array(UInt16[] values)
        {
            WriteUInt16Array(values, 0, values.Length);
        }
        public void WriteUInt16Array(UInt16[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                for (int x = index; x < index + count; ++x)
                {
                    m_writer.Write(values[x]);
                }
            }
        }
        public void WriteInt16Array(Int16[] values)
        {
            WriteInt16Array(values, 0, values.Length);
        }
        public void WriteInt16Array(Int16[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                for (int x = index; x < index + count; ++x)
                {
                    m_writer.Write(values[x]);
                }
            }
        }
        public void WriteUInt32Array(UInt32[] values)
        {
            WriteUInt32Array(values, 0, values.Length);
        }
        public void WriteUInt32Array(UInt32[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                for (int x = index; x < index + count; ++x)
                {
                    m_writer.Write(values[x]);
                }
            }
        }
        public void WriteInt32Array(Int32[] values)
        {
            WriteInt32Array(values, 0, values.Length);
        }
        public void WriteInt32Array(Int32[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                for (int x = index; x < index + count; ++x)
                {
                    m_writer.Write(values[x]);
                }
            }
        }
        public void WriteUInt64Array(UInt64[] values)
        {
            WriteUInt64Array(values, 0, values.Length);
        }
        public void WriteUInt64Array(UInt64[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                for (int x = index; x < index + count; ++x)
                {
                    m_writer.Write(values[x]);
                }
            }
        }
        public void WriteInt64Array(Int64[] values)
        {
            WriteInt64Array(values, 0, values.Length);
        }
        public void WriteInt64Array(Int64[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                for (int x = index; x < index + count; ++x)
                {
                    m_writer.Write(values[x]);
                }
            }
        }
        public void WriteSingleArray(float[] values)
        {
            WriteSingleArray(values, 0, values.Length);
        }
        public void WriteSingleArray(float[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                for (int x = index; x < index + count; ++x)
                {
                    m_writer.Write(values[x]);
                }
            }
        }
        public void WriteDoubleArray(double[] values)
        {
            WriteDoubleArray(values, 0, values.Length);
        }
        public void WriteDoubleArray(double[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                for (int x = index; x < index + count; ++x)
                {
                    m_writer.Write(values[x]);
                }
            }
        }
        public void WriteAsciiArray(String[] values, int codepage)
        {
            WriteAsciiArray(values, 0, values.Length, codepage);
        }
        public void WriteAsciiArray(String[] values, int index, int count, int codepage)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                for (int x = index; x < index + count; ++x)
                {
                    WriteAscii(values[x], codepage);
                }
            }
        }
        public void WriteAsciiArray(String[] values)
        {
            WriteAsciiArray(values, 0, values.Length, 1252);
        }
        public void WriteAsciiArray(String[] values, int index, int count)
        {
            WriteAsciiArray(values, index, count, 1252);
        }
        public void WriteUnicodeArray(String[] values)
        {
            WriteUnicodeArray(values, 0, values.Length);
        }
        public void WriteUnicodeArray(String[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                for (int x = index; x < index + count; ++x)
                {
                    WriteUnicode(values[x]);
                }
            }
        }

        public void WriteUInt8Array(object[] values)
        {
            WriteUInt8Array(values, 0, values.Length);
        }
        public void WriteUInt8Array(object[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                for (int x = index; x < index + count; ++x)
                {
                    WriteUInt8(values[x]);
                }
            }
        }
        public void WriteCharArray(char[] values)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                m_writer.Write(values);
            }
        }
        public void WriteInt8Array(object[] values)
        {
            WriteInt8Array(values, 0, values.Length);
        }
        public void WriteInt8Array(object[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                for (int x = index; x < index + count; ++x)
                {
                    WriteInt8(values[x]);
                }
            }
        }
        public void WriteUInt16Array(object[] values)
        {
            WriteUInt16Array(values, 0, values.Length);
        }
        public void WriteUInt16Array(object[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                for (int x = index; x < index + count; ++x)
                {
                    WriteUInt16(values[x]);
                }
            }
        }
        public void WriteInt16Array(object[] values)
        {
            WriteInt16Array(values, 0, values.Length);
        }
        public void WriteInt16Array(object[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                for (int x = index; x < index + count; ++x)
                {
                    WriteInt16(values[x]);
                }
            }
        }
        public void WriteUInt32Array(object[] values)
        {
            WriteUInt32Array(values, 0, values.Length);
        }
        public void WriteUInt32Array(object[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                for (int x = index; x < index + count; ++x)
                {
                    WriteUInt32(values[x]);
                }
            }
        }
        public void WriteInt32Array(object[] values)
        {
            WriteInt32Array(values, 0, values.Length);
        }
        public void WriteInt32Array(object[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                for (int x = index; x < index + count; ++x)
                {
                    WriteInt32(values[x]);
                }
            }
        }
        public void WriteUInt64Array(object[] values)
        {
            WriteUInt64Array(values, 0, values.Length);
        }
        public void WriteUInt64Array(object[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                for (int x = index; x < index + count; ++x)
                {
                    WriteUInt64(values[x]);
                }
            }
        }
        public void WriteInt64Array(object[] values)
        {
            WriteInt64Array(values, 0, values.Length);
        }
        public void WriteInt64Array(object[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                for (int x = index; x < index + count; ++x)
                {
                    WriteInt64(values[x]);
                }
            }
        }
        public void WriteSingleArray(object[] values)
        {
            WriteSingleArray(values, 0, values.Length);
        }
        public void WriteSingleArray(object[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                for (int x = index; x < index + count; ++x)
                {
                    WriteSingle(values[x]);
                }
            }
        }
        public void WriteDoubleArray(object[] values)
        {
            WriteDoubleArray(values, 0, values.Length);
        }
        public void WriteDoubleArray(object[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                for (int x = index; x < index + count; ++x)
                {
                    WriteDouble(values[x]);
                }
            }
        }
        public void WriteAsciiArray(object[] values, int codepage)
        {
            WriteAsciiArray(values, 0, values.Length, codepage);
        }
        public void WriteAsciiArray(object[] values, int index, int count, int codepage)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                for (int x = index; x < index + count; ++x)
                {
                    WriteAscii(values[x].ToString(), codepage);
                }
            }
        }
        public void WriteAsciiArray(object[] values)
        {
            WriteAsciiArray(values, 0, values.Length, 1252);
        }
        public void WriteAsciiArray(object[] values, int index, int count)
        {
            WriteAsciiArray(values, index, count, 1252);
        }
        public void WriteUnicodeArray(object[] values)
        {
            WriteUnicodeArray(values, 0, values.Length);
        }

        public void WriteUnicodeArray(object[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (m_locked)
                {
                    throw new Exception("Cannot Write to a locked Packet.");
                }
                for (int x = index; x < index + count; ++x)
                {
                    WriteUnicode(values[x].ToString());
                }
            }
        }
    }
}
