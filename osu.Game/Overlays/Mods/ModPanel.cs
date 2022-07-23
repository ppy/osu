// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public class ModPanel : ModSelectPanel
    {
        public Mod Mod => modState.Mod;
        public override BindableBool Active => modState.Active;
        public BindableBool Filtered => modState.Filtered;

        protected override float IdleSwitchWidth => 54;
        protected override float ExpandedSwitchWidth => 70;

        private readonly ModState modState;

        public ModPanel(ModState modState)
        {
            this.modState = modState;

            Title = Mod.Name;
            Description = Mod.Description;

            SwitchContainer.Child = new ModSwitchSmall(Mod)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Active = { BindTarget = Active },
                Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0),
                Scale = new Vector2(HEIGHT / ModSwitchSmall.DEFAULT_SIZE)
            };
        }

        public ModPanel(Mod mod)
            : this(new ModState(mod))
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AccentColour = colours.ForModType(Mod.Type);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Filtered.BindValueChanged(_ => updateFilterState(), true);
        }

        #region Filtering support

        private void updateFilterState()
        {
            this.FadeTo(Filtered.Value ? 0 : 1);
        }

        #endregion
    }
}
