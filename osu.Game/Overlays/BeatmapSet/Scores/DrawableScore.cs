// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays.Profile.Sections.Ranks;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class DrawableScore : Container
    {
        private const int fade_duration = 100;
        private const float side_margin = 20;

        private readonly Box background;

        public DrawableScore(int index, ScoreInfo score)
        {
            ScoreModsContainer modsContainer;

            RelativeSizeAxes = Axes.X;
            Height = 30;
            CornerRadius = 3;
            Masking = true;
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = $"#{index + 1}",
                    Font = OsuFont.GetFont(weight: FontWeight.Regular, italics: true),
                    Margin = new MarginPadding { Left = side_margin }
                },
                new DrawableFlag(score.User.Country)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Size = new Vector2(30, 20),
                    Margin = new MarginPadding { Left = 60 }
                },
                new ClickableUsername
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    User = score.User,
                    Margin = new MarginPadding { Left = 100 }
                },
                modsContainer = new ScoreModsContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.06f,
                    RelativePositionAxes = Axes.X,
                    X = 0.42f
                },
                new DrawableRank(score.Rank)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Size = new Vector2(30, 20),
                    FillMode = FillMode.Fit,
                    RelativePositionAxes = Axes.X,
                    X = 0.55f
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreRight,
                    Text = $@"{score.TotalScore:N0}",
                    Font = OsuFont.Numeric.With(fixedWidth: true),
                    RelativePositionAxes = Axes.X,
                    X = 0.75f,
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreRight,
                    Text = $@"{score.Accuracy:P2}",
                    Font = OsuFont.GetFont(weight: FontWeight.Regular, italics: true),
                    RelativePositionAxes = Axes.X,
                    X = 0.85f
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Text = $"{score.Statistics[HitResult.Great]}/{score.Statistics[HitResult.Good]}/{score.Statistics[HitResult.Meh]}",
                    Font = OsuFont.GetFont(weight: FontWeight.Regular, italics: true),
                    Margin = new MarginPadding { Right = side_margin }
                },
            };

            foreach (Mod mod in score.Mods)
                modsContainer.Add(new ModIcon(mod)
                {
                    AutoSizeAxes = Axes.Both,
                    Scale = new Vector2(0.35f),
                });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.Gray4;
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeIn(fade_duration, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeOut(fade_duration, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected override bool OnClick(ClickEvent e) => true;
    }
}
