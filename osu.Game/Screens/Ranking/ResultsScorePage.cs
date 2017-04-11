// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Modes.Scoring;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Users;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Ranking
{
    internal class ResultsScorePage : ResultsPage
    {
        private ScoreCounter scoreCounter;

        public ResultsScorePage(Score score) : base(score) { }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            const float user_header_height = 150;

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = user_header_height },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White,
                        },
                    }
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new UserHeader(Score.User)
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = user_header_height,
                        },
                        new DrawableRank(Score.Rank)
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Size = new Vector2(150, 80),
                        },
                        scoreCounter = new SlowScoreCounter(6)
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Colour = colours.PinkDarker,
                            TextSize = 60,
                        },
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Schedule(() => scoreCounter.Increment(Score.TotalScore));
        }

        private class UserHeader : Container
        {
            private readonly User user;
            private readonly Sprite cover;

            public UserHeader(User user)
            {
                this.user = user;
                Children = new Drawable[]
                {
                    cover = new Sprite
                    {
                        FillMode = FillMode.Fill,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                    new OsuSpriteText
                    {
                        Font = @"Exo2.0-RegularItalic",
                        Text = user.Username,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        TextSize = 30,
                        Padding = new MarginPadding { Bottom = 10 },
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                cover.Texture = textures.Get(user.CoverUrl);
            }
        }

        private class SlowScoreCounter : ScoreCounter
        {
            protected override double RollingDuration => 3000;

            protected override EasingTypes RollingEasing => EasingTypes.OutPow10;

            public SlowScoreCounter(uint leading = 0) : base(leading)
            {
                DisplayedCountSpriteText.Shadow = false;
            }
        }
    }
}