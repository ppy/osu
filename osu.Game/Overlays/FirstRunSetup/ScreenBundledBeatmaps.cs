// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online;
using osuTK;

namespace osu.Game.Overlays.FirstRunSetup
{
    [Description("Bundled Beatmaps")]
    public class ScreenBundledBeatmaps : FirstRunSetupScreen
    {
        private RoundedButton downloadBundledButton;

        private ProgressBar progressBarBundled;

        private RoundedButton downloadTutorialButton;
        private ProgressBar progressBarTutorial;

        private BundledBeatmapDownloader tutorialDownloader;
        private BundledBeatmapDownloader bundledDownloader;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Vector2 buttonSize = new Vector2(500, 80);

            Content.Children = new Drawable[]
            {
                new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: 20))
                {
                    Text =
                        "osu! doesn't come with any beatmaps pre-loaded. To get started, we have some recommended beatmaps. You can obtain more beatmaps from the main menu \"browse\" button at any time.",
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
                downloadBundledButton = new RoundedButton
                {
                    Size = buttonSize,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    BackgroundColour = colours.Blue3,
                    Text = "Download beatmap selection",
                    Action = downloadBundled
                },
                // TODO: add stable import button if a stable install is detected.
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
            if (tutorialDownloader != null)
                return;

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
            if (bundledDownloader != null)
                return;

            // downloadBundledButton.Enabled.Value = false;

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
