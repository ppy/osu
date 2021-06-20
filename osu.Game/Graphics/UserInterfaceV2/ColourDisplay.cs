// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Graphics.UserInterfaceV2
{
    /// <summary>
    /// A component which displays a colour along with related description text.
    /// </summary>
    public class ColourDisplay : CompositeDrawable, IHasCurrentValue<Colour4>, IHasPopover
    {
        private readonly BindableWithCurrent<Colour4> current = new BindableWithCurrent<Colour4>();

        private Box fill;
        private OsuSpriteText colourHexCode;
        private OsuSpriteText colourName;

        public Bindable<Colour4> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private LocalisableString name;

        public LocalisableString ColourName
        {
            get => name;
            set
            {
                if (name == value)
                    return;

                name = value;

                colourName.Text = name;
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Y;
            Width = 100;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 10),
                Children = new Drawable[]
                {
                    new OsuClickableContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 100,
                        CornerRadius = 50,
                        Masking = true,
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
                                Font = OsuFont.Default.With(size: 12)
                            }
                        },
                        Action = this.ShowPopover
                    },
                    colourName = new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            current.BindValueChanged(_ => updateColour(), true);
        }

        private void updateColour()
        {
            fill.Colour = current.Value;
            colourHexCode.Text = current.Value.ToHex();
            colourHexCode.Colour = OsuColour.ForegroundTextColourFor(current.Value);
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
