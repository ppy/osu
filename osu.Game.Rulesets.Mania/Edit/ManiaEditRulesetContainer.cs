﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osuTK;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.Edit
{
    public class ManiaEditRulesetContainer : ManiaRulesetContainer
    {
        public new IScrollingInfo ScrollingInfo => base.ScrollingInfo;

        public ManiaEditRulesetContainer(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override Playfield CreatePlayfield() => new ManiaEditPlayfield(Beatmap.Stages)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Size = Vector2.One
        };
    }
}
