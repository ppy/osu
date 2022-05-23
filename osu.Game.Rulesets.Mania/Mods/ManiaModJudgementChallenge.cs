// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Mods;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Scoring;


namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModJudgementChallenge : ModJudgementChallenge
    {
        [SettingSource("Maximum misses", "Maximum number of misses before fail.", SettingControlType = typeof(SettingsNumberBox))]
        public Bindable<int?> MaxMiss { get; } = new Bindable<int?>
        {
            Default = null,
            Value = null
        };

        [SettingSource("Maximum mehs", "Maximum number of \"meh\" hits before fail.", SettingControlType = typeof(SettingsNumberBox))]
        public Bindable<int?> MaxMeh { get; } = new Bindable<int?>
        {
            Default = null,
            Value = null
        };

        [SettingSource("Maximum oks", "Maximum number of \"ok\" hits before fail.", SettingControlType = typeof(SettingsNumberBox))]
        public Bindable<int?> MaxOk { get; } = new Bindable<int?>
        {
            Default = null,
            Value = null
        };

        [SettingSource("Maximum goods", "Maximum number of \"good\" hits before fail.", SettingControlType = typeof(SettingsNumberBox))]
        public Bindable<int?> MaxGood { get; } = new Bindable<int?>
        {
            Default = null,
            Value = null
        };

        [SettingSource("Maximum greats", "Maximum number of \"great\" hits before fail.", SettingControlType = typeof(SettingsNumberBox))]
        public Bindable<int?> MaxGreat { get; } = new Bindable<int?>
        {
            Default = null,
            Value = null
        };

        protected override IDictionary<HitResult, Bindable<int?>> HitResultMaximumCounts => hitResultsMaximumCount;
        private Dictionary<HitResult, Bindable<int?>> hitResultsMaximumCount = new Dictionary<HitResult, Bindable<int?>>();

        public ManiaModJudgementChallenge()
        {
            hitResultsMaximumCount[HitResult.Miss] = MaxMiss;
            hitResultsMaximumCount[HitResult.Meh] = MaxMeh;
            hitResultsMaximumCount[HitResult.Ok] = MaxOk;
            hitResultsMaximumCount[HitResult.Good] = MaxGood;
            hitResultsMaximumCount[HitResult.Great] = MaxGreat;
        }
    }
}
