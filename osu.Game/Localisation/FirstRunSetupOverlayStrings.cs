// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class FirstRunSetupOverlayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.FirstRunSetupOverlay";

        /// <summary>
        /// "开始"
        /// </summary>
        public static LocalisableString GetStarted => new TranslatableString(getKey(@"llin_get_started"), @"开始");

        /// <summary>
        /// "点此继续设置"
        /// </summary>
        public static LocalisableString ClickToResumeFirstRunSetupAtAnyPoint =>
            new TranslatableString(getKey(@"llin_click_to_resume_first_run_setup_at_any_point"), @"点此继续设置");

        /// <summary>
        /// "设置向导"
        /// </summary>
        public static LocalisableString FirstRunSetupTitle => new TranslatableString(getKey(@"llin_first_run_setup_title"), @"设置向导");

        /// <summary>
        /// "让osu!更符合你的风格"
        /// </summary>
        public static LocalisableString FirstRunSetupDescription => new TranslatableString(getKey(@"llin_first_run_setup_description"), @"让osu!更符合你的风格");

        /// <summary>
        /// "欢迎"
        /// </summary>
        public static LocalisableString WelcomeTitle => new TranslatableString(getKey(@"llin_welcome_title"), @"欢迎");

        /// <summary>
        /// "欢迎来到设置向导！
        ///
        ///osu!是一款高度可自定义的游戏，直接点开设置有时可能会让你不知所措, 因此此向导会帮助你优化你的初次体验!
        ///
        ///PS：此界面的一些翻译由mfosu单独汉化，并不代表官方品质。"
        /// </summary>
        public static LocalisableString WelcomeDescription => new TranslatableString(getKey(@"llin_welcome_description"), @"欢迎来到设置向导！

osu!是一款高度可自定义的游戏，直接点开设置有时可能会让你不知所措, 因此此向导会帮助你优化你的初次体验!

PS：此界面的一些翻译由mfosu单独汉化，并不代表官方品质。");

        /// <summary>
        /// "osu!的界面大小可以根据你的喜好自由调整"
        /// </summary>
        public static LocalisableString UIScaleDescription => new TranslatableString(getKey(@"llin_ui_scale_description"), @"osu!的界面大小可以根据你的喜好自由调整");

        /// <summary>
        /// "行为"
        /// </summary>
        public static LocalisableString Behaviour => new TranslatableString(getKey(@"llin_behaviour"), @"行为");

        /// <summary>
        ///"为了改进游戏体验和易用性，一些新的行为被添加到了游戏中。
        ///
        ///我们建议您先尝试一下新的默认设置，但如果您更喜欢旧版osu!端的体验，您可以在下面轻松地更改一些设置。"
        /// </summary>
        public static LocalisableString BehaviourDescription => new TranslatableString(getKey(@"llin_behaviour_description"),
            @"为了改进游戏体验和易用性，一些新的行为被添加到了游戏中。

我们建议您先尝试一下新的默认设置，但如果您更喜欢旧版osu!端的体验，您可以在下面轻松地更改一些设置。");

        /// <summary>
        /// "新版默认值"
        /// </summary>
        public static LocalisableString NewDefaults => new TranslatableString(getKey(@"llin_new_defaults"), @"新版默认值");

        /// <summary>
        /// "旧版默认值"
        /// </summary>
        public static LocalisableString ClassicDefaults => new TranslatableString(getKey(@"llin_classic_defaults"), @"旧版默认值");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
