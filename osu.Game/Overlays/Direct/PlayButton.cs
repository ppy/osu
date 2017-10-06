// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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

        private BeatmapSetInfo setInfo;
        public BeatmapSetInfo SetInfo
        {
            get { return setInfo; }
            set
            {
                if (value == setInfo) return;
                setInfo = value;

                Playing.Value = false;
                Preview = null;
            }
        }

        private Color4 hoverColour;
        private readonly SpriteIcon icon;
        private readonly LoadingAnimation loadingAnimation;
        private readonly Container audioWrapper;

        private const float transition_duration = 500;

        private bool loading;
        public bool Loading
        {
            get { return loading; }
            set
            {
                loading = value;
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
            SetInfo = setInfo;
            AddRange(new Drawable[]
            {
                audioWrapper = new Container(),
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

            Playing.ValueChanged += newValue => icon.Icon = newValue ? FontAwesome.fa_pause : FontAwesome.fa_play;
            Playing.ValueChanged += newValue => icon.FadeColour(newValue || IsHovered ? hoverColour : Color4.White, 120, Easing.InOutQuint);

            Playing.ValueChanged += updatePreviewTrack;
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
            if(!Playing.Value)
                icon.FadeColour(Color4.White, 120, Easing.InOutQuint);
            base.OnHoverLost(state);
        }

        protected override void Update()
        {
            base.Update();

            if(Preview?.HasCompleted ?? false)
            {
                Playing.Value = false;
                Preview = null;
            }
        }

        private void updatePreviewTrack(bool newValue)
        {
            if (newValue)
            {
                if (Preview == null)
                {
                    Loading = true;
                    audioWrapper.Child = new AsyncLoadWrapper(new AudioLoadWrapper("https://b.ppy.sh/preview/" + SetInfo.OnlineBeatmapSetID + ".mp3")
                    {
                        OnLoadComplete = d =>
                        {
                            Loading = false;
                            Preview = (d as AudioLoadWrapper)?.Preview;
                            Playing.TriggerChange();
                        },
                    });
                }
                else
                {
                    Preview.Seek(0);
                    Preview.Start();
                }
            }
            else
            {
                Preview?.Stop();
            }
        }

        private class AudioLoadWrapper : Drawable
        {
            private readonly string preview;

            public Track Preview;

            public AudioLoadWrapper(string preview)
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
