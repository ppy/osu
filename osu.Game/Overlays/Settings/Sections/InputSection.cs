// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Handlers;
using osu.Framework.Input.Handlers.Mouse;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings.Sections.Input;

namespace osu.Game.Overlays.Settings.Sections
{
    public class InputSection : SettingsSection
    {
        private readonly KeyBindingPanel keyConfig;

        public override string Header => "Input";

        [Resolved]
        private GameHost host { get; set; }

        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Solid.Keyboard
        };

        public InputSection(KeyBindingPanel keyConfig)
        {
            this.keyConfig = keyConfig;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new BindingSettings(keyConfig),
            };

            foreach (var handler in host.AvailableInputHandlers)
            {
                var handlerSection = createSectionFor(handler);

                if (handlerSection != null)
                    Add(handlerSection);
            }
        }

        private SettingsSubsection createSectionFor(InputHandler handler)
        {
            var settingsControls = handler.CreateSettingsControlsFromAllBindables(false);

            if (settingsControls.Count == 0)
                return null;

            SettingsSubsection section;

            switch (handler)
            {
                case MouseHandler mh:
                    section = new MouseSettings(mh);
                    break;

                default:
                    section = new HandlerSection(handler);
                    break;
            }

            section.AddRange(settingsControls);

            return section;
        }

        private class HandlerSection : SettingsSubsection
        {
            private readonly InputHandler handler;

            public HandlerSection(InputHandler handler)
            {
                this.handler = handler;
            }

            protected override string Header => handler.Description;
        }
    }
}
