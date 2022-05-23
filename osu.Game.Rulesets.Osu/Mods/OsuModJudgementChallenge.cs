// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Mods;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets.Scoring;
using osu.Game.Overlays.Settings;


namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModJudgementChallenge : ModJudgementChallenge
    {
        [SettingSource("Maximum misses", "Maximum number of misses before fail.", SettingControlType = typeof(SettingsSlider<int, JudgementMaxSlider>))]
        public BindableNumber<int> MaxMiss { get; } = new BindableInt
        {
            Default = 26,
            Value = 26,
            MinValue = 0,
            MaxValue = 26
        };

        [SettingSource("Maximum \"meh\" hits", "Maximum number of \"meh\" hits before fail.", SettingControlType = typeof(SettingsSlider<int, JudgementMaxSlider>))]
        public BindableNumber<int> MaxMeh { get; } = new BindableInt
        {
            Default = 26,
            Value = 26,
            MinValue = 0,
            MaxValue = 26
        };

        [SettingSource("Maximum \"ok\" hits", "Maximum number of \"ok\" hits before fail.", SettingControlType = typeof(SettingsSlider<int, JudgementMaxSlider>))]
        public BindableNumber<int> MaxOk { get; } = new BindableInt
        {
            Default = 26,
            Value = 26,
            MinValue = 0,
            MaxValue = 26
        };

        [SettingSource("Maximum \"good\" hits", "Maximum number of \"good\" hits before fail.", SettingControlType = typeof(SettingsSlider<int, JudgementMaxSlider>))]
        public BindableNumber<int> MaxGood { get; } = new BindableInt
        {
            Default = 26,
            Value = 26,
            MinValue = 0,
            MaxValue = 26
        };

        protected override IDictionary<HitResult, BindableNumber<int>> HitResultMaximumCounts => hitResultsMaximumCount;
        private Dictionary<HitResult, BindableNumber<int>> hitResultsMaximumCount = new Dictionary<HitResult, BindableNumber<int>>();

        public OsuModJudgementChallenge()
        {
            hitResultsMaximumCount[HitResult.Miss] = MaxMiss;
            hitResultsMaximumCount[HitResult.Meh] = MaxMeh;
            hitResultsMaximumCount[HitResult.Ok] = MaxOk;
            hitResultsMaximumCount[HitResult.Good] = MaxGood;
        }
    }
}
