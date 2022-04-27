// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.FirstRunSetup
{
    [Description("Bundled Beatmaps")]
    public class ScreenBundledBeatmaps : FirstRunSetupScreen
    {
        private TriangleButton downloadButton;

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
                }
            };
        }

        private void download()
        {
            AddInternal(new BundledBeatmapDownloader());
            downloadButton.Enabled.Value = false;
        }
    }
}
