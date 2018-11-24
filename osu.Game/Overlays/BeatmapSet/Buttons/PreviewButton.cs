// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays.Direct;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet.Buttons
{
    public class PreviewButton : OsuClickableContainer
    {
        public BindableBool Playing { get; }

        private const float transition_duration = 500;

        private readonly Box bg, progress;
        private readonly PlayButton playButton;
        private PreviewTrackManager previewTrackManager;

        private PlayButtonState _playButtonState;
        private PlayButtonState playButtonState
        {
            get { return _playButtonState; }
            set
            {
                Playing.UnbindAll();
                _playButtonState = value;
                Playing.BindTo(value.Playing);
                Playing.BindValueChanged(playingStateChanged, true);
            }
        }

        public BeatmapSetInfo BeatmapSet
        {
            get { return playButton.BeatmapSet; }
            set
            {
                if (playButton.BeatmapSet == value) return;
                playButton.BeatmapSet = value;
                playButtonState = previewTrackManager.GetPlayButtonState(playButton.BeatmapSet);
            }
        }

        private void playingStateChanged(bool newValue) => progress.FadeTo(newValue ? 1 : 0, 100);

        public PreviewButton()
        {
            Height = 42;

            Children = new Drawable[]
            {
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
                playButton = new PlayButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(18),
                },
            };

            Action = () => playButton.Click();

            Playing = new BindableBool();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, PreviewTrackManager previewTrackManager)
        {
            progress.Colour = colours.Yellow;

            this.previewTrackManager = previewTrackManager;
            if (BeatmapSet != null)
            {
                playButtonState = previewTrackManager.GetPlayButtonState(playButton.BeatmapSet);
            }
        }

        protected override void Update()
        {
            base.Update();

            if (playButtonState != null && Playing.Value && playButtonState.Preview != null)
            {
                // prevent negative (potential infinite) width if a track without length was loaded
                progress.Width = playButtonState.Preview.Length > 0 ? (float)(playButtonState.Preview.CurrentTime / playButtonState.Preview.Length) : 0f;
            }
            else
                progress.Width = 0;
        }

        protected override bool OnHover(HoverEvent e)
        {
            bg.FadeColour(Color4.Black.Opacity(0.5f), 100);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            bg.FadeColour(Color4.Black.Opacity(0.25f), 100);
            base.OnHoverLost(e);
        }
    }
}
