// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Settings.Sections;
using osu.Game.Overlays.Settings.Sections.Input;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public partial class SettingsOverlay : SettingsPanel, INamedOverlayComponent
    {
        public IconUsage Icon => HexaconsIcons.Settings;
        public LocalisableString Title => SettingsStrings.HeaderTitle;
        public LocalisableString Description => SettingsStrings.HeaderDescription;

        protected override IEnumerable<SettingsSection> CreateSections() => new SettingsSection[]
        {
            // This list should be kept in sync with ScreenBehaviour.
            new GeneralSection(),
            new SkinSection(),
            new InputSection(createSubPanel(new KeyBindingPanel())),
            new UserInterfaceSection(),
            new GameplaySection(),
            new RulesetSection(),
            new AudioSection(),
            new GraphicsSection(),
            new OnlineSection(),
            new MaintenanceSection(),
            new DebugSection(),
        };

        private readonly List<SettingsSubPanel> subPanels = new List<SettingsSubPanel>();

        private SettingsSubPanel lastOpenedSubPanel;

        protected override Drawable CreateHeader() => new SettingsHeader(Title, Description);
        protected override Drawable CreateFooter() => new SettingsFooter();

        public SettingsOverlay()
            : base(false)
        {
        }

        public override bool AcceptsFocus => lastOpenedSubPanel == null || lastOpenedSubPanel.State.Value == Visibility.Hidden;

        public void ShowAtControl<T>()
            where T : Drawable
        {
            Show();

            // wait for load of sections
            if (!SectionsContainer.Any())
            {
                Scheduler.Add(ShowAtControl<T>);
                return;
            }

            SectionsContainer.ScrollTo(SectionsContainer.ChildrenOfType<T>().Single());
        }

        private T createSubPanel<T>(T subPanel)
            where T : SettingsSubPanel
        {
            subPanel.Depth = 1;
            subPanel.Anchor = Anchor.TopRight;
            subPanel.State.ValueChanged += e => subPanelStateChanged(subPanel, e);

            subPanels.Add(subPanel);

            return subPanel;
        }

        private void subPanelStateChanged(SettingsSubPanel panel, ValueChangedEvent<Visibility> state)
        {
            switch (state.NewValue)
            {
                case Visibility.Visible:
                    Sidebar.Expanded.Value = false;
                    Sidebar.FadeColour(Color4.DarkGray, 300, Easing.OutQuint);

                    SectionsContainer.FadeOut(300, Easing.OutQuint);
                    ContentContainer.MoveToX(-PANEL_WIDTH, 500, Easing.OutQuint);

                    lastOpenedSubPanel = panel;

                    break;

                case Visibility.Hidden:
                    Sidebar.Expanded.Value = true;
                    Sidebar.FadeColour(Color4.White, 300, Easing.OutQuint);

                    SectionsContainer.FadeIn(500, Easing.OutQuint);
                    ContentContainer.MoveToX(0, 500, Easing.OutQuint);
                    break;
            }
        }

        protected override float ExpandedPosition => lastOpenedSubPanel?.State.Value == Visibility.Visible ? -PANEL_WIDTH : base.ExpandedPosition;

        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (var s in subPanels)
                ContentContainer.Add(s);
        }
    }
}
