// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapListing.Panels
{
    public class PlayButton : CompositeDrawable
    {
        private const float transition_duration = 500;

        private readonly Button button;

        public readonly BindableBool Enabled = new BindableBool(true);

        public IBindable<bool> Playing => playing;

        private readonly BindableBool playing = new BindableBool();

        public PreviewTrack Preview { get; private set; }

        private BeatmapSetInfo beatmapSet;

        public BeatmapSetInfo BeatmapSet
        {
            get => beatmapSet;
            set
            {
                if (value == beatmapSet) return;

                beatmapSet = value;

                Preview?.Stop();
                Preview?.Expire();
                Preview = null;

                if (IsLoaded)
                    updateEnabledState();

                if (playing.Value)
                    playing.Value = false;
            }
        }

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; }

        public PlayButton(BeatmapSetInfo setInfo = null)
        {
            BeatmapSet = setInfo;

            InternalChild = button = new Button(this)
            {
                RelativeSizeAxes = Axes.Both,
                Action = TogglePlaying,
            };

            playing.ValueChanged += playingStateChanged;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Enabled.BindValueChanged(_ => updateEnabledState(), true);
        }

        private void updateEnabledState()
        {
            var disabled = BeatmapSet == null || !Enabled.Value;

            if (playing.Value && disabled)
                playing.Value = false;

            playing.Disabled = disabled;
        }

        public void TogglePlaying() => playing.Toggle();

        private void playingStateChanged(ValueChangedEvent<bool> e)
        {
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

                button.Loading = true;

                LoadComponentAsync(Preview = previewTrackManager.Get(beatmapSet), preview =>
                {
                    // beatmapset may have changed.
                    if (Preview != preview)
                        return;

                    AddInternal(preview);
                    button.Loading = false;
                    // make sure that the update of value of Playing (and the ensuing value change callbacks)
                    // are marshaled back to the update thread.
                    preview.Stopped += () => Schedule(() => playing.Value = false);

                    // user may have changed their mind.
                    if (playing.Value)
                        attemptStart();
                });
            }
            else
            {
                Preview?.Stop();
                button.Loading = false;
            }
        }

        private void attemptStart()
        {
            if (Preview?.Start() != true)
                playing.Value = false;
        }

        private class Button : OsuClickableContainer
        {
            private readonly SpriteIcon icon;
            private readonly LoadingSpinner loadingSpinner;

            private readonly IBindable<bool> playing;

            public bool Loading
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

            public Button(PlayButton player)
            {
                playing = player.Playing.GetBoundCopy();

                Children = new Drawable[]
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
                };
            }

            private Color4 hoverColour;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                hoverColour = colours.Yellow;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                playing.BindDisabledChanged(disabled => Enabled.Value = !playing.Disabled, true);
                Enabled.BindValueChanged(e => this.FadeTo(e.NewValue ? 1 : 0), true);

                playing.BindValueChanged(e =>
                {
                    icon.Icon = e.NewValue ? FontAwesome.Solid.Stop : FontAwesome.Solid.Play;
                    icon.FadeColour(e.NewValue || IsHovered ? hoverColour : Color4.White, 120, Easing.InOutQuint);
                }, true);
            }

            protected override bool OnHover(HoverEvent e)
            {
                if (!Enabled.Value)
                    return false;

                icon.FadeColour(hoverColour, 120, Easing.InOutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                if (!playing.Value)
                    icon.FadeColour(Color4.White, 120, Easing.InOutQuint);
                base.OnHoverLost(e);
            }
        }
    }
}
