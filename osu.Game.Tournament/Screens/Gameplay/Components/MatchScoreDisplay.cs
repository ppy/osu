// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Gameplay.Components
{
    public class MatchScoreDisplay : CompositeDrawable
    {
        private readonly Color4 red = new Color4(186, 0, 18, 255);
        private readonly Color4 blue = new Color4(17, 136, 170, 255);

        private const float bar_height = 20;

        private readonly BindableInt score1 = new BindableInt();
        private readonly BindableInt score2 = new BindableInt();

        private readonly MatchScoreCounter score1Text;
        private readonly MatchScoreCounter score2Text;

        private readonly Circle score1Bar;
        private readonly Circle score2Bar;

        public MatchScoreDisplay()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                score1Bar = new Circle
                {
                    Name = "top bar red",
                    RelativeSizeAxes = Axes.X,
                    Height = bar_height,
                    Width = 0,
                    Colour = red,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopRight
                },
                score1Text = new MatchScoreCounter
                {
                    Colour = red,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                },
                score2Bar = new Circle
                {
                    Name = "top bar blue",
                    RelativeSizeAxes = Axes.X,
                    Height = bar_height,
                    Width = 0,
                    Colour = blue,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopLeft
                },
                score2Text = new MatchScoreCounter
                {
                    Colour = blue,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder, MatchIPCInfo ipc)
        {
            score1.BindValueChanged(_ => updateScores());
            score1.BindTo(ipc.Score1);

            score2.BindValueChanged(_ => updateScores());
            score2.BindTo(ipc.Score2);
        }

        private void updateScores()
        {
            score1Text.Current.Value = score1.Value;
            score2Text.Current.Value = score2.Value;

            var winningText = score1.Value > score2.Value ? score1Text : score2Text;
            var losingText = score1.Value <= score2.Value ? score1Text : score2Text;

            winningText.Winning = true;
            losingText.Winning = false;

            var winningBar = score1.Value > score2.Value ? score1Bar : score2Bar;
            var losingBar = score1.Value <= score2.Value ? score1Bar : score2Bar;

            var diff = Math.Max(score1.Value, score2.Value) - Math.Min(score1.Value, score2.Value);

            losingBar.ResizeWidthTo(0, 400, Easing.OutQuint);
            winningBar.ResizeWidthTo(Math.Min(0.4f, (float)Math.Pow(diff / 1500000f, 0.5) / 2), 400, Easing.OutQuint);
        }

        protected override void Update()
        {
            base.Update();

            score1Text.X = -Math.Max(5 + score1Text.DrawWidth / 2, score1Bar.DrawWidth);
            score2Text.X = Math.Max(5 + score2Text.DrawWidth / 2, score2Bar.DrawWidth);
        }

        private class MatchScoreCounter : ScoreCounter
        {
            public MatchScoreCounter()
            {
                Margin = new MarginPadding { Top = bar_height + 5, Horizontal = 10 };

                Winning = false;
            }

            public bool Winning
            {
                set => DisplayedCountSpriteText.Font = value
                    ? TournamentFont.GetFont(typeface: TournamentTypeface.Aquatico, weight: FontWeight.Regular, size: 60)
                    : TournamentFont.GetFont(typeface: TournamentTypeface.Aquatico, weight: FontWeight.Light, size: 40);
            }
        }
    }
}
