// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Core.Screens.Evast;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.States;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Mods.Evast.Easings
{
    public class EasingsTestScreen : BeatmapScreen
    {
        private readonly FillFlowContainer flowContent;

        public EasingsTestScreen()
        {
            Child = new ScrollContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Width = 600,
                RelativeSizeAxes = Axes.Y,
                Child = flowContent = new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                }
            };

            for (int i = 0; i < 35; i++)
                flowContent.Add(new EasingTest(i));
        }

        private class EasingTest : Container
        {
            private readonly Container movingBox;
            private readonly int easingNumber;
            private readonly OsuSpriteText title;

            public EasingTest(int easingNumber)
            {
                this.easingNumber = easingNumber;

                Anchor = Anchor.TopCentre;
                Origin = Anchor.TopCentre;
                RelativeSizeAxes = Axes.X;
                Height = 110;
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black.Opacity(130),
                    },
                    title = new OsuSpriteText
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        Margin = new MarginPadding { Top = 10, Left = 10 },
                        TextSize = 30,
                        Text = ((Easing)easingNumber).ToString(),
                    },
                    new Container
                    {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Margin = new MarginPadding { Top = 50 },
                        Child = movingBox = new Container
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopCentre,
                            RelativePositionAxes = Axes.Both,
                            Size = new Vector2(50),
                            X = 0.1f,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.White,
                            }
                        },
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                title.Colour = colours.Yellow;
            }

            protected override bool OnHover(InputState state)
            {
                movingBox
                    .Delay(500)
                    .Then()
                    .MoveToX(0.9f, 1000, (Easing)easingNumber)
                    .Then()
                    .Delay(500)
                    .Then()
                    .MoveToX(0.1f, 1000, (Easing)easingNumber)
                    .Loop();

                return base.OnHover(state);
            }

            protected override void OnHoverLost(InputState state)
            {
                movingBox.ClearTransforms();
                movingBox.X = 0.1f;

                base.OnHoverLost(state);
            }
        }
    }
}
