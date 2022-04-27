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
using osu.Game.Online;
using osuTK.Graphics;

namespace osu.Game.Overlays.FirstRunSetup
{
    [Description("Bundled Beatmaps")]
    public class ScreenBundledBeatmaps : FirstRunSetupScreen
    {
        private TriangleButton downloadButton;

        private ProgressBar progressBar;
        private BundledBeatmapDownloader downloader;

        [BackgroundDependencyLoader]
        private void load()
        {
            Content.Children = new Drawable[]
            {
                new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: 20))
                {
                    Text = "osu! doesn't come with any beatmaps pre-loaded. To get started, we have some recommended beatmaps.",
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                },
                downloadButton = new TriangleButton
                {
                    Width = 300,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = "Download beatmap selection",
                    Action = download
                },
            };

            downloadButton.Add(progressBar = new ProgressBar(false)
            {
                RelativeSizeAxes = Axes.Both,
                Blending = BlendingParameters.Additive,
                FillColour = Color4.Aqua,
                Alpha = 0.5f,
                Depth = float.MinValue
            });
        }

        private void download()
        {
            AddInternal(downloader = new BundledBeatmapDownloader());
            downloadButton.Enabled.Value = false;

            foreach (var tracker in downloader.DownloadTrackers)
                tracker.State.BindValueChanged(_ => updateProgress());
        }

        private void updateProgress()
        {
            double progress = (double)downloader.DownloadTrackers.Count(t => t.State.Value == DownloadState.LocallyAvailable) / downloader.DownloadTrackers.Count();

            this.TransformBindableTo(progressBar.Current, progress, 1000, Easing.OutQuint);
        }
    }
}
