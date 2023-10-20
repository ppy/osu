// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.Models;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Screens.Gameplay.Components
{
    public partial class TeamScore : CompositeDrawable
    {
        private readonly Bindable<int?> currentTeamScore = new Bindable<int?>();
        private readonly StarCounter counter;

        public TeamScore(Bindable<int?> score, TeamColour colour, int count)
        {
            bool flip = colour == TeamColour.Blue;
            var anchor = flip ? Anchor.TopRight : Anchor.TopLeft;

            AutoSizeAxes = Axes.Both;

            InternalChild = counter = new TeamScoreStarCounter(count)
            {
                Anchor = anchor,
                Scale = flip ? new Vector2(-1, 1) : Vector2.One,
            };

            currentTeamScore.BindValueChanged(scoreChanged);
            currentTeamScore.BindTo(score);
        }

        private void scoreChanged(ValueChangedEvent<int?> score) => counter.Current = score.NewValue ?? 0;

        public partial class TeamScoreStarCounter : StarCounter
        {
            public TeamScoreStarCounter(int count)
                : base(count)
            {
            }

            public override Star CreateStar() => new LightSquare();

            public partial class LightSquare : Star
            {
                private readonly Box box;

                public LightSquare()
                {
                    Size = new Vector2(22.5f);

                    InternalChildren = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            BorderColour = OsuColour.Gray(0.5f),
                            BorderThickness = 3,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = Color4.Transparent,
                                    RelativeSizeAxes = Axes.Both,
                                    AlwaysPresent = true,
                                },
                            }
                        },
                        box = new Box
                        {
                            Colour = Color4Extensions.FromHex("#FFE8AD"),
                            RelativeSizeAxes = Axes.Both,
                        },
                    };

                    Masking = true;
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = Color4Extensions.FromHex("#FFE8AD").Opacity(0.1f),
                        Hollow = true,
                        Radius = 20,
                        Roundness = 10,
                    };
                }

                public override void DisplayAt(float scale)
                {
                    box.FadeTo(scale, 500, Easing.OutQuint);
                    FadeEdgeEffectTo(0.2f * scale, 500, Easing.OutQuint);
                }
            }
        }
    }
}
