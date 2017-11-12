// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Profile.Sections.Ranks;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Users;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class DrawableScore : Container
    {
        private const int fade_duration = 100;
        private const float height = 30;
        private const float side_margin = 20;
        private const float flag_margin = 60;
        private const float username_margin = 100;

        private readonly Box background;

        public DrawableScore(int index, OnlineScore score)
        {
            ScoreModsContainer modsContainer;

            RelativeSizeAxes = Axes.X;
            Height = height;
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
                    Font = @"Exo2.0-RegularItalic",
                    Margin = new MarginPadding { Left = side_margin }
                },
                new DrawableFlag(score.User.Country?.FlagName)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Width = 30,
                    Height = 20,
                    Margin = new MarginPadding { Left = flag_margin }
                },
                new ClickableUsername
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    User = score.User,
                    Margin = new MarginPadding { Left = username_margin }
                },
                modsContainer = new ScoreModsContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Width = 0.05f,
                    RelativePositionAxes = Axes.X,
                    X = 0.45f
                },
                new DrawableRank(score.Rank)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Width = 30,
                    Height = 20,
                    FillMode = FillMode.Fit,
                    RelativePositionAxes = Axes.X,
                    X = 0.57f
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreRight,
                    Text = $@"{score.TotalScore}",
                    Font = @"Exo2.0-MediumItalic",
                    RelativePositionAxes = Axes.X,
                    X = 0.7f
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreRight,
                    Text = $@"{score.Accuracy:P2}",
                    Font = @"Exo2.0-RegularItalic",
                    RelativePositionAxes = Axes.X,
                    X = 0.8f
                },
                new OsuSpriteText
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Text = $"{score.Statistics["300"]}/{score.Statistics["100"]}/{score.Statistics["50"]}",
                    Font = @"Exo2.0-RegularItalic",
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

        protected override bool OnHover(InputState state)
        {
            background.FadeIn(fade_duration, Easing.OutQuint);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            background.FadeOut(fade_duration, Easing.OutQuint);
            base.OnHoverLost(state);
        }

        protected override bool OnClick(InputState state) => true;
    }
}
