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
using osu.Game.Audio;
using osu.Game.Overlays.Direct;
using osu.Framework.Configuration;

namespace osu.Game.Overlays.BeatmapSet
{
    public class PreviewButton : OsuClickableContainer
    {
        private const float transition_duration = 500;

        private readonly Container audioWrapper;
        private readonly Box bg, progress;
        private readonly PlayButton playButton;

        private Track preview;
        private readonly Bindable<bool> playing = new Bindable<bool>();

        private BeatmapSetInfo beatmapSet;
        public BeatmapSetInfo BeatmapSet
        {
            get { return beatmapSet; }
            set
            {
                if (value == beatmapSet) return;
                beatmapSet = value;

                playing.Value = false;
                preview = null;
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
                playButton = new PlayButton(playing)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(18),
                },
            };

            Action = () => playing.Value = !playing.Value;
            playing.ValueChanged += updatePlayingState;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            progress.Colour = colours.Yellow;
        }

        protected override void Update()
        {
            base.Update();

            if (playing.Value && preview != null)
            {
                progress.Width = (float)(preview.CurrentTime / preview.Length);
                if (preview.HasCompleted)
                {
                    playing.Value = false;
                    preview = null;
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            playing.Value = false;
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

        private void updatePlayingState(bool newValue)
        {
            if (preview == null)
            {
                playButton.Loading = true;
                audioWrapper.Child = new AsyncLoadWrapper(new AudioLoadWrapper(BeatmapSet.OnlineInfo.Preview)
                {
                    OnLoadComplete = d =>
                    {
                        playButton.Loading = false;

                        preview = (d as AudioLoadWrapper)?.Preview;
                        playing.TriggerChange();
                    },
                });
            }
            else
            {
                if (newValue)
                {
                    progress.FadeIn(100);

                    preview.Seek(0);
                    preview.Start();
                }
                else
                {
                    progress.FadeOut(100);
                    preview.Stop();
                }
            }
        }
    }
}
