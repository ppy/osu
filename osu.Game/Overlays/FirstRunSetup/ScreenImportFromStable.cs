// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osuTK;

namespace osu.Game.Overlays.FirstRunSetup
{
    [LocalisableDescription(typeof(FirstRunSetupOverlayStrings), nameof(FirstRunSetupOverlayStrings.ImportTitle))]
    public class ScreenImportFromStable : FirstRunSetupScreen
    {
        private ProgressRoundedButton importButton = null!;

        private OsuTextFlowContainer currentStablePath = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private LegacyImportManager legacyImportManager { get; set; } = null!;

        private CancellationTokenSource? stablePathUpdateCancellation;

        private IEnumerable<ImportCheckbox> contentCheckboxes => Content.Children.OfType<ImportCheckbox>();

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
                currentStablePath = new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: HEADER_FONT_SIZE, weight: FontWeight.SemiBold))
                {
                    Colour = OverlayColourProvider.Content2,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                },
                new RoundedButton
                {
                    Size = buttonSize,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    BackgroundColour = colours.Blue3,
                    Text = "Locate osu!(stable) install",
                    Action = locateStable,
                },
                new ImportCheckbox("Beatmaps", StableContent.Beatmaps),
                new ImportCheckbox("Scores", StableContent.Scores),
                new ImportCheckbox("Skins", StableContent.Skins),
                new ImportCheckbox("Collections", StableContent.Collections),
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

            updateStablePath();
        }

        private void locateStable()
        {
            legacyImportManager.ImportFromStableAsync(0).ContinueWith(task =>
            {
                Schedule(updateStablePath);
            });
        }

        private void updateStablePath()
        {
            stablePathUpdateCancellation?.Cancel();

            var storage = legacyImportManager.GetCurrentStableStorage();

            if (storage == null)
            {
                foreach (var c in contentCheckboxes)
                    c.Current.Disabled = true;
                currentStablePath.Text = "No installation found";
                return;
            }

            foreach (var c in contentCheckboxes)
            {
                c.Current.Disabled = false;
                c.UpdateCount();
            }

            currentStablePath.Text = storage.GetFullPath(string.Empty);
            stablePathUpdateCancellation = new CancellationTokenSource();
        }

        private void runImport()
        {
            importButton.Enabled.Value = false;

            StableContent importableContent = 0;

            foreach (var c in contentCheckboxes.Where(c => c.Current.Value))
                importableContent |= c.StableContent;

            legacyImportManager.ImportFromStableAsync(importableContent, false).ContinueWith(t => Schedule(() =>
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

        private class ImportCheckbox : SettingsCheckbox
        {
            public readonly StableContent StableContent;

            private readonly LocalisableString title;

            [Resolved]
            private LegacyImportManager legacyImportManager { get; set; } = null!;

            private CancellationTokenSource? countUpdateCancellation;

            public ImportCheckbox(LocalisableString title, StableContent stableContent)
            {
                this.title = title;

                StableContent = stableContent;

                Current.Value = true;

                LabelText = title;
            }

            public void UpdateCount()
            {
                LabelText = LocalisableString.Interpolate($"{title} (calculating...)");

                countUpdateCancellation?.Cancel();
                countUpdateCancellation = new CancellationTokenSource();

                legacyImportManager.GetImportCount(StableContent, countUpdateCancellation.Token).ContinueWith(task => Schedule(() =>
                {
                    if (task.IsCanceled)
                        return;

                    LabelText = LocalisableString.Interpolate($"{title} ({task.GetResultSafely()} items)");
                }));
            }
        }
    }
}
