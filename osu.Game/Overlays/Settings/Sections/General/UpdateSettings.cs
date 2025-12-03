// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Game.Online.Multiplayer;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.Notifications;
using osu.Game.Updater;

namespace osu.Game.Overlays.Settings.Sections.General
{
    public partial class UpdateSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GeneralSettingsStrings.UpdateHeader;

        private SettingsButton checkForUpdatesButton = null!;
        private SettingsEnumDropdown<ReleaseStream> releaseStreamDropdown = null!;

        private readonly Bindable<ReleaseStream> configReleaseStream = new Bindable<ReleaseStream>();

        [Resolved]
        private UpdateManager? updateManager { get; set; }

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        [Resolved]
        private OsuGame? game { get; set; }

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.ReleaseStream, configReleaseStream);

            bool isDesktop = RuntimeInfo.IsDesktop;

            // For simplicity, hide the concept of release streams from mobile users.
            if (isDesktop)
            {
                Add(releaseStreamDropdown = new SettingsEnumDropdown<ReleaseStream>
                {
                    LabelText = GeneralSettingsStrings.ReleaseStream,
                    Current = { Value = configReleaseStream.Value },
                    Keywords = new[] { @"version" },
                });

                if (updateManager!.FixedReleaseStream != null)
                {
                    configReleaseStream.Value = updateManager.FixedReleaseStream.Value;

                    releaseStreamDropdown.ShowsDefaultIndicator = false;
                    releaseStreamDropdown.Items = [updateManager.FixedReleaseStream.Value];
                    releaseStreamDropdown.SetNoticeText(GeneralSettingsStrings.ChangeReleaseStreamPackageManagerWarning);
                }

                releaseStreamDropdown.Current.BindValueChanged(releaseStreamChanged);
            }

            Add(checkForUpdatesButton = new SettingsButton
            {
                Text = GeneralSettingsStrings.CheckUpdate,
                Action = () => checkForUpdates().FireAndForget()
            });
        }

        private void releaseStreamChanged(ValueChangedEvent<ReleaseStream> stream)
        {
            if (stream.NewValue == ReleaseStream.Tachyon)
            {
                dialogOverlay?.Push(
                    new ConfirmDialog(GeneralSettingsStrings.ChangeReleaseStreamConfirmation,
                        () => configReleaseStream.Value = ReleaseStream.Tachyon,
                        () => releaseStreamDropdown.Current.Value = ReleaseStream.Lazer)
                    {
                        BodyText = GeneralSettingsStrings.ChangeReleaseStreamConfirmationInfo
                    });

                return;
            }

            configReleaseStream.Value = stream.NewValue;
        }

        private async Task checkForUpdates()
        {
            if (updateManager == null || game == null)
                return;

            checkForUpdatesButton.Enabled.Value = false;

            var checkingNotification = new ProgressNotification
            {
                Text = GeneralSettingsStrings.CheckingForUpdates,
            };
            notifications?.Post(checkingNotification);

            try
            {
                bool foundUpdate = await updateManager.CheckForUpdateAsync(checkingNotification.CancellationToken).ConfigureAwait(true);

                if (!foundUpdate)
                {
                    notifications?.Post(new SimpleNotification
                    {
                        Text = GeneralSettingsStrings.RunningLatestRelease(game.Version),
                        Icon = FontAwesome.Solid.CheckCircle,
                    });
                }
            }
            catch
            {
            }
            finally
            {
                // This sequence allows the notification to be immediately dismissed without posting a continuation message.
                checkingNotification.CompletionTarget = null;
                checkingNotification.State = ProgressNotificationState.Completed;
                checkingNotification.Close(false);
                checkForUpdatesButton.Enabled.Value = true;
            }
        }
    }
}
