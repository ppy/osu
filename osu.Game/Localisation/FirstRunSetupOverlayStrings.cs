// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class FirstRunSetupOverlayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.FirstRunSetupOverlay";

        /// <summary>
        /// "Get started"
        /// </summary>
        public static LocalisableString GetStarted => new TranslatableString(getKey(@"llin_get_started"), @"开始");

        /// <summary>
        /// "Click to resume first-run setup at any point"
        /// </summary>
        public static LocalisableString ClickToResumeFirstRunSetupAtAnyPoint =>
            new TranslatableString(getKey(@"llin_click_to_resume_first_run_setup_at_any_point"), @"点此继续设置");

        /// <summary>
        /// "First-run setup"
        /// </summary>
        public static LocalisableString FirstRunSetupTitle => new TranslatableString(getKey(@"llin_first_run_setup_title"), @"设置向导");

        /// <summary>
        /// "Set up osu! to suit you"
        /// </summary>
        public static LocalisableString FirstRunSetupDescription => new TranslatableString(getKey(@"llin_first_run_setup_description"), @"让osu!更符合你的风格");

        /// <summary>
        /// "Welcome"
        /// </summary>
        public static LocalisableString WelcomeTitle => new TranslatableString(getKey(@"llin_welcome_title"), @"欢迎");

        /// <summary>
        /// "Welcome to the first-run setup guide!
        ///
        /// osu! is a very configurable game, and diving straight into the settings can sometimes be overwhelming. This guide will help you get the important choices out of the way to ensure a great first experience!"
        /// </summary>
        public static LocalisableString WelcomeDescription => new TranslatableString(getKey(@"llin_welcome_description"), @"欢迎来到设置向导！

osu!是一款高度可自定义的游戏，直接点开设置有时可能会让你不知所措, 因此此向导会帮助你优化你的初次体验!

PS：此界面的一些翻译由mfosu单独汉化，并不代表官方品质。");

        /// <summary>
        /// "The size of the osu! user interface can be adjusted to your liking."
        /// </summary>
        public static LocalisableString UIScaleDescription => new TranslatableString(getKey(@"llin_ui_scale_description"), @"osu!的界面大小可以根据你的喜好自由调整");

        /// <summary>
        /// "Behaviour"
        /// </summary>
        public static LocalisableString Behaviour => new TranslatableString(getKey(@"llin_behaviour"), @"行为");

        /// <summary>
        /// "Some new defaults for game behaviours have been implemented, with the aim of improving the game experience and making it more accessible to everyone.
        ///
        /// We recommend you give the new defaults a try, but if you&#39;d like to have things feel more like classic versions of osu!, you can easily apply some sane defaults below."
        /// </summary>
        public static LocalisableString BehaviourDescription => new TranslatableString(getKey(@"llin_behaviour_description"),
            @"为了改进游戏体验和易用性，一些新的行为被添加到了游戏中。

我们建议您先尝试一下新的默认设置，但如果您更喜欢旧版osu!端的体验，您可以在下面轻松地更改一些设置。");

        /// <summary>
        /// "New defaults"
        /// </summary>
        public static LocalisableString NewDefaults => new TranslatableString(getKey(@"llin_new_defaults"), @"新版默认值");

        /// <summary>
        /// "Classic defaults"
        /// </summary>
        public static LocalisableString ClassicDefaults => new TranslatableString(getKey(@"llin_classic_defaults"), @"旧版默认值");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
