// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Cursor;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public class OsuEditRulesetContainer : EditRulesetContainer<OsuHitObject>
    {
        public OsuEditRulesetContainer(Ruleset ruleset, WorkingBeatmap workingBeatmap)
            : base(ruleset, workingBeatmap)
        {
        }

        protected override RulesetContainer<OsuHitObject> CreateRulesetContainer(Ruleset ruleset, WorkingBeatmap workingBeatmap)
            => new RulesetContainer(ruleset, workingBeatmap);

        private new class RulesetContainer : OsuRulesetContainer
        {
            public RulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap)
                : base(ruleset, beatmap)
            {
            }

            protected override CursorContainer CreateCursor() => null;
        }
        protected override Playfield CreatePlayfield() => new OsuPlayfield { Size = Vector2.One　};
    }
}
