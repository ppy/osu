// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public partial class ModPanel : ModSelectPanel, IFilterable
    {
        public Mod Mod => modState.Mod;
        public override BindableBool Active => modState.Active;

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

            modState.ValidForSelection.BindValueChanged(_ => updateFilterState());
            modState.MatchingTextFilter.BindValueChanged(_ => updateFilterState(), true);
        }

        protected override void Select()
        {
            modState.PendingConfiguration = Mod.RequiresConfiguration;
            Active.Value = true;
        }

        protected override void Deselect()
        {
            modState.PendingConfiguration = false;
            Active.Value = false;
        }

        #region Filtering support

        /// <seealso cref="ModState.Visible"/>
        public bool Visible => modState.Visible;

        public override IEnumerable<LocalisableString> FilterTerms => new[]
        {
            Mod.Name,
            Mod.Acronym,
            Mod.Description
        };

        public override bool MatchingFilter
        {
            get => modState.MatchingTextFilter.Value;
            set => modState.MatchingTextFilter.Value = value;
        }

        private void updateFilterState()
        {
            this.FadeTo(Visible ? 1 : 0);
        }

        #endregion
    }
}
