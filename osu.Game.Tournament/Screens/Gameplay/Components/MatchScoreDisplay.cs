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
using osuTK;

namespace osu.Game.Tournament.Screens.Gameplay.Components
{
    public class MatchScoreDisplay : CompositeDrawable
    {
        private const float bar_height = 18;

        private readonly BindableInt score1 = new BindableInt();
        private readonly BindableInt score2 = new BindableInt();

        private readonly MatchScoreCounter score1Text;
        private readonly MatchScoreCounter score2Text;

        private readonly Drawable score1Bar;
        private readonly Drawable score2Bar;

        public MatchScoreDisplay()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new[]
            {
                new Box
                {
                    Name = "top bar red (static)",
                    RelativeSizeAxes = Axes.X,
                    Height = bar_height / 4,
                    Width = 0.5f,
                    Colour = TournamentGame.COLOUR_RED,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopRight
                },
                new Box
                {
                    Name = "top bar blue (static)",
                    RelativeSizeAxes = Axes.X,
                    Height = bar_height / 4,
                    Width = 0.5f,
                    Colour = TournamentGame.COLOUR_BLUE,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopLeft
                },
                score1Bar = new Box
                {
                    Name = "top bar red",
                    RelativeSizeAxes = Axes.X,
                    Height = bar_height,
                    Width = 0,
                    Colour = TournamentGame.COLOUR_RED,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopRight
                },
                score1Text = new MatchScoreCounter
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                },
                score2Bar = new Box
                {
                    Name = "top bar blue",
                    RelativeSizeAxes = Axes.X,
                    Height = bar_height,
                    Width = 0,
                    Colour = TournamentGame.COLOUR_BLUE,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopLeft
                },
                score2Text = new MatchScoreCounter
                {
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
            winningBar.ResizeWidthTo(Math.Min(0.4f, MathF.Pow(diff / 1500000f, 0.5f) / 2), 400, Easing.OutQuint);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();
            score1Text.X = -Math.Max(5 + score1Text.DrawWidth / 2, score1Bar.DrawWidth);
            score2Text.X = Math.Max(5 + score2Text.DrawWidth / 2, score2Bar.DrawWidth);
        }

        private class MatchScoreCounter : ScoreCounter
        {
            public MatchScoreCounter()
            {
                Margin = new MarginPadding { Top = bar_height, Horizontal = 10 };

                Winning = false;

                DisplayedCountSpriteText.Spacing = new Vector2(-6);
            }

            public bool Winning
            {
                set => DisplayedCountSpriteText.Font = value
                    ? OsuFont.Torus.With(weight: FontWeight.Bold, size: 50, fixedWidth: true)
                    : OsuFont.Torus.With(weight: FontWeight.Regular, size: 40, fixedWidth: true);
            }
        }
    }
}
