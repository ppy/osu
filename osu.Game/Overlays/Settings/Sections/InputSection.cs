// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Handlers;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings.Sections.Input;

namespace osu.Game.Overlays.Settings.Sections
{
    public partial class InputSection : SettingsSection
    {
        private readonly KeyBindingPanel keyConfig;

        public override LocalisableString Header => InputSettingsStrings.InputSectionHeader;

        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Solid.Keyboard
        };

        public InputSection(KeyBindingPanel keyConfig)
        {
            this.keyConfig = keyConfig;
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host, OsuGameBase game)
        {
            Children = new Drawable[]
            {
                new BindingSettings(keyConfig),
                new FrameworkActionManagerSetings()
            };

            foreach (var handler in host.AvailableInputHandlers)
            {
                var handlerSection = game.CreateSettingsSubsectionFor(handler);

                if (handlerSection != null)
                    Add(handlerSection);
            }
        }

        public partial class HandlerSection : SettingsSubsection
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

    internal partial class FrameworkActionManagerSetings : SettingsSubsection
    {
        protected override LocalisableString Header => "框架";

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager fcm)
        {
            Children = new[]
            {
                new SettingsCheckbox
                {
                    LabelText = "忽略框架KeyBind",
                    Current = fcm.GetBindable<bool>(FrameworkSetting.ActionContainerIgnoreKeyBind)
                }
            };
        }
    }
}
