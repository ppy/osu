// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Direct
{
    public class PlayButton : Container
    {
        public readonly Bindable<bool> Playing = new Bindable<bool>();
        public PreviewTrack Preview { get; private set; }

        private BeatmapSetInfo beatmapSet;

        public BeatmapSetInfo BeatmapSet
        {
            get { return beatmapSet; }
            set
            {
                if (value == beatmapSet) return;
                beatmapSet = value;

                if (Preview != null)
                {
                    Preview.Stop();
                    RemoveInternal(Preview);
                    Preview = null;
                }

                Playing.Value = false;
            }
        }

        private Color4 hoverColour;
        private readonly SpriteIcon icon;
        private readonly LoadingAnimation loadingAnimation;

        private const float transition_duration = 500;

        private bool loading
        {
            set
            {
                if (value)
                {
                    loadingAnimation.Show();
                    icon.FadeOut(transition_duration * 5, Easing.OutQuint);
                }
                else
                {
                    loadingAnimation.Hide();
                    icon.FadeIn(transition_duration, Easing.OutQuint);
                }
            }
        }

        public PlayButton(BeatmapSetInfo setInfo = null)
        {
            BeatmapSet = setInfo;
            AddRange(new Drawable[]
            {
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    FillMode = FillMode.Fit,
                    RelativeSizeAxes = Axes.Both,
                    Icon = FontAwesome.fa_play,
                },
                loadingAnimation = new LoadingAnimation(),
            });

            Playing.ValueChanged += playingStateChanged;
        }

        private PreviewTrackManager previewTrackManager;

        [BackgroundDependencyLoader]
        private void load(OsuColour colour, PreviewTrackManager previewTrackManager)
        {
            this.previewTrackManager = previewTrackManager;

            hoverColour = colour.Yellow;
        }

        protected override bool OnClick(InputState state)
        {
            if (!Playing.Value)
            {
                if (Preview == null)
                {
                    loading = true;

                    Preview = previewTrackManager.Get(beatmapSet);
                    Preview.Started += () => Playing.Value = true;
                    Preview.Stopped += () => Playing.Value = false;

                    LoadComponentAsync(Preview, t =>
                    {
                        AddInternal(t);

                        Preview.Start();

                        loading = false;
                    });

                    return true;
                }

                Preview.Start();
            }
            else
                Preview?.Stop();

            return true;
        }

        protected override bool OnHover(InputState state)
        {
            icon.FadeColour(hoverColour, 120, Easing.InOutQuint);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            if (!Playing.Value)
                icon.FadeColour(Color4.White, 120, Easing.InOutQuint);
            base.OnHoverLost(state);
        }

        private void playingStateChanged(bool playing)
        {
            if (playing && BeatmapSet == null)
            {
                Playing.Value = false;
                return;
            }

            icon.Icon = playing ? FontAwesome.fa_stop : FontAwesome.fa_play;
            icon.FadeColour(playing || IsHovered ? hoverColour : Color4.White, 120, Easing.InOutQuint);

            if (!playing)
            {
                loading = false;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Playing.Value = false;
        }
    }
}
