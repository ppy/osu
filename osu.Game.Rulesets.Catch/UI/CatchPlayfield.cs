// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
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
            Size = new Vector2(1);

            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;

            Children = new Drawable[]
            {
                new Catcher
                {
                    RelativePositionAxes = Axes.Both,
                    RelativeSizeAxes = Axes.Both,
                    Scale = new Vector2(0.2f),
                    FillMode = FillMode.Fit,
                    Origin = Anchor.TopCentre,
                    Position = new Vector2(0.5f, 1),
                }
            };
        }
    }
}