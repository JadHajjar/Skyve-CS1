namespace LoadOrderShared {
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;

    public class CSCache {
        public class Item {
            public string IncludedPath;
            public string Name;
            public string Description;
        }

        public class Mod : Item { }

        public class Asset : Item {
            public string[] Tags;
        }

        public const string FILE_NAME = "CSCache.xml";

        public string WorkShopContentPath;
        public string GamePath;
        public string SteamPath;


        // do not use these directly. use Add/GetItem instead.
        public Mod[] Mods = new Mod[0];
        public Asset[] Assets = new Asset[0];
        public uint[] Dlcs = new uint[0];

		/// <summary>
		/// missing root dir
		/// </summary>
		public ulong[] MissingDir = new ulong[0];

        internal Dictionary<string, Item> ItemTable = new(100000);

        public void AddItem(Item item) {
            ItemTable[item.IncludedPath] = item;
        }

        public Item GetItem(string path) {
            if (ItemTable.TryGetValue(path, out var ret))
                return ret;
            else
                return null;
        }

        public static string FilePath => Path.Combine(SharedUtil.LocalLOMData, FILE_NAME);


		public void Serialize() {
            Mods = ItemTable.Values.OfType<Mod>().ToArray();
            Assets = ItemTable.Values.OfType<Asset>().ToArray();
            SharedUtil.Serialize(this, FilePath);
        }

        public static CSCache Deserialize() {
            try {
                var ret = SharedUtil.Deserialize<CSCache>(FilePath);
                foreach (var item in ret.Mods) ret.ItemTable[item.IncludedPath] = item;
                foreach (var item in ret.Assets) ret.ItemTable[item.IncludedPath] = item;
                return ret;
            } catch { }
            return null;
        }
    }
}
