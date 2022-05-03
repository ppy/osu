// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class FirstRunSetupBeatmapScreenStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.FirstRunSetupBeatmapScreen";

        /// <summary>
        /// "Obtaining Beatmaps"
        /// </summary>
        public static LocalisableString Header => new TranslatableString(getKey(@"llin_header"), @"获取谱面");

        /// <summary>
        /// "&quot;Beatmaps&quot; are what we call playable levels. osu! doesn&#39;t come with any beatmaps pre-loaded. This step will help you get started on your beatmap collection."
        /// </summary>
        public static LocalisableString Description => new TranslatableString(getKey(@"llin_description"), @"我们通常称呼那些可游玩的关卡为“谱面”。 osu!并不自带任何谱面，因此这一步将帮助您获取您的第一张图。");

        /// <summary>
        /// "If you are a new player, we recommend playing through the tutorial to get accustomed to the gameplay."
        /// </summary>
        public static LocalisableString TutorialDescription => new TranslatableString(getKey(@"llin_tutorial_description"), @"如果您是名新手玩家，我们建议先通过教程来掌握一些游玩的基础知识。");

        /// <summary>
        /// "Get the osu! tutorial"
        /// </summary>
        public static LocalisableString TutorialButton => new TranslatableString(getKey(@"llin_tutorial_button"), @"获取osu!教程");

        /// <summary>
        /// "To get you started, we have some recommended beatmaps."
        /// </summary>
        public static LocalisableString BundledDescription => new TranslatableString(getKey(@"llin_bundled_description"), @"在开始之前，我们这有一些推荐的谱面可以试试。");

        /// <summary>
        /// "Get recommended beatmaps"
        /// </summary>
        public static LocalisableString BundledButton => new TranslatableString(getKey(@"llin_bundled_button"), @"获取推荐谱面");

        /// <summary>
        /// "You can also obtain more beatmaps from the main menu &quot;browse&quot; button at any time."
        /// </summary>
        public static LocalisableString ObtainMoreBeatmaps => new TranslatableString(getKey(@"llin_obtain_more_beatmaps"), @"您也可以通过主菜单的“浏览谱面”按钮获取更多谱面");

        /// <summary>
        /// "You currently have {0} beatmap(s) loaded!"
        /// </summary>
        public static LocalisableString CurrentlyLoadedBeatmaps(int beatmaps) => new TranslatableString(getKey(@"llin_currently_loaded_beatmaps"), @"你目前共有{0}张图！", beatmaps);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
