// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Online
{
    public partial class IntegrationSettings : SettingsSubsection
    {
        protected override LocalisableString Header => OnlineSettingsStrings.IntegrationsHeader;

        private Bindable<string?> gameStateServerUrl = null!;
        private readonly Bindable<SettingsNote.Data?> gameStateServerNote = new Bindable<SettingsNote.Data?>();

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, SessionStatics session)
        {
            gameStateServerUrl = session.GetBindable<string?>(Static.GameStateServerUrl);

            Children = new Drawable[]
            {
                new SettingsItemV2(new FormEnumDropdown<DiscordRichPresenceMode>
                {
                    Caption = OnlineSettingsStrings.DiscordRichPresence,
                    Current = config.GetBindable<DiscordRichPresenceMode>(OsuSetting.DiscordRichPresence)
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = OnlineSettingsStrings.GameStateIntegration,
                    HintText = OnlineSettingsStrings.GameStateIntegrationTooltip,
                    Current = config.GetBindable<bool>(OsuSetting.GameStateIntegration),
                })
                {
                    CanBeShown = { Value = RuntimeInfo.IsDesktop },
                    Note = { BindTarget = gameStateServerNote }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            gameStateServerUrl.BindValueChanged(e => Schedule(() =>
            {
                if (e.NewValue == null)
                {
                    gameStateServerNote.Value = null;
                }
                else
                {
                    gameStateServerNote.Value = new SettingsNote.Data(
                        OnlineSettingsStrings.GameStateIntegrationCurrentServer(e.NewValue),
                        SettingsNote.Type.Informational
                    );
                }
            }), true);
        }
    }
}
