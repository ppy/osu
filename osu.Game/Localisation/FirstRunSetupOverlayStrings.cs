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
        public static LocalisableString GetStarted => new TranslatableString(getKey(@"get_started"), @"Get started");

        /// <summary>
        /// "Click to resume first-run setup at any point"
        /// </summary>
        public static LocalisableString ClickToResumeFirstRunSetupAtAnyPoint =>
            new TranslatableString(getKey(@"click_to_resume_first_run_setup_at_any_point"), @"Click to resume first-run setup at any point");

        /// <summary>
        /// "First-run setup"
        /// </summary>
        public static LocalisableString FirstRunSetupTitle => new TranslatableString(getKey(@"first_run_setup_title"), @"First-run setup");

        /// <summary>
        /// "Set up osu! to suit you"
        /// </summary>
        public static LocalisableString FirstRunSetupDescription => new TranslatableString(getKey(@"first_run_setup_description"), @"Set up osu! to suit you");

        /// <summary>
        /// "Welcome"
        /// </summary>
        public static LocalisableString WelcomeTitle => new TranslatableString(getKey(@"welcome_title"), @"Welcome");

        /// <summary>
        /// "Welcome to the first-run setup guide!
        ///
        /// osu! is a very configurable game, and diving straight into the settings can sometimes be overwhelming. This guide will help you get the important choices out of the way to ensure a great first experience!"
        /// </summary>
        public static LocalisableString WelcomeDescription => new TranslatableString(getKey(@"welcome_description"), @"Welcome to the first-run setup guide!

osu! is a very configurable game, and diving straight into the settings can sometimes be overwhelming. This guide will help you get the important choices out of the way to ensure a great first experience!");

        /// <summary>
        /// "The size of the osu! user interface can be adjusted to your liking."
        /// </summary>
        public static LocalisableString UIScaleDescription => new TranslatableString(getKey(@"ui_scale_description"), @"The size of the osu! user interface can be adjusted to your liking.");

        /// <summary>
        /// "Behaviour"
        /// </summary>
        public static LocalisableString Behaviour => new TranslatableString(getKey(@"behaviour"), @"Behaviour");

        /// <summary>
        /// "Some new defaults for game behaviours have been implemented, with the aim of improving the game experience and making it more accessible to everyone.
        ///
        /// We recommend you give the new defaults a try, but if you&#39;d like to have things feel more like classic versions of osu!, you can easily apply some sane defaults below."
        /// </summary>
        public static LocalisableString BehaviourDescription => new TranslatableString(getKey(@"behaviour_description"),
            @"Some new defaults for game behaviours have been implemented, with the aim of improving the game experience and making it more accessible to everyone.

We recommend you give the new defaults a try, but if you'd like to have things feel more like classic versions of osu!, you can easily apply some sane defaults below.");

        /// <summary>
        /// "New defaults"
        /// </summary>
        public static LocalisableString NewDefaults => new TranslatableString(getKey(@"new_defaults"), @"New defaults");

        /// <summary>
        /// "Classic defaults"
        /// </summary>
        public static LocalisableString ClassicDefaults => new TranslatableString(getKey(@"classic_defaults"), @"Classic defaults");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
