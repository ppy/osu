// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.UI;
using OpenTK;
using osu.Game.Rulesets.Catch.Judgements;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatchPlayfield : Playfield<CatchBaseHit, CatchJudgement>
    {
        public CatchPlayfield()
        {
            Size = new Vector2(1, 0.9f);
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;

            Add(new Box { RelativeSizeAxes = Axes.Both, Alpha = 0.5f });
        }
    }
}