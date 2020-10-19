// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModHidden : Mod, IReadFromConfig, IApplicableToDrawableHitObjects, IApplicableToScoreProcessor
    {
        public override string Name => "Hidden";
        public override string Acronym => "HD";
        public override IconUsage? Icon => OsuIcon.ModHidden;
        public override ModType Type => ModType.DifficultyIncrease;
        public override bool Ranked => true;

        protected Bindable<bool> IncreaseFirstObjectVisibility = new Bindable<bool>();

        /// <summary>
        /// Check whether the provided hitobject should be considered the "first" hideable object.
        /// Can be used to skip spinners, for instance.
        /// </summary>
        /// <param name="hitObject">The hitobject to check.</param>
        protected virtual bool IsFirstHideableObject(DrawableHitObject hitObject) => true;

        public void ReadFromConfig(OsuConfigManager config)
        {
            IncreaseFirstObjectVisibility = config.GetBindable<bool>(OsuSetting.IncreaseFirstObjectVisibility);
        }

        public virtual void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            if (IncreaseFirstObjectVisibility.Value)
            {
                drawables = drawables.SkipWhile(h => !IsFirstHideableObject(h));

                var firstObject = drawables.FirstOrDefault();
                if (firstObject != null)
                    firstObject.ApplyCustomUpdateState += ApplyFirstObjectIncreaseVisibilityState;

                drawables = drawables.Skip(1);
            }

            foreach (var dho in drawables)
                dho.ApplyCustomUpdateState += ApplyHiddenState;
        }

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

        /// <summary>
        /// Apply a special visibility state to the first object in a beatmap, if the user chooses to turn on the "increase first object visibility" setting.
        /// </summary>
        /// <param name="hitObject">The hit object to apply the state change to.</param>
        /// <param name="state">The state of the hit object.</param>
        protected virtual void ApplyFirstObjectIncreaseVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
        }

        /// <summary>
        /// Apply a hidden state to the provided object.
        /// </summary>
        /// <param name="hitObject">The hit object to apply the state change to.</param>
        /// <param name="state">The state of the hit object.</param>
        protected virtual void ApplyHiddenState(DrawableHitObject hitObject, ArmedState state)
        {
        }
    }
}
