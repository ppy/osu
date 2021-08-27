// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play.HUD;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public class LocalPlayerModButton : ModButton
    {
        private readonly CompositeDrawable incompatibleIcon;

        [Resolved]
        private Bindable<IReadOnlyList<Mod>> selectedMods { get; set; }

        public LocalPlayerModButton(Mod mod)
            : base(mod)
        {
            ButtonContent.Add(incompatibleIcon = new IncompatibleIcon
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.Centre,
                Position = new Vector2(-13),
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedMods.BindValueChanged(_ => Scheduler.AddOnce(updateCompatibility), true);
        }

        protected override void DisplayMod(Mod mod)
        {
            base.DisplayMod(mod);

            Scheduler.AddOnce(updateCompatibility);
        }

        private void updateCompatibility()
        {
            var m = SelectedMod ?? Mods.First();

            bool isIncompatible = false;

            if (selectedMods.Value.Count > 0 && !selectedMods.Value.Contains(m))
                isIncompatible = !ModUtils.CheckCompatibleSet(selectedMods.Value.Append(m));

            if (isIncompatible)
                incompatibleIcon.Show();
            else
                incompatibleIcon.Hide();
        }
    }
}
