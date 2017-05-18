// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Play.Options
{
    public abstract class OptionContainer : Container
    {
        /// <summary>
        /// The title of this option.
        /// </summary>
        public abstract string Title { get; }

        private readonly OptionDropdown content;
        private readonly SimpleButton button;
        private bool contentIsVisible;

        protected OptionContainer()
        {
            AutoSizeAxes = Axes.Y;
            Width = 250;
            Masking = true;
            CornerRadius = 5;
            BorderColour = Color4.Black;
            BorderThickness = 2;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.5f,
                },
                new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,

                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 30,
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,

                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Origin = Anchor.CentreLeft,
                                    Anchor = Anchor.CentreLeft,
                                    Text = Title,
                                    TextSize = 17,
                                    Font = @"Exo2.0-Bold",
                                    Margin = new MarginPadding { Left = 10 },
                                },
                                button = new SimpleButton
                                {
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.CentreRight,
                                    Position = new Vector2(-15,0),
                                    Icon = FontAwesome.fa_bars,
                                    Scale = new Vector2(0.7f),
                                    Action = () => triggerContentVisibility(),
                                },
                            }
                        },
                        content = new OptionDropdown
                        {
                            RelativeSizeAxes = Axes.X,
                        }
                    }
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            content.StateChanged += (c, s) => button.FadeColour(s == Visibility.Visible ? colours.Yellow : Color4.White, 200, EasingTypes.OutQuint);
        }

        public new void Add(Drawable drawable)
        {
            content.Add(drawable);
        }

        private void triggerContentVisibility()
        {
            contentIsVisible = !contentIsVisible;

            if (contentIsVisible)
                content.Show();
            else
                content.Hide();
        }
    }
}
