// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Mods;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Mods
{
    public class CModSettingsContainer : Container
    {
        private readonly FillFlowContainer<ModControlSection> modSettingsContent;

        public CModSettingsContainer()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(0, 0, 0, 192)
                },
                new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = modSettingsContent = new FillFlowContainer<ModControlSection>
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(0f, 10f),
                        Padding = new MarginPadding(20),
                    }
                }
            };
        }

        ///<returns>Bool indicating whether any settings are listed</returns>
        public bool UpdateModSettings(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            modSettingsContent.Clear();

            foreach (var mod in mods.NewValue)
            {
                var settings = mod.CreateSettingsControls().ToList();
                if (settings.Count > 0)
                    modSettingsContent.Add(new ModControlSection(mod, settings));
            }

            bool hasSettings = modSettingsContent.Count > 0;

            if (!hasSettings)
                Hide();

            return hasSettings;
        }

        protected override bool OnMouseDown(MouseDownEvent e) => true;
        protected override bool OnHover(HoverEvent e) => true;
    }
}
