namespace SR_GameServer.Data.RefData
{
    using System;
    using System.Collections.Generic;

    using SCommon;

    public class RefShop
    {
        public int ID, ShopType, NpcID;
        public string CodeName128;
        public _shop_tab[] Tabs;

        public struct _shop_tab
        {
            public int ID;
            public List<_shop_item> Items;
        }

        public struct _shop_item
        {
            public int RefItemID;
            public int GoldPrice;
            public int SilkPrice;
            public byte OptLevel;

            public _shop_item(int rid, int gp, int sp, byte opt)
            {
                RefItemID = rid;
                if (gp <= 0) GoldPrice = Globals.Ref.ObjItem[rid].Price;
                else GoldPrice = gp;
                SilkPrice = sp;
                OptLevel = opt;
            }
        }
    }

    public static class RefShopExtensions
    {
        public static void Load(this List<RefShop> list)
        {
            using (var reader = Globals.ShardDB.ExecuteReader("SELECT * FROM _RefShop WHERE Service = 1 ORDER BY ID"))
            {
                while (reader.Read())
                {
                    var shop = new RefShop();
                    shop.NpcID = (int)reader["NPCID"];
                    shop.ID = (int)reader["ID"];
                    shop.ShopType = (int)reader["ShopType"];
                    shop.Tabs = new RefShop._shop_tab[10];
                    for (byte b = 0; b < 10; b++)
                    {
                        int tabid = (int)reader[String.Format("TabID_{0}", b + 1)];
                        if (tabid > 0)
                        {
                            shop.Tabs[b] = new RefShop._shop_tab();
                            shop.Tabs[b].ID = tabid;
                            shop.Tabs[b].Items = new List<RefShop._shop_item>();
                            using (var reader2 = Globals.ShardDB.ExecuteReader("SELECT * FROM _RefShopGoods WHERE AssocTabID = {0} AND Service = 1", tabid))
                            {
                                while (reader2.Read())
                                    shop.Tabs[b].Items.Add(new RefShop._shop_item((int)reader2["ItemToSell"], (int)reader2["PriceGold"], (int)reader2["PriceSilk"], (byte)reader2["OptLevel"]));
                            }
                        }
                    }
                    list.Add(shop);
                }
            }
            Logging.Log()("Shops are loaded");
        }
    }
}
