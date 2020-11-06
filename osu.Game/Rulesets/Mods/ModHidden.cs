// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModHidden : ModWithVisibilityAdjustment, IApplicableToScoreProcessor
    {
        public override string Name => "Hidden";
        public override string Acronym => "HD";
        public override IconUsage? Icon => OsuIcon.ModHidden;
        public override ModType Type => ModType.DifficultyIncrease;
        public override bool Ranked => true;

        /// <summary>
        /// Check whether the provided hitobject should be considered the "first" hideable object.
        /// Can be used to skip spinners, for instance.
        /// </summary>
        /// <param name="hitObject">The hitobject to check.</param>
        [Obsolete("Use IsFirstAdjustableObject() instead.")] // Can be removed 20210506
        protected virtual bool IsFirstHideableObject(DrawableHitObject hitObject) => true;

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            // Default value of ScoreProcessor's Rank in Hidden Mod should be SS+
            scoreProcessor.Rank.Value = ScoreRank.XH;
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy)
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

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
#pragma warning disable 618
            ApplyFirstObjectIncreaseVisibilityState(hitObject, state);
#pragma warning restore 618
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
#pragma warning disable 618
            ApplyHiddenState(hitObject, state);
#pragma warning restore 618
        }

        /// <summary>
        /// Apply a special visibility state to the first object in a beatmap, if the user chooses to turn on the "increase first object visibility" setting.
        /// </summary>
        /// <param name="hitObject">The hit object to apply the state change to.</param>
        /// <param name="state">The state of the hit object.</param>
        [Obsolete("Use ApplyIncreasedVisibilityState() instead.")] // Can be removed 20210506
        protected virtual void ApplyFirstObjectIncreaseVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
        }

        /// <summary>
        /// Apply a hidden state to the provided object.
        /// </summary>
        /// <param name="hitObject">The hit object to apply the state change to.</param>
        /// <param name="state">The state of the hit object.</param>
        [Obsolete("Use ApplyNormalVisibilityState() instead.")] // Can be removed 20210506
        protected virtual void ApplyHiddenState(DrawableHitObject hitObject, ArmedState state)
        {
        }
    }
}
