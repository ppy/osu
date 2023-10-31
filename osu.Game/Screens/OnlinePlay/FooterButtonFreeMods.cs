// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Select;
using osuTK;

namespace osu.Game.Screens.OnlinePlay
{
    public partial class FooterButtonFreeMods : FooterButton, IHasCurrentValue<IReadOnlyList<Mod>>
    {
        private readonly BindableWithCurrent<IReadOnlyList<Mod>> current = new BindableWithCurrent<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        public Bindable<IReadOnlyList<Mod>> Current
        {
            get => current.Current;
            set
            {
                ArgumentNullException.ThrowIfNull(value);

                current.Current = value;
            }
        }

        private OsuSpriteText count = null!;

        private Circle circle = null!;

        private readonly FreeModSelectOverlay freeModSelectOverlay;

        public FooterButtonFreeMods(FreeModSelectOverlay freeModSelectOverlay)
        {
            this.freeModSelectOverlay = freeModSelectOverlay;
        }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

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
                        count = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Padding = new MarginPadding(5),
                            UseFullGlyphHeight = false,
                        }
                    }
                },
                new IconButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(0.8f),
                    Icon = FontAwesome.Solid.Bars,
                    Action = () => freeModSelectOverlay.ToggleVisibility()
                }
            });

            SelectedColour = colours.Yellow;
            DeselectedColour = SelectedColour.Opacity(0.5f);
            Text = @"freemods";
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(_ => updateModDisplay(), true);

            // Overwrite any external behaviour as we delegate the main toggle action to a sub-button.
            Action = toggleAllFreeMods;
        }

        /// <summary>
        /// Immediately toggle all free mods on/off.
        /// </summary>
        private void toggleAllFreeMods()
        {
            var availableMods = allAvailableAndValidMods.ToArray();

            Current.Value = Current.Value.Count == availableMods.Length
                ? Array.Empty<Mod>()
                : availableMods;
        }

        private void updateModDisplay()
        {
            int currentCount = Current.Value.Count;

            if (currentCount == allAvailableAndValidMods.Count())
            {
                count.Text = "all";
                count.FadeColour(colours.Gray2, 200, Easing.OutQuint);
                circle.FadeColour(colours.Yellow, 200, Easing.OutQuint);
            }
            else if (currentCount > 0)
            {
                count.Text = $"{currentCount} mods";
                count.FadeColour(colours.Gray2, 200, Easing.OutQuint);
                circle.FadeColour(colours.YellowDark, 200, Easing.OutQuint);
            }
            else
            {
                count.Text = "off";
                count.FadeColour(colours.GrayF, 200, Easing.OutQuint);
                circle.FadeColour(colours.Gray4, 200, Easing.OutQuint);
            }
        }

        private IEnumerable<Mod> allAvailableAndValidMods => freeModSelectOverlay.AllAvailableMods
                                                                                 .Where(state => state.ValidForSelection.Value)
                                                                                 .Select(state => state.Mod);
    }
}
