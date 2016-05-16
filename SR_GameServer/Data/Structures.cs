namespace SR_GameServer.Data
{
    using System;
    using System.Collections.Generic;

    public struct AccountInfo
    {
        public int SID;
        public string StrUserID;
        public string Password;
        public AuthType Auth;
        public List<Tuple<int, int>> Characters; //charid, index, deleting
    }

    public struct _teleport_restrict
    {
        public int Restrict;
        public int Data1, Data2;
    }
}
