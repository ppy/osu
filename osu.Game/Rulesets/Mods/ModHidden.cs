// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModHidden : ModWithVisibilityAdjustment, IApplicableToScoreProcessor
    {
        public override string Name => "Hidden";
        public override string Acronym => "HD";
        public override IconUsage? Icon => OsuIcon.ModHidden;
        public override ModType Type => ModType.DifficultyIncrease;
        public override bool Ranked => UsesDefaultConfiguration;

        private uint combo;
        private float alpha;
        private readonly Dictionary<HitObject, float> opacityTable = new Dictionary<HitObject, float>(ReferenceEqualityComparer.Instance);

        protected virtual int EnableAtComboValue => 0;

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
            combo = (uint)EnableAtComboValue;
            opacityTable.Clear();

            if (EnableAtComboValue == 0) return;

            scoreProcessor.NewJudgement += result => scoreProcessorOnNewJudgement(result);
            scoreProcessor.JudgementReverted += result => scoreProcessorOnNewJudgement(result, true);

            void scoreProcessorOnNewJudgement(JudgementResult judgement, bool revert = false)
            {
                if (revert) return; // TODO: handle revert for replays

                uint oldCombo = combo;
                combo = ComputeNewComboValue(combo, judgement);
                if (oldCombo == combo)
                    return;

                uint comboValue = GetHiddenComboInfluence(judgement);
                if (comboValue == 0) return;

                combo = !judgement.IsHit ? 0 : combo + comboValue;
                float oldAlpha = alpha;
                alpha = Math.Clamp(Interpolation.ValueAt(combo, 1f, 0f, 0, EnableAtComboValue, Easing.InQuad), 0, 1);

                if (oldAlpha != alpha)
                {
                    foreach (DrawableHitObject? drawableHitObject in PlayfieldAccessor.HitObjectContainer.AliveObjects)
                    {
                        drawableHitObject.RefreshStateTransforms();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the alpha value for a hit object based on the current combo. And stores it internally.
        /// Hitobjects that have already started will never have this value decreased.
        /// </summary>
        /// <param name="drawableHitObject">The drawable hitobject which has an assigned HitObject</param>
        /// <param name="hasStarted">
        /// If supplied, uses this instead of computing it from drawableHitObject.
        /// The default can handle implementations of IHasTimePreempt.
        /// </param>
        /// <returns></returns>
        protected float GetAndUpdateDrawableHitObjectComboAlpha(DrawableHitObject drawableHitObject, bool? hasStarted = null)
        {
            if (EnableAtComboValue == 0) return 0;

            HitObject? ho = drawableHitObject.HitObject;
            hasStarted ??= ho.StartTime - ((ho as IHasTimePreempt)?.TimePreempt ?? 0) < drawableHitObject.Time.Current;

            if (opacityTable.TryGetValue(drawableHitObject.HitObject, out float oldAlpha))
            {
                if (alpha > oldAlpha || !hasStarted.Value)
                {
                    oldAlpha = alpha;
                    opacityTable[ho] = oldAlpha;
                }
            }
            else opacityTable.Add(ho, oldAlpha = alpha);

            return oldAlpha;
        }

        /// <summary>
        /// Specifies how much a hit will add to the internal combo of the mod. Return zero to not break the combo on miss.
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
        /// probably because the type parameter is not specific enough
        /// </remarks>
        public virtual Playfield PlayfieldAccessor => null!;
    }
}
