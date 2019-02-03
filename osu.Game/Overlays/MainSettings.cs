// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Settings.Sections;
using osuTK.Graphics;
using System.Collections.Generic;

namespace osu.Game.Overlays
{
    public class MainSettings : SettingsOverlay
    {
        private readonly KeyBindingOverlay keyBindingOverlay;

        protected override IEnumerable<SettingsSection> CreateSections() => new SettingsSection[]
        {
            new GeneralSection(),
            new GraphicsSection(),
            new GameplaySection(),
            new AudioSection(),
            new SkinSection(),
            new InputSection(keyBindingOverlay),
            new OnlineSection(),
            new MaintenanceSection(),
            new DebugSection(),
        };

        protected override Drawable CreateHeader() => new SettingsHeader("settings", "Change the way osu! behaves");
        protected override Drawable CreateFooter() => new SettingsFooter();

        public MainSettings()
            : base(true)
        {
            keyBindingOverlay = new KeyBindingOverlay
            {
                Depth = 1,
                Anchor = Anchor.TopRight,
            };
            keyBindingOverlay.StateChanged += keyBindingOverlay_StateChanged;
        }

        public override bool AcceptsFocus => keyBindingOverlay.State != Visibility.Visible;

        private void keyBindingOverlay_StateChanged(Visibility visibility)
        {
            switch (visibility)
            {
                case Visibility.Visible:
                    Background.FadeTo(0.9f, 300, Easing.OutQuint);
                    Sidebar?.FadeColour(Color4.DarkGray, 300, Easing.OutQuint);

                    SectionsContainer.FadeOut(300, Easing.OutQuint);
                    ContentContainer.MoveToX(-WIDTH, 500, Easing.OutQuint);
                    break;
                case Visibility.Hidden:
                    Background.FadeTo(0.6f, 500, Easing.OutQuint);
                    Sidebar?.FadeColour(Color4.White, 300, Easing.OutQuint);

                    SectionsContainer.FadeIn(500, Easing.OutQuint);
                    ContentContainer.MoveToX(0, 500, Easing.OutQuint);
                    break;
            }
        }

        protected override float ExpandedPosition => keyBindingOverlay.State == Visibility.Visible ? -WIDTH : base.ExpandedPosition;

        [BackgroundDependencyLoader]
        private void load()
        {
            ContentContainer.Add(keyBindingOverlay);
        }
    }
}
