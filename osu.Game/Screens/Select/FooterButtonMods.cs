// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using System.Collections.Generic;
using osuTK;

namespace osu.Game.Screens.Select
{
    public class FooterButtonMods : FooterButton
    {
        private readonly Bindable<IEnumerable<Mod>> selectedMods = new Bindable<IEnumerable<Mod>>();

        private readonly FillFlowContainer<ModIcon> modIcons;

        public FooterButtonMods(Bindable<IEnumerable<Mod>> mods)
        {
            Add(modIcons = new FillFlowContainer<ModIcon>
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Direction = FillDirection.Horizontal,
                AutoSizeAxes = Axes.Both,
                Margin = new MarginPadding { Left = 80, Right = 20 }
            });

            if (mods != null)
            {
                selectedMods.BindTo(mods);
                selectedMods.ValueChanged += updateModIcons;
            }
        }

        private void updateModIcons(ValueChangedEvent<IEnumerable<Mod>> mods)
        {
            modIcons.Clear();
            foreach (Mod mod in mods.NewValue)
            {
                modIcons.Add(new ModIcon(mod) { Scale = new Vector2(0.4f) });
            }
        }
    }
}
