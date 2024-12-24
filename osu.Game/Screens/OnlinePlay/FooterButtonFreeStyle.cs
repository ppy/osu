// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.OnlinePlay
{
    public class FooterButtonFreeStyle : FooterButton, IHasCurrentValue<bool>
    {
        private readonly BindableWithCurrent<bool> current = new BindableWithCurrent<bool>();

        public Bindable<bool> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        public new Action Action { set => throw new NotSupportedException("The click action is handled by the button itself."); }

        private OsuSpriteText text = null!;
        private Circle circle = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public FooterButtonFreeStyle()
        {
            // Overwrite any external behaviour as we delegate the main toggle action to a sub-button.
            base.Action = () => current.Value = !current.Value;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ButtonContentContainer.AddRange(new[]
            {
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        circle = new Circle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = colours.YellowDark,
                            RelativeSizeAxes = Axes.Both,
                        },
                        text = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Padding = new MarginPadding(5),
                            UseFullGlyphHeight = false,
                        }
                    }
                }
            });

            SelectedColour = colours.Yellow;
            DeselectedColour = SelectedColour.Opacity(0.5f);
            Text = @"freestyle";
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(_ => updateDisplay(), true);
        }

        private void updateDisplay()
        {
            if (current.Value)
            {
                text.Text = "on";
                text.FadeColour(colours.Gray2, 200, Easing.OutQuint);
                circle.FadeColour(colours.Yellow, 200, Easing.OutQuint);
            }
            else
            {
                text.Text = "off";
                text.FadeColour(colours.GrayF, 200, Easing.OutQuint);
                circle.FadeColour(colours.Gray4, 200, Easing.OutQuint);
            }
        }
    }
}
