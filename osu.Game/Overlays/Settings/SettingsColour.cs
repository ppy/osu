// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Overlays.Settings
{
    public partial class SettingsColour : SettingsItem<Colour4>
    {
        protected override Drawable CreateControl() => new ColourControl();

        public partial class ColourControl : OsuClickableContainer, IHasPopover, IHasCurrentValue<Colour4>
        {
            private readonly BindableWithCurrent<Colour4> current = new BindableWithCurrent<Colour4>(Colour4.White);

            public Bindable<Colour4> Current
            {
                get => current.Current;
                set => current.Current = value;
            }

            private readonly Box fill;
            private readonly OsuSpriteText colourHexCode;

            public ColourControl()
            {
                RelativeSizeAxes = Axes.X;
                Height = 40;
                CornerRadius = 20;
                Masking = true;
                Action = this.ShowPopover;

                Children = new Drawable[]
                {
                    fill = new Box
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    colourHexCode = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = OsuFont.Default.With(size: 20)
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Current.BindValueChanged(_ => updateColour(), true);
            }

            private void updateColour()
            {
                fill.Colour = Current.Value;
                colourHexCode.Text = Current.Value.ToHex();
                colourHexCode.Colour = OsuColour.ForegroundTextColourFor(Current.Value);
            }

            public Popover GetPopover() => new OsuPopover(false)
            {
                Child = new OsuColourPicker
                {
                    Current = { BindTarget = Current }
                }
            };
        }
    }
}
