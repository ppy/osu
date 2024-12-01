// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
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

        private uint _combo;
        private float _alpha;
        private Playfield _playfield = null!;
        private Dictionary<HitObject, float> _opacityTable = new(ReferenceEqualityComparer.Instance);

        [SettingSource("Enable at combo", "The combo at which the hidden effect will take full effect. 0 for always.")]
        public BindableNumber<int> EnableAtCombo { get; } = new BindableNumber<int>(10) // TODO: set default to 0
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
            Logger.Log("Hidden mod applied", level: LogLevel.Verbose);
            _opacityTable.Clear();

            if (EnableAtCombo.Value == 0) return;

            scoreProcessor.NewJudgement += result => ScoreProcessorOnNewJudgement(result);
            scoreProcessor.JudgementReverted += result => ScoreProcessorOnNewJudgement(result, true);

            void ScoreProcessorOnNewJudgement(JudgementResult judgement, bool revert = false)
            {
                if (revert) return; // TODO: handle revert for replays
                uint oldCombo = _combo;
                _combo = ComputeNewComboValue(_combo, judgement);
                if (oldCombo == _combo)
                    return;
                else
                {

                }
                uint comboValue = GetHiddenComboInfluence(judgement);
                if (comboValue == 0) return;
                _combo = !judgement.IsHit ? 0 : _combo + comboValue;
                float oldAlpha = _alpha;
                _alpha = Math.Clamp(Interpolation.ValueAt(_combo, 1f, 0f, 0, EnableAtCombo.Value, Easing.InQuad), 0, 1);

                if (oldAlpha != _alpha)
                {
                    foreach (DrawableHitObject? drawableHitObject in PlayfieldAccessor.HitObjectContainer.AliveObjects)
                    {
                        drawableHitObject.RefreshStateTransforms();
                    }
                }

                Logger.Log($"Combo: {_combo}     {_alpha}", level: LogLevel.Verbose);
            }
        }

        protected float GetAndUpdateDrawableHitObjectComboAlpha(DrawableHitObject dho, bool? hasStarted = null)
        {
            if (EnableAtCombo.Value == 0) return 0;
            HitObject? ho = dho.HitObject;

            hasStarted ??= ho.StartTime - ((ho as IHasTimePreempt)?.TimePreempt ?? 0) < dho.Time.Current;

            if (_opacityTable.TryGetValue(dho.HitObject, out float alpha))
            {
                if (_alpha > alpha || !hasStarted.Value)
                {
                    alpha = _alpha;
                    _opacityTable[ho] = alpha;
                }
            }
            else _opacityTable.Add(ho, alpha = _alpha);

            return alpha;
        }

        /// <summary>
        /// Speciefies how much a hit will add to the internal combo of the mod. Return zero to not break the combo on miss.
        /// </summary>
        protected virtual uint GetHiddenComboInfluence(JudgementResult judgementResult) => 0;

        /// <summary>
        /// Computes the new combo value based on the current combo and the judgement.
        /// The default Implementation is based on the <see cref="GetHiddenComboInfluence(JudgementResult)"/> method.
        /// Override this method to provide a custom combo calculation.
        /// </summary>
        protected virtual uint ComputeNewComboValue(uint currentCombo, JudgementResult judgement)
        {
            uint comboValue = GetHiddenComboInfluence(judgement);
            if (comboValue == 0) return currentCombo;
            return !judgement.IsHit ? 0 : currentCombo + comboValue;
        }

        /// <summary>
        /// Playfield accessor for the hidden mod.
        /// </summary>
        /// <remarks>
        /// This is required because implementing IApplicableToDrawableRuleset&lt;HitObject&gt; here does not work,
        /// probabbly because the type parameter is not specific enough
        /// </remarks>
        public virtual Playfield PlayfieldAccessor => null!;
    }
}
