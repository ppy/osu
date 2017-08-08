// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.UI;
using OpenTK;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatchPlayfield : ScrollingPlayfield<CatchBaseHit, CatchJudgement>
    {
        protected override Container<Drawable> Content => content;
        private readonly Container<Drawable> content;

        public CatchPlayfield()
            : base(Axes.Y)
        {
            Size = new Vector2(1);

            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;

            InternalChildren = new Drawable[]
            {
                content = new Container<Drawable>
                {
                    Scale = new Vector2(1, -1),
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.BottomLeft
                },
                new CatcherArea
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.TopLeft,
                    Height = 0.3f
                }
            };
        }
    }
}
