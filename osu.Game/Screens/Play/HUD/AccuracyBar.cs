// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Judgements;
using osuTK.Graphics;
using osuTK;
using osu.Framework.Graphics.Sprites;
using System.Collections.Generic;

namespace osu.Game.Screens.Play.HUD
{
    public class AccuracyBar : Container
    {
        private const int bar_width = 5;
        private const int bar_height = 250;
        private const int spacing = 3;

        private readonly bool mirrored;
        private readonly SpriteIcon arrow;
        private readonly List<double> judgementOffsets = new List<double>();

        public AccuracyBar(bool mirrored = false)
        {
            this.mirrored = mirrored;

            Size = new Vector2(bar_width, bar_height);

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                },
                arrow = new SpriteIcon
                {
                    Anchor = mirrored ? Anchor.CentreLeft : Anchor.CentreRight,
                    Origin = mirrored ? Anchor.CentreRight : Anchor.CentreLeft,
                    X = mirrored ? -spacing : spacing,
                    RelativePositionAxes = Axes.Y,
                    Icon = mirrored ? FontAwesome.Solid.ChevronRight : FontAwesome.Solid.ChevronLeft,
                    Size = new Vector2(10),
                }
            };
        }

        public void OnNewJudgement(JudgementResult judgement)
        {
            Container judgementLine;

            Add(judgementLine = CreateJudgementLine(judgement));

            judgementLine.FadeOut(5000, Easing.OutQuint);
            judgementLine.Expire();

            arrow.MoveToY(calculateArrowPosition(judgement) / bar_height, 500, Easing.OutQuint);
        }

        protected virtual Container CreateJudgementLine(JudgementResult judgement) => new CircularContainer
        {
            Anchor = mirrored ? Anchor.CentreRight : Anchor.CentreLeft,
            Origin = mirrored ? Anchor.CentreLeft : Anchor.CentreRight,
            Masking = true,
            Size = new Vector2(10, 2),
            RelativePositionAxes = Axes.Y,
            Y = (float)judgement.TimeOffset / bar_height,
            X = mirrored ? spacing : -spacing,
            Child = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.White,
            }
        };

        private float calculateArrowPosition(JudgementResult judgement)
        {
            if (judgementOffsets.Count > 5)
                judgementOffsets.RemoveAt(0);

            judgementOffsets.Add(judgement.TimeOffset);

            double offsets = 0;

            foreach (var offset in judgementOffsets)
                offsets += offset;

            return (float)offsets / judgementOffsets.Count;
        }
    }
}
