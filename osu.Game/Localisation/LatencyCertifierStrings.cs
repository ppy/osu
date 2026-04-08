// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class LatencyCertifierStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.LatencyCertifier";

        /// <summary>
        /// "Visual Spacing"
        /// </summary>
        public static LocalisableString VisualSpacing => new TranslatableString(getKey(@"visual_spacing"), @"Visual Spacing");

        /// <summary>
        /// "Welcome to the latency certifier!"
        /// </summary>
        public static LocalisableString ExplanatoryTextWelcome => new TranslatableString(getKey(@"explanatory_text_welcome"), @"Welcome to the latency certifier!");

        /// <summary>
        /// "Do whatever you need to try and perceive the difference in latency, then choose your best side. Read more about the methodology [here]({0})."
        /// </summary>
        public static LocalisableString ExplanatoryTextMethodology(string url) => new TranslatableString(getKey(@"explanatory_text_methodology"), @"Do whatever you need to try and perceive the difference in latency, then choose your best side. Read more about the methodology [here]({0}).", url);

        /// <summary>
        /// "Use the arrow keys or Z/X/F/J to control the display."
        /// </summary>
        public static LocalisableString ExplanatoryTextControlDisplay => new TranslatableString(getKey(@"explanatory_text_control_display"), @"Use the arrow keys or Z/X/F/J to control the display.");

        /// <summary>
        /// "Tab key to change focus. Space to change display mode."
        /// </summary>
        public static LocalisableString ExplanatoryTextDisplayMode => new TranslatableString(getKey(@"explanatory_text_display_mode"), @"Tab key to change focus. Space to change display mode.");

        /// <summary>
        /// "unknown"
        /// </summary>
        public static LocalisableString ExclusiveUnknown => new TranslatableString(getKey(@"exclusive_unknown"), @"unknown");

        /// <summary>
        /// "You scored {0} out of {1} ({2:0%})!"
        /// </summary>
        public static LocalisableString StatusTextScore(int correctCount, int totalCount, float successRate) => new TranslatableString(getKey(@"status_text_score"), @"You scored {0} out of {1} ({2:0%})!", correctCount, totalCount, successRate);

        /// <summary>
        /// "Level {0} ({1:N0} Hz)"
        /// </summary>
        public static LocalisableString StatusTextLevel(int difficultyLevel, int frameRate) => new TranslatableString(getKey(@"status_text_level"), @"Level {0} ({1:N0} Hz)", difficultyLevel, frameRate);

        /// <summary>
        /// "Level {0}
        /// Round {1} of {2}"
        /// </summary>
        public static LocalisableString StatusTextLevelWithRound(int difficultyLevel, int currentRound, int totalRounds) => new TranslatableString(getKey(@"status_text_level_with_round"), @"Level {0}
Round {1} of {2}", difficultyLevel, currentRound, totalRounds);

        /// <summary>
        /// "To complete certification, the difficulty level will now decrease until you can get {0} rounds correct in a row!"
        /// </summary>
        public static LocalisableString StatusTextCertification(int roundsCount) => new TranslatableString(getKey(@"status_text_certification"), @"To complete certification, the difficulty level will now decrease until you can get {0} rounds correct in a row!", roundsCount);

        /// <summary>
        /// "Polling: {0} Hz | Monitor: {1:N0} Hz | Exclusive: {2}"
        /// </summary>
        public static LocalisableString StatusTextMonitor(int pollingRate, float refreshRate, LocalisableString exclusiveStatus) => new TranslatableString(getKey(@"status_text_monitor"), @"Polling: {0} Hz | Monitor: {1:N0} Hz | Exclusive: {2}", pollingRate, refreshRate, exclusiveStatus);

        /// <summary>
        /// "Input: {0} Hz | Update: {1} Hz | Draw: {2} Hz"
        /// </summary>
        public static LocalisableString StatusTextRendering(double inputRate, double updateRate, double drawRate) => new TranslatableString(getKey(@"status_text_rendering"), @"Input: {0} Hz | Update: {1} Hz | Draw: {2} Hz", inputRate, updateRate, drawRate);

        /// <summary>
        /// "You&#39;ve reached the maximum level."
        /// </summary>
        public static LocalisableString CannotIncreaseMaximumLevel => new TranslatableString(getKey(@"cannot_increase_maximum_level"), @"You've reached the maximum level.");

        /// <summary>
        /// "Game is not running fast enough to test this level!"
        /// </summary>
        public static LocalisableString CannotIncreaseLowPerformance => new TranslatableString(getKey(@"cannot_increase_low_performance"), @"Game is not running fast enough to test this level!");

        /// <summary>
        /// "Continue to the next level"
        /// </summary>
        public static LocalisableString NextLevel => new TranslatableString(getKey(@"next_level"), @"Continue to the next level");

        /// <summary>
        /// "Are you even trying..?"
        /// </summary>
        public static LocalisableString RetryTooltip => new TranslatableString(getKey(@"retry_tooltip"), @"Are you even trying..?");

        /// <summary>
        /// "Begin certification at last level"
        /// </summary>
        public static LocalisableString BeginCertificationText => new TranslatableString(getKey(@"begin_certification_text"), @"Begin certification at last level");

        /// <summary>
        /// "Chain {0} rounds to confirm your perception!"
        /// </summary>
        public static LocalisableString BeginCertificationTooltipConfirm(int roundsCount) => new TranslatableString(getKey(@"begin_certification_tooltip_confirm"), @"Chain {0} rounds to confirm your perception!", roundsCount);

        /// <summary>
        /// "You've reached your limits. Go to the previous level to complete certification!"
        /// </summary>
        public static LocalisableString BeginCertificationTooltipComplete => new TranslatableString(getKey(@"begin_certification_tooltip_complete"), @"You've reached your limits. Go to the previous level to complete certification!");

        /// <summary>
        /// "Certified!"
        /// </summary>
        public static LocalisableString Certified => new TranslatableString(getKey(@"certified"), @"Certified!");

        /// <summary>
        /// "You should use a frame limiter with update rate of {0} Hz (or FPS) for best results!"
        /// </summary>
        public static LocalisableString Results(int recommendedRate) => new TranslatableString(getKey(@"results"), @"You should use a frame limiter with update rate of {0} Hz (or FPS) for best results!", recommendedRate);

        /// <summary>
        /// "Feels better"
        /// </summary>
        public static LocalisableString FeelsBetter => new TranslatableString(getKey(@"feels_better"), @"Feels better");

        /// <summary>
        /// "{0} (Press {1})"
        /// </summary>
        public static LocalisableString ButtonWithKeyBindText(LocalisableString action, string keyString) => new TranslatableString(getKey(@"button_with_key_bind_text"), @"{0} (Press {1})", action, keyString);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
