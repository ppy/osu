// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Settings.Sections;

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
            keyBindingOverlay = new KeyBindingOverlay { Depth = 1 };
            keyBindingOverlay.StateChanged += keyBindingOverlay_StateChanged;
        }

        public override bool AcceptsFocus => keyBindingOverlay.State != Visibility.Visible;

        private void keyBindingOverlay_StateChanged(VisibilityContainer container, Visibility visibility)
        {
            const float hidden_width = 120;

            switch (visibility)
            {
                case Visibility.Visible:
                    Background.FadeTo(0.9f, 500, Easing.OutQuint);
                    SectionsContainer.FadeOut(100);
                    ContentContainer.MoveToX(hidden_width - ContentContainer.DrawWidth, 500, Easing.OutQuint);
                    break;
                case Visibility.Hidden:
                    Background.FadeTo(0.6f, 500, Easing.OutQuint);
                    SectionsContainer.FadeIn(500, Easing.OutQuint);
                    ContentContainer.MoveToX(0, 500, Easing.OutQuint);
                    break;
            }
        }

        protected override void PopOut()
        {
            base.PopOut();
            keyBindingOverlay.State = Visibility.Hidden;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(keyBindingOverlay);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            keyBindingOverlay.Margin = new MarginPadding { Left = ContentContainer.Margin.Left + ContentContainer.DrawWidth + ContentContainer.X };
        }
    }
}