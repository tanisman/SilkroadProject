namespace SR_GameServer.Data.NavMesh
{
    using System;
    using System.IO;
    using System.Globalization;

    using SCommon;

    public static class JmxObj
    {
        private static _nvm_link_bsr[] s_List;

        public static void Load()
        {
            using (var reader = new StreamReader(Path.Combine(Environment.CurrentDirectory, "data\\navmesh\\object.ifo")))
            {
                if (reader.ReadLine() == "JMXVOBJI1000")
                {
                    int count = Convert.ToInt32(reader.ReadLine());
                    s_List = new _nvm_link_bsr[count];

                    while(!reader.EndOfStream)
                    {
                        string[] data = reader.ReadLine().Split(' ');

                        _nvm_link_bsr link = new _nvm_link_bsr();
                        link.model = Convert.ToUInt32(data[0]);
                        link.unk = Int32.Parse(data[1].Replace("0x", ""), NumberStyles.HexNumber);
                        link.directory = data[2].Replace('"', ' ').Replace(" ", "");

                        s_List[link.model] = link;
                    }
                }
                else
                    Logging.Log()("Wrong Ifo File Format", LogLevel.Error);
            }
        }

        public static _nvm_link_bsr[] Items => s_List;
    }
}
