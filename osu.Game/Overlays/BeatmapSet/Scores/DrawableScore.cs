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
        private const float text_size = 14;

        private readonly Box hoveredBackground;
        private readonly Box background;

        private readonly SpriteText rank;
        private readonly SpriteText scoreText;
        private readonly SpriteText accuracy;
        private readonly SpriteText maxCombo;
        private readonly SpriteText hitGreat;
        private readonly SpriteText hitGood;
        private readonly SpriteText hitMeh;
        private readonly SpriteText hitMiss;
        private readonly SpriteText pp;

        private readonly ClickableScoreUsername username;

        private readonly APIScoreInfo score;

        public DrawableScore(int index, APIScoreInfo score, int maxModsAmount)
        {
            FillFlowContainer modsContainer;

            this.score = score;

            RelativeSizeAxes = Axes.X;
            Height = 25;
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
                rank = new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreRight,
                    Text = $"#{index + 1}",
                    TextSize = text_size,
                    X = ScoreTextLine.RANK_POSITION,
                    Font = @"Exo2.0-Bold",
                },
                new DrawableRank(score.Rank)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Size = new Vector2(30, 20),
                    FillMode = FillMode.Fit,
                    X = 45
                },
                scoreText = new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = $@"{score.TotalScore:N0}",
                    X = ScoreTextLine.SCORE_POSITION,
                    TextSize = text_size,
                },
                accuracy = new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = $@"{score.Accuracy:P2}",
                    X = ScoreTextLine.ACCURACY_POSITION,
                    TextSize = text_size,
                },
                new DrawableFlag(score.User.Country)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Size = new Vector2(20, 13),
                    X = 230,
                },
                username = new ClickableScoreUsername
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    User = score.User,
                    X = ScoreTextLine.PLAYER_POSITION,
                },
                maxCombo = new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = $@"{score.MaxCombo:N0}x",
                    RelativePositionAxes = Axes.X,
                    X = ScoreTextLine.MAX_COMBO_POSITION,
                    TextSize = text_size,
                },
                hitGreat = new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = $"{score.Statistics[HitResult.Great]}",
                    RelativePositionAxes = Axes.X,
                    X = ScoreTextLine.HIT_GREAT_POSITION,
                    TextSize = text_size,
                },
                hitGood = new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = $"{score.Statistics[HitResult.Good]}",
                    RelativePositionAxes = Axes.X,
                    X = ScoreTextLine.HIT_GOOD_POSITION,
                    TextSize = text_size,
                },
                hitMeh = new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = $"{score.Statistics[HitResult.Meh]}",
                    RelativePositionAxes = Axes.X,
                    X = ScoreTextLine.HIT_MEH_POSITION,
                    TextSize = text_size,
                },
                hitMiss = new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = $"{score.Statistics[HitResult.Miss]}",
                    RelativePositionAxes = Axes.X,
                    X = ScoreTextLine.HIT_MISS_POSITION,
                    TextSize = text_size,
                },
                pp = new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = $@"{score.PP:N0}",
                    RelativePositionAxes = Axes.X,
                    X = ScoreTextLine.PP_POSITION,
                    TextSize = text_size,
                },
                modsContainer = new FillFlowContainer
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreLeft,
                    Direction = FillDirection.Horizontal,
                    AutoSizeAxes = Axes.Both,
                    X = -30 * maxModsAmount,
                },
            };

            if (index == 0)
                scoreText.Font = @"Exo2.0-Bold";

            accuracy.Colour = (score.Accuracy == 1) ? Color4.GreenYellow : Color4.White;

            hitGreat.Colour = (score.Statistics[HitResult.Great] == 0) ? Color4.Gray : Color4.White;
            hitGood.Colour = (score.Statistics[HitResult.Good] == 0) ? Color4.Gray : Color4.White;
            hitMeh.Colour = (score.Statistics[HitResult.Meh] == 0) ? Color4.Gray : Color4.White;
            hitMiss.Colour = (score.Statistics[HitResult.Miss] == 0) ? Color4.Gray : Color4.White;

            if (index % 2 == 0)
                background.Alpha = 0;

            foreach (Mod mod in score.Mods)
                modsContainer.Add(new ModIcon(mod)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Both,
                    Scale = new Vector2(0.3f),
                });
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
    }
}
