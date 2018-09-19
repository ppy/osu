// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using OpenTK;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaEditRulesetContainer : EditRulesetContainer<ManiaHitObject>
    {
        public ManiaEditRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, new RulesetContainer(ruleset, beatmap))
        {
        }

        private class RulesetContainer : UI.ManiaRulesetContainer
        {
            public RulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap)
                : base(ruleset, beatmap)
            {
            }

            protected override Playfield CreatePlayfield() => new ManiaEditPlayfield(Beatmap.Stages)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };

            protected override Vector2 PlayfieldArea => Vector2.One;
        }
    }
}
