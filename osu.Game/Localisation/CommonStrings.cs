// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class CommonStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Common";

        /// <summary>
        /// "返回"
        /// </summary>
        public static LocalisableString Back => new TranslatableString(getKey(@"llin_back"), @"返回");

        /// <summary>
        /// "下一步"
        /// </summary>
        public static LocalisableString Next => new TranslatableString(getKey(@"llin_next"), @"下一步");

        /// <summary>
        /// "Finish"
        /// </summary>
        public static LocalisableString Finish => new TranslatableString(getKey(@"finish"), @"Finish");

        /// <summary>
        /// "已启用"
        /// </summary>
        public static LocalisableString Enabled => new TranslatableString(getKey(@"llin_enabled"), @"已启用");

        /// <summary>
        /// "已禁用"
        /// </summary>
        public static LocalisableString Disabled => new TranslatableString(getKey(@"llin_disabled"), @"已禁用");

        /// <summary>
        /// "默认"
        /// </summary>
        public static LocalisableString Default => new TranslatableString(getKey(@"default"), @"默认");

        /// <summary>
        /// "宽度"
        /// </summary>
        public static LocalisableString Width => new TranslatableString(getKey(@"width"), @"宽度");

        /// <summary>
        /// "高度"
        /// </summary>
        public static LocalisableString Height => new TranslatableString(getKey(@"height"), @"高度");

        /// <summary>
        /// "下载中..."
        /// </summary>
        public static LocalisableString Downloading => new TranslatableString(getKey(@"downloading"), @"下载中...");

        /// <summary>
        /// "导入中..."
        /// </summary>
        public static LocalisableString Importing => new TranslatableString(getKey(@"importing"), @"导入中...");

        /// <summary>
        /// "取消全选"
        /// </summary>
        public static LocalisableString DeselectAll => new TranslatableString(getKey(@"llin_deselect_all"), @"取消全选");

        /// <summary>
        /// "全选"
        /// </summary>
        public static LocalisableString SelectAll => new TranslatableString(getKey(@"llin_select_all"), @"全选");

        /// <summary>
        /// "谱面"
        /// </summary>
        public static LocalisableString Beatmaps => new TranslatableString(getKey(@"llin_beatmaps"), @"谱面");

        /// <summary>
        /// "成绩"
        /// </summary>
        public static LocalisableString Scores => new TranslatableString(getKey(@"llin_scores"), @"成绩");

        /// <summary>
        /// "皮肤"
        /// </summary>
        public static LocalisableString Skins => new TranslatableString(getKey(@"llin_skins"), @"皮肤");

        /// <summary>
        /// "收藏夹"
        /// </summary>
        public static LocalisableString Collections => new TranslatableString(getKey(@"llin_collections"), @"收藏夹");

        /// <summary>
        /// "Mod presets"
        /// </summary>
        public static LocalisableString ModPresets => new TranslatableString(getKey(@"mod_presets"), @"Mod presets");

        /// <summary>
        /// "Name"
        /// </summary>
        public static LocalisableString Name => new TranslatableString(getKey(@"name"), @"Name");

        /// <summary>
        /// "Description"
        /// </summary>
        public static LocalisableString Description => new TranslatableString(getKey(@"description"), @"Description");

        /// <summary>
        /// "File"
        /// </summary>
        public static LocalisableString MenuBarFile => new TranslatableString(getKey(@"menu_bar_file"), @"File");

        /// <summary>
        /// "Edit"
        /// </summary>
        public static LocalisableString MenuBarEdit => new TranslatableString(getKey(@"menu_bar_edit"), @"Edit");

        /// <summary>
        /// "View"
        /// </summary>
        public static LocalisableString MenuBarView => new TranslatableString(getKey(@"menu_bar_view"), @"View");

        /// <summary>
        /// "Undo"
        /// </summary>
        public static LocalisableString Undo => new TranslatableString(getKey(@"undo"), @"Undo");

        /// <summary>
        /// "Redo"
        /// </summary>
        public static LocalisableString Redo => new TranslatableString(getKey(@"redo"), @"Redo");

        /// <summary>
        /// "Cut"
        /// </summary>
        public static LocalisableString Cut => new TranslatableString(getKey(@"cut"), @"Cut");

        /// <summary>
        /// "Copy"
        /// </summary>
        public static LocalisableString Copy => new TranslatableString(getKey(@"copy"), @"Copy");

        /// <summary>
        /// "Paste"
        /// </summary>
        public static LocalisableString Paste => new TranslatableString(getKey(@"paste"), @"Paste");

        /// <summary>
        /// "Clone"
        /// </summary>
        public static LocalisableString Clone => new TranslatableString(getKey(@"clone"), @"Clone");

        /// <summary>
        /// "Exit"
        /// </summary>
        public static LocalisableString Exit => new TranslatableString(getKey(@"exit"), @"Exit");

        /// <summary>
        /// "Revert to default"
        /// </summary>
        public static LocalisableString RevertToDefault => new TranslatableString(getKey(@"revert_to_default"), @"Revert to default");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
