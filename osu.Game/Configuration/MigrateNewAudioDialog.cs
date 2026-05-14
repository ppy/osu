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

        public MigrateNewAudioDialog()
        {
            Icon = FontAwesome.Regular.Bell;

            HeaderText = @"New audio engine has been enabled";
            BodyText =
                $"""
                 We recently added a new "Experimental Audio" backend for Windows users to reduce hitsound latency. Due to overwhelmingly positive feedback, this is now default mode.

                 If you were already using this engine, your audio offset has been adjusted to account an internal offset change (no intervention required).

                 If you have any issues, you can switch back to the legacy engine below, or at any time in settings via the "{AudioSettingsStrings.LegacyAudioLabel}" checkbox.
                 """;

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogOkButton
                {
                    Text = BeatmapOverlayStrings.UserContentConfirmButtonText,
                },
            };

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
    }
}
