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
        public OsuEditRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap)
            : this(ruleset, new RulesetContainer(ruleset, beatmap))
        {
        }

        private OsuEditRulesetContainer(Ruleset ruleset, RulesetContainer<OsuHitObject> rulesetContainer)
            : base(ruleset, rulesetContainer)
        {
        }

        private class RulesetContainer : OsuRulesetContainer
        {
            public RulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap)
                : base(ruleset, beatmap)
            {
            }

            protected override Vector2 PlayfieldArea => Vector2.One;

            protected override CursorContainer CreateCursor() => null;
        }
    }
}
