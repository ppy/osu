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
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Select;
using osuTK;

namespace osu.Game.Screens.OnlinePlay
{
    public partial class FooterButtonFreeMods : FooterButton
    {
        public readonly Bindable<IReadOnlyList<Mod>> FreeMods = new Bindable<IReadOnlyList<Mod>>();
        public readonly IBindable<bool> Freestyle = new Bindable<bool>();

        protected override bool IsActive => FreeMods.Value.Count > 0;

        public new Action Action { set => throw new NotSupportedException("The click action is handled by the button itself."); }

        private OsuSpriteText count = null!;
        private Circle circle = null!;

        private readonly FreeModSelectOverlay freeModSelectOverlay;

        public FooterButtonFreeMods(FreeModSelectOverlay freeModSelectOverlay)
        {
            this.freeModSelectOverlay = freeModSelectOverlay;

            // Overwrite any external behaviour as we delegate the main toggle action to a sub-button.
            base.Action = toggleAllFreeMods;
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

            TooltipText = MultiplayerMatchStrings.FreeModsButtonTooltip;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Freestyle.BindValueChanged(_ => updateModDisplay());
            FreeMods.BindValueChanged(_ => updateModDisplay(), true);
        }

        /// <summary>
        /// Immediately toggle all free mods on/off.
        /// </summary>
        private void toggleAllFreeMods()
        {
            var availableMods = allAvailableAndValidMods.ToArray();

            FreeMods.Value = FreeMods.Value.Count == availableMods.Length
                ? Array.Empty<Mod>()
                : availableMods;
        }

        private void updateModDisplay()
        {
            int currentCount = FreeMods.Value.Count;

            if (currentCount == allAvailableAndValidMods.Count() || Freestyle.Value)
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
