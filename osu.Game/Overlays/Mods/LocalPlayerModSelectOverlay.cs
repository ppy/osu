// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays.Mods
{
    public class LocalPlayerModSelectOverlay : ModSelectOverlay
    {
        private SelectAllButton selectAllButton;
        private readonly BindableBool optUI = new BindableBool();

        [BackgroundDependencyLoader]
        private void load(MConfigManager mConfig)
        {
            mConfig.BindWith(MSetting.OptUI, optUI);

            FooterContainer.Add(selectAllButton = new SelectAllButton
            {
                Origin = Anchor.CentreLeft,
                Anchor = Anchor.CentreLeft,
                Width = 180,
                Text = "全选",
                Action = selectAll,
                TooltipText = "¿",
                Alpha = 0
            });

            optUI.BindValueChanged(v =>
            {
                if (v.NewValue) selectAllButton.FadeIn(300);
                else selectAllButton.FadeOut(300);
            }, true);

            FooterContainer.SetLayoutPosition(selectAllButton, -2);
        }

        protected override void OnModSelected(Mod mod)
        {
            base.OnModSelected(mod);

            foreach (var section in ModSectionsContainer.Children)
                section.DeselectTypes(mod.IncompatibleMods, true, mod);
        }

        private void selectAll()
        {
            foreach (var section in ModSectionsContainer.Children)
                section.SelectAll();
        }

        private class SelectAllButton : TriangleButton, IHasTooltip
        {
            public LocalisableString TooltipText { get; set; }
        }
    }
}
