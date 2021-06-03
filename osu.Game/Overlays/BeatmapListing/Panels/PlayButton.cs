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
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;
namespace osu.Game.Overlays.BeatmapListing.Panels
{
    public class PlayButton : CompositeDrawable
    {
        /// <summary>
        /// Whether this button should be usable. If disabled, it will generally be hidden from view.
        /// </summary>
        public readonly BindableBool Disabled = new BindableBool();

        private const float transition_duration = 500;

        private Button button;

        protected IBindable<bool> ShouldDisplay => shouldDisplay;

        private readonly BindableBool shouldDisplay = new BindableBool();

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
                    updateDisabledState();

                if (playing.Value)
                    playing.Value = false;
            }
        }

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; }

        protected virtual Drawable CreateContent() => button = new Button(this)
        {
            RelativeSizeAxes = Axes.Both,
        };

        public PlayButton(BeatmapSetInfo setInfo = null)
        {
            BeatmapSet = setInfo;

            InternalChildren = new[]
            {
                CreateContent(),
                new HoverClickSounds()
            };

            playing.ValueChanged += playingStateChanged;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Disabled.BindValueChanged(_ => updateDisabledState(), true);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (ShouldDisplay.Value)
                return true;

            playing.Toggle();
            return true;
        }

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

        private void updateDisabledState()
        {
            var disabledValue = Disabled.Value || BeatmapSet == null;

            if (playing.Value && disabledValue)
                playing.Value = false;

            shouldDisplay.Value = disabledValue;
        }

        private class Button : CompositeDrawable
        {
            private readonly SpriteIcon icon;
            private readonly LoadingSpinner loadingSpinner;

            private readonly IBindable<bool> playing;
            private readonly IBindable<bool> disabled;

            [Resolved]
            private OsuColour colours { get; set; }

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

            public Button(PlayButton playButton)
            {
                playing = playButton.Playing.GetBoundCopy();
                disabled = playButton.ShouldDisplay.GetBoundCopy();

                InternalChildren = new Drawable[]
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

            protected override void LoadComplete()
            {
                base.LoadComplete();

                playing.BindValueChanged(_ => updateDisplay());
                disabled.BindValueChanged(_ => updateDisplay(), true);
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateDisplay();
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                updateDisplay();
                base.OnHoverLost(e);
            }

            private void updateDisplay()
            {
                icon.Icon = playing.Value ? FontAwesome.Solid.Stop : FontAwesome.Solid.Play;

                if ((IsHovered || playing.Value) && !disabled.Value)
                    icon.FadeColour(colours.Yellow, 120, Easing.InOutQuint);
                else if (!playing.Value || disabled.Value)
                    icon.FadeColour(Color4.White, 120, Easing.InOutQuint);

                this.FadeTo(disabled.Value ? 0 : 1, 120, Easing.InOutQuint);
            }
        }
    }
}
