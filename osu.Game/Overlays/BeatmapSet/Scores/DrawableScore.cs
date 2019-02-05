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

        public DrawableScore(int index, APIScoreInfo score)
        {
            FillFlowContainer modsContainer;
            Box background;

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
                    Colour = Color4.Black,
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
                    Colour = Color4.Black,
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
                    Colour = Color4.Black,
                },
                maxCombo = new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = $@"{score.MaxCombo:N0}x",
                    RelativePositionAxes = Axes.X,
                    X = ScoreTextLine.MAX_COMBO_POSITION,
                    TextSize = text_size,
                    Colour = Color4.Black,
                },
                hitGreat = new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = $"{score.Statistics[HitResult.Great]}",
                    RelativePositionAxes = Axes.X,
                    X = ScoreTextLine.HIT_GREAT_POSITION,
                    TextSize = text_size,
                    Colour = Color4.Black,
                },
                hitGood = new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = $"{score.Statistics[HitResult.Good]}",
                    RelativePositionAxes = Axes.X,
                    X = ScoreTextLine.HIT_GOOD_POSITION,
                    TextSize = text_size,
                    Colour = Color4.Black,
                },
                hitMeh = new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = $"{score.Statistics[HitResult.Meh]}",
                    RelativePositionAxes = Axes.X,
                    X = ScoreTextLine.HIT_MEH_POSITION,
                    TextSize = text_size,
                    Colour = Color4.Black,
                },
                hitMiss = new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = $"{score.Statistics[HitResult.Miss]}",
                    RelativePositionAxes = Axes.X,
                    X = ScoreTextLine.HIT_MISS_POSITION,
                    TextSize = text_size,
                    Colour = Color4.Black,
                },
                pp = new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = $@"{score.PP:N0}",
                    RelativePositionAxes = Axes.X,
                    X = ScoreTextLine.PP_POSITION,
                    TextSize = text_size,
                    Colour = Color4.Black,
                },
                modsContainer = new FillFlowContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.Centre,
                    Direction = FillDirection.Horizontal,
                    AutoSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.X,
                    X = ScoreTextLine.MODS_POSITION,
                },
            };

            if (index == 0)
                scoreText.Font = @"Exo2.0-Bold";

            accuracy.Colour = (score.Accuracy == 1) ? Color4.Green : Color4.Black;

            hitGreat.Colour = (score.Statistics[HitResult.Great] == 0) ? Color4.Gray : Color4.Black;
            hitGood.Colour = (score.Statistics[HitResult.Good] == 0) ? Color4.Gray : Color4.Black;
            hitMeh.Colour = (score.Statistics[HitResult.Meh] == 0) ? Color4.Gray : Color4.Black;
            hitMiss.Colour = (score.Statistics[HitResult.Miss] == 0) ? Color4.Gray : Color4.Black;

            background.Colour = (index % 2 == 0) ? Color4.WhiteSmoke : Color4.White;

            foreach (Mod mod in score.Mods)
                modsContainer.Add(new ModIcon(mod)
                {
                    AutoSizeAxes = Axes.Both,
                    Scale = new Vector2(0.3f),
                });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            hoveredBackground.Colour = colours.Gray4;
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoveredBackground.FadeIn(fade_duration, Easing.OutQuint);
            rank.FadeColour(Color4.White, fade_duration, Easing.OutQuint);
            scoreText.FadeColour(Color4.White, fade_duration, Easing.OutQuint);
            accuracy.FadeColour(Color4.White, fade_duration, Easing.OutQuint);
            username.FadeColour(Color4.White, fade_duration, Easing.OutQuint);
            maxCombo.FadeColour(Color4.White, fade_duration, Easing.OutQuint);
            pp.FadeColour(Color4.White, fade_duration, Easing.OutQuint);

            if (score.Statistics[HitResult.Great] != 0)
                hitGreat.FadeColour(Color4.White, fade_duration, Easing.OutQuint);

            if (score.Statistics[HitResult.Good] != 0)
                hitGood.FadeColour(Color4.White, fade_duration, Easing.OutQuint);

            if (score.Statistics[HitResult.Meh] != 0)
                hitMeh.FadeColour(Color4.White, fade_duration, Easing.OutQuint);

            if (score.Statistics[HitResult.Miss] != 0)
                hitMiss.FadeColour(Color4.White, fade_duration, Easing.OutQuint);

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoveredBackground.FadeOut(fade_duration, Easing.OutQuint);
            rank.FadeColour(Color4.Black, fade_duration, Easing.OutQuint);
            scoreText.FadeColour(Color4.Black, fade_duration, Easing.OutQuint);
            username.FadeColour(Color4.Black, fade_duration, Easing.OutQuint);
            accuracy.FadeColour((score.Accuracy == 1) ? Color4.Green : Color4.Black, fade_duration, Easing.OutQuint);
            maxCombo.FadeColour(Color4.Black, fade_duration, Easing.OutQuint);
            pp.FadeColour(Color4.Black, fade_duration, Easing.OutQuint);

            if (score.Statistics[HitResult.Great] != 0)
                hitGreat.FadeColour(Color4.Black, fade_duration, Easing.OutQuint);

            if (score.Statistics[HitResult.Good] != 0)
                hitGood.FadeColour(Color4.Black, fade_duration, Easing.OutQuint);

            if (score.Statistics[HitResult.Meh] != 0)
                hitMeh.FadeColour(Color4.Black, fade_duration, Easing.OutQuint);

            if (score.Statistics[HitResult.Miss] != 0)
                hitMiss.FadeColour(Color4.Black, fade_duration, Easing.OutQuint);

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
