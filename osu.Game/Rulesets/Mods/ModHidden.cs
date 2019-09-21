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
        public override IconUsage Icon => OsuIcon.ModHidden;
        public override ModType Type => ModType.DifficultyIncrease;
        public override bool Ranked => true;

        protected Bindable<bool> IncreaseFirstObjectVisibility = new Bindable<bool>();

        public void ReadFromConfig(OsuConfigManager config)
        {
            IncreaseFirstObjectVisibility = config.GetBindable<bool>(OsuSetting.IncreaseFirstObjectVisibility);
        }

        public virtual void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var d in drawables.Skip(IncreaseFirstObjectVisibility.Value ? 1 : 0))
                d.ApplyCustomUpdateState += ApplyHiddenState;
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

        protected virtual void ApplyHiddenState(DrawableHitObject hitObject, ArmedState state)
        {
        }
    }
}
