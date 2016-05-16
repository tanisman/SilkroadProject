namespace SCommon
{
    using System;
    using System.Text;
    using System.IO;
    using System.Security.Cryptography;

    using SCommon.Security;

    using SharpDX;

    public static class Utility
    {
        #region Private Properties and Fields

        /// <summary>
        /// The md5 cipher
        /// </summary>
        private static MD5 s_Cipher;

        #endregion

        #region Constructors & Destructors

        static Utility()
        {
            s_Cipher = MD5.Create();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Computes the hash of the given text.
        /// </summary>
        /// <param name="text">The text in ASCII encoding.</param>
        /// <returns>The MD5 hash</returns>
        public static byte[] MD5Hash(string text)
        {
            return s_Cipher.ComputeHash(Encoding.ASCII.GetBytes(text));
        }

        /// <summary>
        /// Computes the hash of the given text.
        /// </summary>
        /// <param name="text">The text in ASCII encoding.</param>
        /// <param name="lowercase">if <c>true</c>, returns string in lowercase</param>
        /// <returns>The MD5 hash as string</returns>
        public static string MD5Hash(string text, bool lowercase)
        {
            byte[] hash = MD5Hash(text);
            string builder = string.Empty;
            foreach (byte b in hash)
                builder += b.ToString(lowercase ? "x2" : "X2");

            return builder;
        }

        /// <summary>
        /// Returns dump string of the given packet
        /// </summary>
        /// <param name="packet">The packet.</param>
        /// <returns>Packet dump as string</returns>
        public static string HexDump(this Packet packet, bool sc_true_or_cs_false)
        {
            return String.Format("[{0}][Opcode:0x{3:X4}][Encrypted:{1}][Massive:{2}]{4}{5}", sc_true_or_cs_false ? "S->C" : "C->S", packet.Encrypted, packet.Massive, packet.Opcode, Environment.NewLine, HexDump(packet.GetBytes()));
        }

        /// <summary>
        /// Returns dump string of the given buffer
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>buffer dump as string</returns>
        public static string HexDump(this byte[] buffer)
        {
            return HexDump(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Returns dump string of the given buffer
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>buffer dump as string</returns>
        public static string HexDump(this byte[] buffer, int offset, int count)
        {
            const int bytesPerLine = 16;
            StringBuilder output = new StringBuilder();
            StringBuilder ascii_output = new StringBuilder();
            int length = count;
            if (length % bytesPerLine != 0)
            {
                length += bytesPerLine - length % bytesPerLine;
            }
            for (int x = 0; x <= length; ++x)
            {
                if (x % bytesPerLine == 0)
                {
                    if (x > 0)
                    {
                        output.AppendFormat("  {0}{1}", ascii_output.ToString(), Environment.NewLine);
                        ascii_output.Clear();
                    }
                    if (x != length)
                    {
                        output.AppendFormat("{0:d10}   ", x);
                    }
                }
                if (x < count)
                {
                    output.AppendFormat("{0:X2} ", buffer[offset + x]);
                    char ch = (char)buffer[offset + x];
                    if (!Char.IsControl(ch))
                    {
                        ascii_output.AppendFormat("{0}", ch);
                    }
                    else
                    {
                        ascii_output.Append(".");
                    }
                }
                else
                {
                    output.Append("   ");
                    ascii_output.Append(".");
                }
            }
            return output.ToString();
        }

        public static string ReadAscii(this BinaryReader reader)
        {
            return new string(reader.ReadChars(reader.ReadInt32()));
        }

        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
        
        #endregion
    }
}
