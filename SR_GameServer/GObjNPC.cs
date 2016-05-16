using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_GameServer
{
    public class GObjNPC : GObj
    {
        #region Public Properties and Fields

        public GObjTalkFlags m_talkFlag;
        public List<byte> m_talkOptions;

        #endregion

        #region Constructors and Destructors

        public GObjNPC()
            : base (GObjType.GObjNPC)
        {
            m_talkOptions = new List<byte>();
        }

        #endregion
    }
}
