// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osuTK;

namespace osu.Game.Overlays.FirstRunSetup
{
    [LocalisableDescription(typeof(FirstRunSetupOverlayStrings), nameof(FirstRunSetupOverlayStrings.ImportTitle))]
    public class ScreenImportFromStable : FirstRunSetupScreen
    {
        private ProgressRoundedButton importButton = null!;

        private SettingsCheckbox checkboxSkins = null!;
        private SettingsCheckbox checkboxBeatmaps = null!;
        private SettingsCheckbox checkboxScores = null!;
        private SettingsCheckbox checkboxCollections = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved(canBeNull: true)]
        private LegacyImportManager? legacyImportManager { get; set; }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load()
        {
            Vector2 buttonSize = new Vector2(400, 50);

            Content.Children = new Drawable[]
            {
                new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: CONTENT_FONT_SIZE))
                {
                    Colour = OverlayColourProvider.Content1,
                    Text =
                        "If you have an installation of a previous osu! version, you can choose to migrate your existing content. Note that this will create a copy, and not affect your existing installation.",
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                },
                checkboxBeatmaps = new SettingsCheckbox
                {
                    LabelText = "Beatmaps",
                    Current = { Value = true }
                },
                checkboxScores = new SettingsCheckbox
                {
                    LabelText = "Scores",
                    Current = { Value = true }
                },
                checkboxSkins = new SettingsCheckbox
                {
                    LabelText = "Skins",
                    Current = { Value = true }
                },
                checkboxCollections = new SettingsCheckbox
                {
                    LabelText = "Collections",
                    Current = { Value = true }
                },
                importButton = new ProgressRoundedButton
                {
                    Size = buttonSize,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    BackgroundColour = colours.Blue3,
                    Text = FirstRunSetupOverlayStrings.ImportContentFromStable,
                    Action = runImport
                },
            };
        }

        private void runImport()
        {
            importButton.Enabled.Value = false;

            StableContent importableContent = 0;

            if (checkboxBeatmaps.Current.Value) importableContent |= StableContent.Beatmaps;
            if (checkboxScores.Current.Value) importableContent |= StableContent.Scores;
            if (checkboxSkins.Current.Value) importableContent |= StableContent.Skins;
            if (checkboxCollections.Current.Value) importableContent |= StableContent.Collections;

            legacyImportManager?.ImportFromStableAsync(importableContent)
                               .ContinueWith(t => Schedule(() =>
                               {
                                   if (t.IsCompletedSuccessfully)
                                       importButton.Complete();
                                   else
                                   {
                                       importButton.Enabled.Value = true;
                                       importButton.Abort();
                                   }
                               }));
        }
    }
}
