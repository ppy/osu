// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.UI;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Rulesets.Mania.Judgements;

namespace osu.Game.Rulesets.Mania.UI
{
    public class ManiaPlayfield : Playfield<ManiaBaseHit, ManiaJudgement>
    {
        public ManiaPlayfield(int columns)
        {
            Size = new Vector2(0.8f, 1f);
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;

            Add(new Box { RelativeSizeAxes = Axes.Both, Alpha = 0.5f });

            for (int i = 0; i < columns; i++)
                Add(new Box
                {
                    RelativeSizeAxes = Axes.Y,
                    Size = new Vector2(2, 1),
                    RelativePositionAxes = Axes.Both,
                    Position = new Vector2((float)i / columns, 0),
                    Alpha = 0.5f,
                    Colour = Color4.Black
                });
        }
    }
}