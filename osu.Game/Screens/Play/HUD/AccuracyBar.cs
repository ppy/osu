// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Judgements;
using osuTK.Graphics;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public class AccuracyBar : Container
    {
        private const int bar_width = 5;
        private const int bar_height = 250;
        private const int spacing = 3;

        private readonly bool mirrored;

        public AccuracyBar(bool mirrored = false)
        {
            this.mirrored = mirrored;

            Size = new Vector2(bar_width, bar_height);
            Child = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.White,
            };
        }

        public void OnNewJudgement(JudgementResult judgement)
        {
            Container judgementLine;

            Add(judgementLine = CreateJudgementLine(judgement.TimeOffset));

            judgementLine.FadeOut(5000, Easing.OutQuint);
            judgementLine.Expire();
        }

        protected virtual Container CreateJudgementLine(double offset) => new CircularContainer
        {
            Anchor = mirrored ? Anchor.CentreRight : Anchor.CentreLeft,
            Origin = mirrored ? Anchor.CentreLeft : Anchor.CentreRight,
            Masking = true,
            Size = new Vector2(10, 2),
            RelativePositionAxes = Axes.Y,
            Y = (float)offset / bar_height,
            X = mirrored ? spacing : -spacing,
            Child = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.White,
            }
        };
    }
}
