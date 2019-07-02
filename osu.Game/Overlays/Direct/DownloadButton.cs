// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;

namespace osu.Game.Overlays.Direct
{
    public class DownloadButton : BeatmapDownloadTrackingComposite
    {
        protected bool DownloadEnabled => button.Enabled.Value;

        private readonly bool noVideo;
        private readonly SpriteIcon icon;
        private readonly SpriteIcon checkmark;
        private readonly Box background;

        private OsuColour colours;
        private readonly ShakeContainer shakeContainer;
        private readonly OsuDownloadButton button;

        public DownloadButton(BeatmapSetInfo beatmapSet, bool noVideo = false)
            : base(beatmapSet)
        {
            this.noVideo = noVideo;

            InternalChild = shakeContainer = new ShakeContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = button = new OsuDownloadButton
                {
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            button.State.BindTo(State);
            FinishTransforms(true);
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours, OsuGame game, BeatmapManager beatmaps)
        {
            this.colours = colours;

            if (BeatmapSet.Value.OnlineInfo.Availability?.DownloadDisabled ?? false)
            {
                button.Enabled.Value = false;
                button.TooltipText = "This beatmap is currently not available for download.";
                return;
            }

            button.Action = () =>
            {
                switch (State.Value)
                {
                    case DownloadState.Downloading:
                    case DownloadState.Downloaded:
                        shakeContainer.Shake();
                        break;

                    case DownloadState.LocallyAvailable:
                        game.PresentBeatmap(BeatmapSet.Value);
                        break;

                    default:
                        beatmaps.Download(BeatmapSet.Value, noVideo);
                        break;
                }
            };
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            button?.State.UnbindAll();
        }
    }
}
