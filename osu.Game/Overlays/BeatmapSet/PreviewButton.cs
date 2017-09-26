// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet
{
    public class PreviewButton : OsuClickableContainer
    {
        private const float transition_duration = 500;

        private readonly Container audioWrapper;
        private readonly Box bg, progress;
        private readonly SpriteIcon icon;
        private readonly LoadingAnimation loadingAnimation;

        private Track preview;

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

        private BeatmapSetInfo beatmapSet;
        public BeatmapSetInfo BeatmapSet
        {
            get { return beatmapSet; }
            set
            {
                if (value == beatmapSet) return;
                beatmapSet = value;

                Playing = false;
                preview = null;
            }
        }

        private bool playing;
        public bool Playing
        {
            get { return playing; }
            set
            {
                if (value == playing) return;
                playing = value;

                if (preview == null)
                {
                    loading = true;
                    audioWrapper.Child = new AsyncLoadWrapper(new AudioLoadWrapper(BeatmapSet)
                    {
                        OnLoadComplete = d =>
                        {
                            loading = false;

                            preview = (d as AudioLoadWrapper)?.Preview;
                            Playing = Playing;
                            updatePlayingState();
                        },
                    });

                    return;
                }

                updatePlayingState();
            }
        }

        public PreviewButton()
        {
            Height = 42;

            Children = new Drawable[]
            {
                audioWrapper = new Container(),
                bg = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.25f),
                },
                new Container
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = 3,
                    Child = progress = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0f,
                        Alpha = 0f,
                    },
                },
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.fa_play,
                    Size = new Vector2(18),
                    Shadow = false,
                },
                loadingAnimation = new LoadingAnimation
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
            };

            Action = () => Playing = !Playing;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            progress.Colour = colours.Yellow;
        }

        protected override void Update()
        {
            base.Update();

            if (Playing && preview != null)
            {
                progress.Width = (float)(preview.CurrentTime / preview.Length);
                if (preview.HasCompleted)
                {
                    Playing = false;
                    preview = null;
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            Playing = false;
            base.Dispose(isDisposing);
        }

        protected override bool OnHover(InputState state)
        {
            bg.FadeColour(Color4.Black.Opacity(0.5f), 100);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            bg.FadeColour(Color4.Black.Opacity(0.25f), 100);
            base.OnHoverLost(state);
        }

        private void updatePlayingState()
        {
            if (preview == null) return;

            if (Playing)
            {
                icon.Icon = FontAwesome.fa_stop;
                progress.FadeIn(100);

                preview.Seek(0);
                preview.Start();
            }
            else
            {
                icon.Icon = FontAwesome.fa_play;
                progress.FadeOut(100);
                preview.Stop();
            }
        }

        private class AudioLoadWrapper : Drawable
        {
            private readonly string preview;

            public Track Preview;

            public AudioLoadWrapper(BeatmapSetInfo set)
            {
                preview = set.OnlineInfo.Preview;
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
