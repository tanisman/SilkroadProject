namespace SR_GameServer
{
    using System;

    using SCommon;

    public class GObjItem : GObj
    {
        #region Public Properties and Fields

        public int m_data;
        public byte m_optLevel;
        public GObjChar m_owner;

        public bool IsGold => Data.Globals.Ref.ObjItem[m_model].Type == Data.ItemType.GOLD;
        public bool IsQuest => Data.Globals.Ref.ObjItem[m_model].Type == Data.ItemType.EVENT_ITEM;
        public bool IsGoods => Data.Globals.Ref.ObjItem[m_model].Type == Data.ItemType.ETC_TRADE_ITEM;

        #endregion

        #region Constructors & Destructors

        public GObjItem()
            : base (GObjType.GObjItem)
        {
            StartDisappear(60000);
        }

        #endregion

        #region Private Methods

        protected override void DisappearTimer_Callback(object sender, object state)
        {
            base.DisappearTimer_Callback(sender, state);
            m_owner = null;
        }

        #endregion
    }
}
