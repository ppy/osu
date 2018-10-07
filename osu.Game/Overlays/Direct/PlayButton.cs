// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Direct
{
    public class PlayButton : Container
    {
        private BeatmapSetInfo beatmapSet;

        public BeatmapSetInfo BeatmapSet
        {
            get { return beatmapSet; }
            set
            {
                if (value == beatmapSet) return;
                beatmapSet = value;
                if (previewTrackManager != null)
                    getPlayButtonState();
            }
        }

        private Color4 hoverColour;
        private readonly SpriteIcon icon;
        private readonly LoadingAnimation loadingAnimation;

        private const float transition_duration = 500;

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
                loadingAnimation = new LoadingAnimation
                {
                    Size = new Vector2(15),
                },
            });
        }

        private PreviewTrackManager previewTrackManager;
        private PlayButtonState playButtonState;

        [BackgroundDependencyLoader]
        private void load(OsuColour colour, PreviewTrackManager previewTrackManager)
        {
            this.previewTrackManager = previewTrackManager;
            if (beatmapSet != null)
                getPlayButtonState();
            hoverColour = colour.Yellow;
        }

        private void getPlayButtonState()
        {
            if (playButtonState != null)
                unsubscribeFromEvents();
            playButtonState = previewTrackManager.GetPlayButtonState(beatmapSet);
            subscribeToEvents();
        }

        private void subscribeToEvents()
        {
            playButtonState.Loading.ValueChanged += loadingStateChanged;
            playButtonState.Playing.ValueChanged += playingStateChanged;

            playButtonState.Loading.TriggerChange();
            playButtonState.Playing.TriggerChange();
        }

        private void unsubscribeFromEvents()
        {
            playButtonState.Loading.ValueChanged -= loadingStateChanged;
            playButtonState.Playing.ValueChanged -= playingStateChanged;
        }

        protected override bool OnClick(ClickEvent e)
        {
            playButtonState.Playing.Toggle();
            return true;
        }

        protected override bool OnHover(HoverEvent e)
        {
            icon.FadeColour(hoverColour, 120, Easing.InOutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (!playButtonState.Playing.Value)
                icon.FadeColour(Color4.White, 120, Easing.InOutQuint);
            base.OnHoverLost(e);
        }

        private void loadingStateChanged(bool loading)
        {
            if (loading)
            {
                icon.FadeTo(0.5f, transition_duration, Easing.OutQuint);
                loadingAnimation.Show();
            }
            else
            {
                icon.FadeTo(1, transition_duration, Easing.OutQuint);
                loadingAnimation.Hide();
            }
        }

        private void playingStateChanged(bool playing)
        {
            icon.Icon = playing ? FontAwesome.fa_stop : FontAwesome.fa_play;
            icon.FadeColour(playing || IsHovered ? hoverColour : Color4.White, 120, Easing.InOutQuint);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (playButtonState != null)
            {
                unsubscribeFromEvents();
                playButtonState = null;
            }
        }
    }
}
