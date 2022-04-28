#nullable enable
// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Online;
using osuTK;

namespace osu.Game.Overlays.FirstRunSetup
{
    [Description("Obtaining Beatmaps")]
    public class ScreenBeatmaps : FirstRunSetupScreen
    {
        private RoundedButton downloadBundledButton = null!;
        private RoundedButton importBeatmapsButton = null!;

        private ProgressBar progressBarBundled = null!;

        private RoundedButton downloadTutorialButton = null!;
        private ProgressBar progressBarTutorial = null!;

        private BundledBeatmapDownloader tutorialDownloader = null!;
        private BundledBeatmapDownloader bundledDownloader = null!;

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuColour colours, OverlayColourProvider overlayColourProvider, LegacyImportManager? legacyImportManager)
        {
            Vector2 buttonSize = new Vector2(500, 60);

            Content.Children = new Drawable[]
            {
                new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: 20))
                {
                    Colour = overlayColourProvider.Content1,
                    Text =
                        "\"Beatmaps\" are what we call playable levels in osu!.\n\nosu! doesn't come with any beatmaps pre-loaded. This step will help you get started on your beatmap collection.",
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                },
                new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: 20))
                {
                    Colour = overlayColourProvider.Content1,
                    Text =
                        "If you are a new player, we recommend playing through the tutorial to get accustomed to the gameplay.",
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                },
                downloadTutorialButton = new RoundedButton
                {
                    Size = buttonSize,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    BackgroundColour = colours.Pink3,
                    Text = "Download tutorial",
                    Action = downloadTutorial
                },
                new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: 20))
                {
                    Colour = overlayColourProvider.Content1,
                    Text = "To get you started, we have some recommended beatmaps.",
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                },
                downloadBundledButton = new RoundedButton
                {
                    Size = buttonSize,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    BackgroundColour = colours.Blue3,
                    Text = "Download beatmap selection",
                    Action = downloadBundled
                },
                new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: 20))
                {
                    Colour = overlayColourProvider.Content1,
                    Text = "If you have an existing osu! install, you can also choose to import your existing beatmap collection.",
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                },
                importBeatmapsButton = new RoundedButton
                {
                    Size = buttonSize,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    BackgroundColour = colours.Blue3,
                    Text = MaintenanceSettingsStrings.ImportBeatmapsFromStable,
                    Action = () =>
                    {
                        importBeatmapsButton.Enabled.Value = false;
                        legacyImportManager?.ImportFromStableAsync(StableContent.Beatmaps).ContinueWith(t => Schedule(() => importBeatmapsButton.Enabled.Value = true));
                    }
                },
                new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: 20))
                {
                    Colour = overlayColourProvider.Content1,
                    Text = "You can also obtain more beatmaps from the main menu \"browse\" button at any time.",
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                },
            };
            downloadTutorialButton.Add(progressBarTutorial = new ProgressBar(false)
            {
                RelativeSizeAxes = Axes.Both,
                Blending = BlendingParameters.Additive,
                FillColour = downloadTutorialButton.BackgroundColour,
                Alpha = 0.5f,
                Depth = float.MinValue
            });

            downloadBundledButton.Add(progressBarBundled = new ProgressBar(false)
            {
                RelativeSizeAxes = Axes.Both,
                Blending = BlendingParameters.Additive,
                FillColour = downloadBundledButton.BackgroundColour,
                Alpha = 0.5f,
                Depth = float.MinValue
            });
        }

        private void downloadTutorial()
        {
            tutorialDownloader = new BundledBeatmapDownloader(true);

            AddInternal(tutorialDownloader);

            var downloadTracker = tutorialDownloader.DownloadTrackers.First();

            downloadTracker.Progress.BindValueChanged(progress =>
            {
                progressBarTutorial.Current.Value = progress.NewValue;

                if (progress.NewValue == 1)
                    downloadTutorialButton.Enabled.Value = false;
            }, true);
        }

        private void downloadBundled()
        {
            bundledDownloader = new BundledBeatmapDownloader(false);

            AddInternal(bundledDownloader);

            foreach (var tracker in bundledDownloader.DownloadTrackers)
                tracker.State.BindValueChanged(_ => updateProgress(), true);

            void updateProgress()
            {
                double progress = (double)bundledDownloader.DownloadTrackers.Count(t => t.State.Value == DownloadState.LocallyAvailable) / bundledDownloader.DownloadTrackers.Count();

                this.TransformBindableTo(progressBarBundled.Current, progress, 1000, Easing.OutQuint);

                if (progress == 1)
                    downloadBundledButton.Enabled.Value = false;
            }
        }
    }
}
