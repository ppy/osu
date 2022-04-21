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
        public static LocalisableString ClickToResumeFirstRunSetupAtAnyPoint => new TranslatableString(getKey(@"click_to_resume_first_run_setup_at_any_point"), @"Click to resume first-run setup at any point");

        /// <summary>
        /// "First-run setup"
        /// </summary>
        public static LocalisableString FirstRunSetup => new TranslatableString(getKey(@"first_run_setup"), @"First-run setup");

        /// <summary>
        /// "Setup osu! to suit you"
        /// </summary>
        public static LocalisableString SetupOsuToSuitYou => new TranslatableString(getKey(@"setup_osu_to_suit_you"), @"Setup osu! to suit you");

        /// <summary>
        /// "Welcome"
        /// </summary>
        public static LocalisableString Welcome => new TranslatableString(getKey(@"welcome"), @"Welcome");

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
        /// "Next ({0})"
        /// </summary>
        public static LocalisableString Next(LocalisableString nextStepDescription) => new TranslatableString(getKey(@"next"), @"Next ({0})", nextStepDescription);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
