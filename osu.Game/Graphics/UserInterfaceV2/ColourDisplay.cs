// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterfaceV2
{
    /// <summary>
    /// A component which displays a colour along with related description text.
    /// </summary>
    public class ColourDisplay : CompositeDrawable, IHasCurrentValue<Color4>
    {
        private readonly BindableWithCurrent<Color4> current = new BindableWithCurrent<Color4>();

        private Box fill;
        private OsuSpriteText colourHexCode;
        private OsuSpriteText colourName;

        public Bindable<Color4> Current
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
                    new CircularContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 100,
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
                        }
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
    }
}
