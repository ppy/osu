// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Handlers;
using osu.Framework.Input.Handlers.Joystick;
using osu.Framework.Input.Handlers.Midi;
using osu.Framework.Input.Handlers.Mouse;
using osu.Framework.Input.Handlers.Tablet;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings.Sections.Input;

namespace osu.Game.Overlays.Settings.Sections
{
    public class InputSection : SettingsSection
    {
        private readonly KeyBindingPanel keyConfig;

        public override LocalisableString Header => InputSettingsStrings.InputSectionHeader;

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
            SettingsSubsection section;

            switch (handler)
            {
                // ReSharper disable once SuspiciousTypeConversion.Global (net standard fuckery)
                case ITabletHandler th:
                    section = new TabletSettings(th);
                    break;

                case MouseHandler mh:
                    section = new MouseSettings(mh);
                    break;

                // whitelist the handlers which should be displayed to avoid any weird cases of users touching settings they shouldn't.
                case JoystickHandler jh:
                    section = new JoystickSettings(jh);
                    break;

                case MidiHandler _:
                    section = new HandlerSection(handler);
                    break;

                default:
                    return null;
            }

            return section;
        }

        private class HandlerSection : SettingsSubsection
        {
            private readonly InputHandler handler;

            public HandlerSection(InputHandler handler)
            {
                this.handler = handler;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Children = new Drawable[]
                {
                    new SettingsCheckbox
                    {
                        LabelText = CommonStrings.Enabled,
                        Current = handler.Enabled
                    },
                };
            }

            protected override LocalisableString Header => handler.Description;
        }
    }
}
