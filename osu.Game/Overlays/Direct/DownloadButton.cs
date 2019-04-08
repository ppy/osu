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
using osuTK;

namespace osu.Game.Overlays.Direct
{
    public class DownloadButton : DownloadTrackingComposite
    {
        private readonly bool noVideo;
        private readonly SpriteIcon icon;
        private readonly SpriteIcon checkmark;
        private readonly Box background;

        private OsuColour colours;

        private readonly ShakeContainer shakeContainer;

        private readonly OsuAnimatedButton button;

        public DownloadButton(BeatmapSetInfo beatmapSet, bool noVideo = false)
            : base(beatmapSet)
        {
            this.noVideo = noVideo;

            InternalChild = shakeContainer = new ShakeContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = button = new OsuAnimatedButton
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        background = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Depth = float.MaxValue
                        },
                        icon = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(13),
                            Icon = FontAwesome.Solid.Download,
                        },
                        checkmark = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            X = 8,
                            Size = Vector2.Zero,
                            Icon = FontAwesome.Solid.Check,
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            State.BindValueChanged(state => updateState(state.NewValue), true);
            FinishTransforms(true);
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(OsuColour colours, OsuGame game, BeatmapManager beatmaps)
        {
            this.colours = colours;

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

        private void updateState(DownloadState state)
        {
            switch (state)
            {
                case DownloadState.NotDownloaded:
                    background.FadeColour(colours.Gray4, 500, Easing.InOutExpo);
                    icon.MoveToX(0, 500, Easing.InOutExpo);
                    checkmark.ScaleTo(Vector2.Zero, 500, Easing.InOutExpo);
                    break;

                case DownloadState.Downloading:
                    background.FadeColour(colours.Blue, 500, Easing.InOutExpo);
                    icon.MoveToX(0, 500, Easing.InOutExpo);
                    checkmark.ScaleTo(Vector2.Zero, 500, Easing.InOutExpo);
                    break;
                case DownloadState.Downloaded:
                    background.FadeColour(colours.Yellow, 500, Easing.InOutExpo);
                    break;
                case DownloadState.LocallyAvailable:
                    background.FadeColour(colours.Green, 500, Easing.InOutExpo);
                    icon.MoveToX(-8, 500, Easing.InOutExpo);
                    checkmark.ScaleTo(new Vector2(13), 500, Easing.InOutExpo);
                    break;
            }
        }
    }
}
