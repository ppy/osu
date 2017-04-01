// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Modes.Catch.Objects;
using osu.Game.Modes.UI;
using OpenTK;
using osu.Game.Modes.Catch.Judgements;

namespace osu.Game.Modes.Catch.UI
{
    public class CatchPlayfield : Playfield<CatchBaseHit, CatchJudgement>
    {
        public CatchPlayfield()
        {
            RelativeSizeAxes = Axes.Y;
            Size = new Vector2(512, 0.9f);
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;

            Add(new Box { RelativeSizeAxes = Axes.Both, Alpha = 0.5f });
        }
    }
}