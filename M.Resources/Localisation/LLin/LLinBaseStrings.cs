using osu.Framework.Localisation;

namespace M.Resources.Localisation.LLin
{
    public static class LLinBaseStrings
    {
        private const string prefix = @"M.Resources.Localisation.LLin.BaseStrings";

        public static LocalisableString Exit => new TranslatableString(getKey(@"exit"), @"退出");

        public static LocalisableString Manual => new TranslatableString(getKey(@"manual"), @"食用手册");

        public static LocalisableString PrevOrRestart => new TranslatableString(getKey(@"prev_restart"), @"上一首 / 重新开始");

        public static LocalisableString Next => new TranslatableString(getKey(@"next"), @"下一首");

        public static LocalisableString TogglePause => new TranslatableString(getKey(@"toggle_pause"), @"暂停 / 播放");

        public static LocalisableString ViewPlugins => new TranslatableString(getKey(@"view_plugins"), @"查看插件");

        public static LocalisableString HideAndLockInterface => new TranslatableString(getKey(@"hide_and_lock_interface"), @"锁定变更并隐藏界面");

        public static LocalisableString LockInterface => new TranslatableString(getKey(@"lock_interface"), @"锁定变更");

        public static LocalisableString ToggleLoop => new TranslatableString(getKey(@"toggle_loop"), @"单曲循环");

        public static LocalisableString ViewInSongSelect => new TranslatableString(getKey(@"view_in_song_select"), @"在单人游戏选歌界面中查看");

        public static LocalisableString OpenSidebar => new TranslatableString(getKey(@"open_sidebar"), @"侧边栏");

        public static LocalisableString AudioControlRequestedMain => new TranslatableString(getKey(@"open_sidebar"), @"请求接手音频控制。");

        public static LocalisableString AudioControlRequestedSub(string reason) => new TranslatableString(getKey(@"open_sidebar"), @"原因是: {0}", reason);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
