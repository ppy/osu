// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Direct
{
    public class PlayButton : Container
    {
        public readonly Bindable<bool> Playing = new Bindable<bool>();
        public Track Preview { get; private set; }

        private BeatmapSetInfo beatmapSet;
        public BeatmapSetInfo BeatmapSet
        {
            get { return beatmapSet; }
            set
            {
                if (value == beatmapSet) return;
                beatmapSet = value;

                Playing.Value = false;
                trackLoader = null;
                Preview = null;
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

            Playing.ValueChanged += playing =>
            {
                icon.Icon = playing ? FontAwesome.fa_pause : FontAwesome.fa_play;
                icon.FadeColour(playing || IsHovered ? hoverColour : Color4.White, 120, Easing.InOutQuint);
                updatePreviewTrack(playing);
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            hoverColour = colour.Yellow;
        }

        protected override bool OnClick(InputState state)
        {
            Playing.Value = !Playing.Value;
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

        protected override void Update()
        {
            base.Update();

            if (Preview?.HasCompleted ?? false)
            {
                Playing.Value = false;
                Preview = null;
            }
        }

        private void updatePreviewTrack(bool playing)
        {
            if (playing)
            {
                if (Preview == null)
                {
                    beginAudioLoad();
                    return;
                }

                Preview.Seek(0);
                Preview.Start();
            }
            else
            {
                Preview?.Stop();
                loading = false;
            }
        }

        private TrackLoader trackLoader;

        private void beginAudioLoad()
        {
            if (trackLoader != null) return;

            loading = true;

            LoadComponentAsync(trackLoader = new TrackLoader($"https://b.ppy.sh/preview/{BeatmapSet.OnlineBeatmapSetID}.mp3"),
                d =>
                {
                    // We may have been replaced by another loader
                    if (trackLoader != d) return;

                    Preview = d?.Preview;
                    Playing.TriggerChange();
                    loading = false;
                    Add(trackLoader);
                });
        }

        private class TrackLoader : Drawable
        {
            private readonly string preview;

            public Track Preview;

            public TrackLoader(string preview)
            {
                this.preview = preview;
            }

            [BackgroundDependencyLoader]
            private void load(AudioManager audio)
            {
                if (!string.IsNullOrEmpty(preview))
                {
                    Preview = audio.Track.Get(preview);
                    Preview.Volume.Value = 0.5;
                }
            }
        }
    }
}
