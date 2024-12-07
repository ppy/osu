// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// Displays a single-line horizontal auto-sized flow of mods. For cases where wrapping is required, use <see cref="ModFlowDisplay"/> instead.
    /// </summary>
    public partial class SkinnableModDisplay : CompositeDrawable, ISerialisableDrawable
    {
        private ModDisplay modDisplay = null!;

        [Resolved]
        private Bindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [SettingSource("Show extended info", "Whether to show extended information for each mod.")]
        public Bindable<bool> ShowExtendedInformation { get; } = new Bindable<bool>(true);

        [SettingSource("Expansion mode", "How the mod display expands when interacted with.")]
        public Bindable<ExpansionMode> ExpansionModeSetting { get; } = new Bindable<ExpansionMode>(ExpansionMode.ExpandOnHover);

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = modDisplay = new ModDisplay();
            modDisplay.Current = mods;
            AutoSizeAxes = Axes.Both;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ShowExtendedInformation.BindValueChanged(_ => modDisplay.ShowExtendedInformation = ShowExtendedInformation.Value, true);
            ExpansionModeSetting.BindValueChanged(_ => modDisplay.ExpansionMode = ExpansionModeSetting.Value, true);

            FinishTransforms(true);
        }

        public bool UsesFixedAnchor { get; set; }
    }
}
