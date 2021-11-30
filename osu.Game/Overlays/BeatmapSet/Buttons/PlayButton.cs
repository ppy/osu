// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapSet.Buttons
{
    public class PlayButton : Container
    {
        public IBindable<bool> Playing => playing;

        private readonly BindableBool playing = new BindableBool();

        public PreviewTrack Preview { get; private set; }

        private APIBeatmapSet beatmapSet;

        public APIBeatmapSet BeatmapSet
        {
            get => beatmapSet;
            set
            {
                if (value == beatmapSet) return;

                beatmapSet = value;

                Preview?.Stop();
                Preview?.Expire();
                Preview = null;

                playing.Value = false;
            }
        }

        private Color4 hoverColour;
        private readonly SpriteIcon icon;
        private readonly LoadingSpinner loadingSpinner;

        private const float transition_duration = 500;

        private bool loading
        {
            set
            {
                if (value)
                {
                    icon.FadeTo(0.5f, transition_duration, Easing.OutQuint);
                    loadingSpinner.Show();
                }
                else
                {
                    icon.FadeTo(1, transition_duration, Easing.OutQuint);
                    loadingSpinner.Hide();
                }
            }
        }

        public PlayButton(APIBeatmapSet setInfo = null)
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
                    Icon = FontAwesome.Solid.Play,
                },
                loadingSpinner = new LoadingSpinner
                {
                    Size = new Vector2(15),
                },
            });

            playing.ValueChanged += playingStateChanged;
        }

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            hoverColour = colour.Yellow;
        }

        protected override bool OnClick(ClickEvent e)
        {
            playing.Toggle();
            return true;
        }

        protected override bool OnHover(HoverEvent e)
        {
            icon.FadeColour(hoverColour, 120, Easing.InOutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (!playing.Value)
                icon.FadeColour(Color4.White, 120, Easing.InOutQuint);
            base.OnHoverLost(e);
        }

        private void playingStateChanged(ValueChangedEvent<bool> e)
        {
            icon.Icon = e.NewValue ? FontAwesome.Solid.Stop : FontAwesome.Solid.Play;
            icon.FadeColour(e.NewValue || IsHovered ? hoverColour : Color4.White, 120, Easing.InOutQuint);

            if (e.NewValue)
            {
                if (BeatmapSet == null)
                {
                    playing.Value = false;
                    return;
                }

                if (Preview != null)
                {
                    attemptStart();
                    return;
                }

                loading = true;

                LoadComponentAsync(Preview = previewTrackManager.Get(beatmapSet), preview =>
                {
                    // Make sure that we schedule to after the next audio frame to fix crashes in single-threaded execution.
                    // See: https://github.com/ppy/osu-framework/issues/4692
                    Schedule(() =>
                    {
                        // beatmapset may have changed.
                        if (Preview != preview)
                            return;

                        AddInternal(preview);
                        loading = false;
                        // make sure that the update of value of Playing (and the ensuing value change callbacks)
                        // are marshaled back to the update thread.
                        preview.Stopped += () => Schedule(() => playing.Value = false);

                        // user may have changed their mind.
                        if (playing.Value)
                            attemptStart();
                    });
                });
            }
            else
            {
                Preview?.Stop();
                loading = false;
            }
        }

        private void attemptStart()
        {
            if (Preview?.Start() != true)
                playing.Value = false;
        }
    }
}
