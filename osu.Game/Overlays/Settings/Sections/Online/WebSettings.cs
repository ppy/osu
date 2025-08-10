// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Threading;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;
// using osu.Game.Overlays.Settings;

namespace osu.Game.Overlays.Settings.Sections.Online
{
    public partial class WebSettings : SettingsSubsection
    {
        protected override LocalisableString Header => OnlineSettingsStrings.WebHeader;

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [Resolved]
        private OsuGame? game { get; set; }

        private SettingsTextBox customApiUrlTextBox = null!;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = OnlineSettingsStrings.ExternalLinkWarning,
                    Current = config.GetBindable<bool>(OsuSetting.ExternalLinkWarning)
                },
                new SettingsCheckbox
                {
                    LabelText = OnlineSettingsStrings.PreferNoVideo,
                    Keywords = new[] { "no-video" },
                    Current = config.GetBindable<bool>(OsuSetting.PreferNoVideo)
                },
                new SettingsCheckbox
                {
                    LabelText = OnlineSettingsStrings.AutomaticallyDownloadMissingBeatmaps,
                    Keywords = new[] { "spectator", "replay" },
                    Current = config.GetBindable<bool>(OsuSetting.AutomaticallyDownloadMissingBeatmaps),
                },
                new SettingsCheckbox
                {
                    LabelText = OnlineSettingsStrings.ShowExplicitContent,
                    Keywords = new[] { "nsfw", "18+", "offensive" },
                    Current = config.GetBindable<bool>(OsuSetting.ShowOnlineExplicitContent),
                },
                customApiUrlTextBox = new SettingsTextBox
                {
                    LabelText = OnlineSettingsStrings.CustomApiUrl,
                    Current = config.GetBindable<string>(OsuSetting.CustomApiUrl)
                }
            };

            customApiUrlTextBox.Current.BindValueChanged(onCustomApiUrlChanged, true);
        }

        private string lastApiUrl = string.Empty;
        private bool isInitialLoad = true;
        private ScheduledDelegate? pendingDialog;
        private const double debounce_delay = 500;

        private void onCustomApiUrlChanged(ValueChangedEvent<string> e)
        {
            if (isInitialLoad)
            {
                lastApiUrl = e.NewValue ?? string.Empty;
                isInitialLoad = false;
                return;
            }

            pendingDialog?.Cancel();

            string newValue = e.NewValue ?? string.Empty;

            if (string.Equals(lastApiUrl, newValue, StringComparison.OrdinalIgnoreCase))
                return;

            pendingDialog = Scheduler.AddDelayed(() =>
            {
                string currentValue = customApiUrlTextBox.Current.Value ?? string.Empty;
                if (string.Equals(lastApiUrl, currentValue, StringComparison.OrdinalIgnoreCase))
                    return;

                bool wasEmpty = string.IsNullOrWhiteSpace(lastApiUrl);
                bool isEmpty = string.IsNullOrWhiteSpace(currentValue);

                if (wasEmpty != isEmpty || (!wasEmpty && !isEmpty))
                {
                    showRestartDialog();
                }

                lastApiUrl = currentValue;
            }, debounce_delay);
        }

        private void showRestartDialog()
        {
            if (game?.RestartAppWhenExited() == true)
            {
                game.AttemptExit();
            }
            else
            {
                dialogOverlay?.Push(new ConfirmDialog(
                    OnlineSettingsStrings.CustomApiUrlRestartRequired,
                    () => game?.AttemptExit(),
                    () => { }
                )
                {
                    BodyText = OnlineSettingsStrings.CustomApiUrlRestartMessage
                });
            }
        }
    }
}
