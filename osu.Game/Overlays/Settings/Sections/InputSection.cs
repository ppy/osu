// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
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

            if (RuntimeInfo.OS == RuntimeInfo.Platform.Android)
            {
                Add(new AndroidKeyboardSection());
            }

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
                case JoystickHandler _:
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

        private class AndroidKeyboardSection : SettingsSubsection
        {
            private Bindable<bool> keyboardFix = new Bindable<bool>();

            private SettingsCheckbox checkbox;

            [BackgroundDependencyLoader]
            private void load(FrameworkConfigManager config)
            {
                keyboardFix = config.GetBindable<bool>(FrameworkSetting.AndroidKeyboardFix);

                Children = new Drawable[]
                {
                    checkbox = new SettingsCheckbox
                    {
                        LabelText = "Enforce character based text input",
                        Current = keyboardFix
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                checkbox.WarningText = "Enable this and restart the game if you can't enter text with the keyboard.";
            }

            protected override LocalisableString Header => "Keyboard";
        }
    }
}
