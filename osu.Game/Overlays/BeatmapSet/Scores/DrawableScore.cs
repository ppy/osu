// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class DrawableScore : Container
    {
        private const int fade_duration = 100;
        private const int text_size = 14;

        private readonly Box hoveredBackground;
        private readonly Box background;

        public DrawableScore(int index, APIScoreInfo score, int maxModsAmount)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            CornerRadius = 3;
            Masking = true;
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                hoveredBackground = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                },
                new DrawableScoreData(index, score, maxModsAmount),
            };

            if (index % 2 != 0)
                background.Alpha = 0;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            hoveredBackground.Colour = colours.Gray4;
            background.Colour = colours.Gray3;
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoveredBackground.FadeIn(fade_duration, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoveredBackground.FadeOut(fade_duration, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected override bool OnClick(ClickEvent e) => true;

        private class ClickableScoreUsername : ClickableUserContainer
        {
            private readonly SpriteText text;
            private readonly SpriteText textBold;

            public ClickableScoreUsername()
            {
                Add(text = new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    TextSize = text_size,
                });

                Add(textBold = new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    TextSize = text_size,
                    Font = @"Exo2.0-Bold",
                    Alpha = 0,
                });
            }

            protected override void OnUserChange(User user)
            {
                text.Text = textBold.Text = user.Username;
            }

            protected override bool OnHover(HoverEvent e)
            {
                textBold.FadeIn(fade_duration, Easing.OutQuint);
                text.FadeOut(fade_duration, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                textBold.FadeOut(fade_duration, Easing.OutQuint);
                text.FadeIn(fade_duration, Easing.OutQuint);
                base.OnHoverLost(e);
            }
        }

        private class DrawableScoreData : ScoreTableLine
        {
            public DrawableScoreData(int index, APIScoreInfo score, int maxModsAmount) : base(maxModsAmount)
            {
                SpriteText scoreText;
                SpriteText accuracy;
                SpriteText hitGreat;
                SpriteText hitGood;
                SpriteText hitMeh;
                SpriteText hitMiss;

                FillFlowContainer modsContainer;

                RankContainer.Add(new SpriteText
                {
                    Text = $"#{index + 1}",
                    Font = @"Exo2.0-Bold",
                    TextSize = text_size,
                });
                DrawableRankContainer.Add(new DrawableRank(score.Rank)
                {
                    Size = new Vector2(30, 20),
                    FillMode = FillMode.Fit,
                });
                ScoreContainer.Add(scoreText = new SpriteText
                {
                    Text = $@"{score.TotalScore:N0}",
                    TextSize = text_size,
                });
                AccuracyContainer.Add(accuracy = new SpriteText
                {
                    Text = $@"{score.Accuracy:P2}",
                    TextSize = text_size,
                });
                FlagContainer.Add(new DrawableFlag(score.User.Country)
                {
                    Size = new Vector2(20, 13),
                });
                PlayerContainer.Add(new ClickableScoreUsername
                {
                    User = score.User,
                });
                MaxComboContainer.Add(new SpriteText
                {
                    Text = $@"{score.MaxCombo:N0}x",
                    TextSize = text_size,
                });
                HitGreatContainer.Add(hitGreat = new SpriteText
                {
                    Text = $"{score.Statistics[HitResult.Great]}",
                    TextSize = text_size,
                });
                HitGoodContainer.Add(hitGood = new SpriteText
                {
                    Text = $"{score.Statistics[HitResult.Good]}",
                    TextSize = text_size,
                });
                HitMehContainer.Add(hitMeh = new SpriteText
                {
                    Text = $"{score.Statistics[HitResult.Meh]}",
                    TextSize = text_size,
                });
                HitMissContainer.Add(hitMiss = new SpriteText
                {
                    Text = $"{score.Statistics[HitResult.Miss]}",
                    TextSize = text_size,
                });
                PPContainer.Add(new SpriteText
                {
                    Text = $@"{score.PP:N0}",
                    TextSize = text_size,
                });
                ModsContainer.Add(modsContainer = new FillFlowContainer
                {
                    Direction = FillDirection.Horizontal,
                    AutoSizeAxes = Axes.Both,
                });

                if (index == 0)
                    scoreText.Font = @"Exo2.0-Bold";

                accuracy.Colour = (score.Accuracy == 1) ? Color4.GreenYellow : Color4.White;
                hitGreat.Colour = (score.Statistics[HitResult.Great] == 0) ? Color4.Gray : Color4.White;
                hitGood.Colour = (score.Statistics[HitResult.Good] == 0) ? Color4.Gray : Color4.White;
                hitMeh.Colour = (score.Statistics[HitResult.Meh] == 0) ? Color4.Gray : Color4.White;
                hitMiss.Colour = (score.Statistics[HitResult.Miss] == 0) ? Color4.Gray : Color4.White;

                foreach (Mod mod in score.Mods)
                    modsContainer.Add(new ModIcon(mod)
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                        Scale = new Vector2(0.3f),
                    });
            }
        }
    }
}
