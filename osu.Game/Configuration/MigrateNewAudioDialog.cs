// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.Settings.Sections.Audio;

namespace osu.Game.Configuration
{
    public partial class MigrateNewAudioDialog : PopupDialog
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        public MigrateNewAudioDialog(bool wasAlreadyUsing)
        {
            Icon = FontAwesome.Regular.Bell;

            if (wasAlreadyUsing)
            {
                HeaderText = DialogStrings.MigrateNewAudioAlreadyUsingHeaderText;
                BodyText = DialogStrings.MigrateNewAudioAlreadyUsingBodyText(AudioSettingsStrings.LegacyAudioLabel);
            }
            else
            {
                HeaderText = DialogStrings.MigrateNewAudioHeaderText;
                BodyText = DialogStrings.MigrateNewAudioBodyText(AudioSettingsStrings.LegacyAudioLabel);

                MainContent.Add(new Container
                {
                    Margin = new MarginPadding { Top = 20 },
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 400,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new LegacyAudioCheckbox(),
                    }
                });
            }

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = BeatmapOverlayStrings.UserContentConfirmButtonText,
                },
            };
        }
    }
}
