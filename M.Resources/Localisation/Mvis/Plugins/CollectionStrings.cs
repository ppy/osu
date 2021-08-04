using osu.Framework.Localisation;

namespace M.Resources.Localisation.Mvis.Plugins
{
    public static class CollectionStrings
    {
        private const string prefix = @"M.Resources.Localisation.Mvis.Plugins.YaspStrings";

        public static LocalisableString NoCollectionSelected => new TranslatableString(getKey(@"no_collection_selected"), @"未选择收藏夹");

        public static LocalisableString SelectOneFirst => new TranslatableString(getKey(@"select_one_first"), @"请先选择一个！");

        public static LocalisableString AudioControlRequest => new TranslatableString(getKey(@"audio_control_request"), "激活以确保插件可以发挥功能\n本提示在本次会话中不会出现第二次。");

        public static LocalisableString EntryTooltip => new TranslatableString(getKey(@"entry_tooltip"), "浏览收藏夹");

        public static LocalisableString SongCount(int count) => new TranslatableString(getKey(@"song_count"), @"{0}首歌曲", count);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
