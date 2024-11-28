// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework.Internal;
using OpenTabletDriver.Plugin;
using osu.Framework.Bindables;
using osu.Game.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using Logger = osu.Framework.Logging.Logger;
using LogLevel = osu.Framework.Logging.LogLevel;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModHidden : ModWithVisibilityAdjustment, IApplicableToScoreProcessor
    {
        public override string Name => "Hidden";
        public override string Acronym => "HD";
        public override IconUsage? Icon => OsuIcon.ModHidden;
        public override ModType Type => ModType.DifficultyIncrease;
        public override bool Ranked => UsesDefaultConfiguration;

        private bool lastShown;
        private uint _combo;
        private bool Show => _combo < EnableAtCombo.Value;

        [SettingSource("Enable at combo", "The combo at which the hidden effect will start to take effect.")]
        public BindableNumber<int> EnableAtCombo { get; } = new BindableNumber<int>(10)
        {
            MinValue = 0,
            MaxValue = 100,
            Precision = 1,
        };

        public virtual ScoreRank AdjustRank(ScoreRank rank, double accuracy)
        {
            switch (rank)
            {
                case ScoreRank.X:
                    return ScoreRank.XH;

                case ScoreRank.S:
                    return ScoreRank.SH;

                default:
                    return rank;
            }
        }

        public virtual void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            _combo = (uint)EnableAtCombo.Value;
            lastShown = false;

            scoreProcessor.NewJudgement += result => ScoreProcessorOnNewJudgement(result);
            scoreProcessor.JudgementReverted += result => ScoreProcessorOnNewJudgement(result, true);

            void ScoreProcessorOnNewJudgement(JudgementResult obj, bool revert = false)
            {
                if (revert) return;
                uint abs = GetHiddenComboInfluence(obj);
                _combo = !obj.IsHit && abs > 0 ? 0 : _combo + abs;
                Logger.Log($"Combo: {_combo}", level: LogLevel.Verbose);
            }
        }

        protected virtual bool OverrideShowHitObjects() => Show;

        protected virtual uint GetHiddenComboInfluence(JudgementResult judgementResult) => 0;
    }
}
