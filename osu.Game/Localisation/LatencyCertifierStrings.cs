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
        /// "Approach Rate"
        /// </summary>
        public static LocalisableString ApproachRate => new TranslatableString(getKey(@"approach_rate"), @"Approach Rate");

        /// <summary>
        /// "Welcome to the latency certifier!"
        /// </summary>
        public static LocalisableString WelcomeToLatencyCertifier => new TranslatableString(getKey(@"welcome_to_latency_certifier"), @"Welcome to the latency certifier!");

        /// <summary>
        /// "Do whatever you need to try and perceive the difference in latency, then choose your best side. Read more about the methodology [here]({0})."
        /// </summary>
        public static LocalisableString LatencyCertifierMethodology(string url) => new TranslatableString(getKey(@"latency_certifier_methodology"), @"Do whatever you need to try and perceive the difference in latency, then choose your best side. Read more about the methodology [here]({0}).", url);

        /// <summary>
        /// "Use the arrow keys or Z/X/F/J to control the display.
        /// Tab key to change focus. Space to change display mode."
        /// </summary>
        public static LocalisableString DisplayControls => new TranslatableString(getKey(@"display_controls"), @"Use the arrow keys or Z/X/F/J to control the display.
Tab key to change focus. Space to change display mode.");

        /// <summary>
        /// "unknown"
        /// </summary>
        public static LocalisableString ExclusiveFullscreenCapabilityUnknown => new TranslatableString(getKey(@"exclusive_fullscreen_capability_unknown"), @"unknown");

        /// <summary>
        /// "You scored {0} out of {1} ({2:0%})!"
        /// </summary>
        public static LocalisableString ScoreInfo(int correctAttempts, int totalAttempts, float successRate) => new TranslatableString(getKey(@"score_info"), @"You scored {0} out of {1} ({2:0%})!", correctAttempts, totalAttempts, successRate);

        /// <summary>
        /// "Level {0} ({1} Hz)"
        /// </summary>
        public static LocalisableString LevelInfo(int difficultyLevel, int refreshRate) => new TranslatableString(getKey(@"level_info"), @"Level {0} ({1} Hz)", difficultyLevel, refreshRate);

        /// <summary>
        /// "Level {0}
        /// Round {1} of {2}"
        /// </summary>
        public static LocalisableString LevelAndRoundInfo(int difficultyLevel, int currentRound, int totalRounds) => new TranslatableString(getKey(@"level_and_round_info"), @"Level {0}
Round {1} of {2}", difficultyLevel, currentRound, totalRounds);

        /// <summary>
        /// "To complete certification, the difficulty level will now decrease until you can get {0} rounds correct in a row!"
        /// </summary>
        public static LocalisableString CertificationRequirements(int roundsCount) => new TranslatableString(getKey(@"certification_requirements"), @"To complete certification, the difficulty level will now decrease until you can get {0} rounds correct in a row!", roundsCount);

        /// <summary>
        /// "Polling: {0} Hz | Monitor: {1} Hz | Exclusive: {2}
        /// Input: {3} Hz | Update: {4} Hz | Draw: {5} Hz"
        /// </summary>
        public static LocalisableString RefreshRatesInfo(int pollingRate, int refreshRate, LocalisableString exclusiveStatus, int inputRate, int updateRate, int drawRate) => new TranslatableString(getKey(@"refresh_rates_info"), @"Polling: {0} Hz | Monitor: {1} Hz | Exclusive: {2}
Input: {3} Hz | Update: {4} Hz | Draw: {5} Hz", pollingRate, refreshRate, exclusiveStatus, inputRate, updateRate, drawRate);

        /// <summary>
        /// "You've reached the maximum level."
        /// </summary>
        public static LocalisableString CannotIncreaseLevelDueToMaximumLevel => new TranslatableString(getKey(@"cannot_increase_level_due_to_maximum_level"), @"You've reached the maximum level.");

        /// <summary>
        /// "Game is not running fast enough to test this level."
        /// </summary>
        public static LocalisableString CannotIncreaseLevelDueToLowPerformance => new TranslatableString(getKey(@"cannot_increase_level_due_to_low_performance"), @"Game is not running fast enough to test this level.");

        /// <summary>
        /// "Continue to next level"
        /// </summary>
        public static LocalisableString ContinueToNextLevel => new TranslatableString(getKey(@"continue_to_next_level"), @"Continue to next level");

        /// <summary>
        /// "Retry"
        /// </summary>
        public static LocalisableString Retry => new TranslatableString(getKey(@"retry"), @"Retry");

        /// <summary>
        /// "Are you even trying..?"
        /// </summary>
        public static LocalisableString AreYouEvenTrying => new TranslatableString(getKey(@"are_you_even_trying"), @"Are you even trying..?");

        /// <summary>
        /// "Begin certification at last level"
        /// </summary>
        public static LocalisableString BeginCertificationAtLastLevel => new TranslatableString(getKey(@"begin_certification_at_last_level"), @"Begin certification at last level");

        /// <summary>
        /// "Chain {0} rounds to confirm your perception!"
        /// </summary>
        public static LocalisableString ChainRoundsToConfirmYourPerception(int roundsCount) => new TranslatableString(getKey(@"chain_rounds_to_confirm_your_perception"), @"Chain {0} rounds to confirm your perception!", roundsCount);

        /// <summary>
        /// "You've reached your limits. Go to the previous level to complete certification!"
        /// </summary>
        public static LocalisableString GoToPreviousLevelToCompleteCertification => new TranslatableString(getKey(@"go_to_previous_level_to_complete_certification"), @"You've reached your limits. Go to the previous level to complete certification!");

        /// <summary>
        /// "Certified!"
        /// </summary>
        public static LocalisableString Certified => new TranslatableString(getKey(@"certified"), @"Certified!");

        /// <summary>
        /// "You should use a frame limiter with update rate of {0} Hz (or fps) for best results!"
        /// </summary>
        public static LocalisableString ResultsWithRecommendedRefreshRate(int recommendedRefreshRate) => new TranslatableString(getKey(@"results_with_recommended_refresh_rate"), @"You should use a frame limiter with update rate of {0} Hz (or fps) for best results!", recommendedRefreshRate);

        /// <summary>
        /// "Feels better"
        /// </summary>
        public static LocalisableString FeelsBetter => new TranslatableString(getKey(@"feels_better"), @"Feels better");

        /// <summary>
        /// "{0} (Press {1})"
        /// </summary>
        public static LocalisableString PressButtonToPerformAction(LocalisableString action, string key) => new TranslatableString(getKey(@"press_button_to_perform_action"), @"{0} (Press {1})", action, key);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
