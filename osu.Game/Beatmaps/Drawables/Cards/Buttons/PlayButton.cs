// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Beatmaps.Drawables.Cards.Buttons
{
    public partial class PlayButton : OsuHoverContainer
    {
        public IBindable<double> Progress => progress;
        private readonly BindableDouble progress = new BindableDouble();

        public BindableBool Playing { get; } = new BindableBool();

        private readonly IBeatmapSetInfo beatmapSetInfo;

        protected override IEnumerable<Drawable> EffectTargets => icon.Yield();

        private readonly SpriteIcon icon;
        private readonly LoadingSpinner loadingSpinner;

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; } = null!;

        private PreviewTrack? previewTrack;

        public PlayButton(IBeatmapSetInfo beatmapSetInfo)
        {
            this.beatmapSetInfo = beatmapSetInfo;

            Anchor = Origin = Anchor.Centre;

            // needed for touch input to work when card is not hovered/expanded
            AlwaysPresent = true;

            Children = new Drawable[]
            {
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.Solid.Play,
                    Size = new Vector2(14)
                },
                loadingSpinner = new LoadingSpinner
                {
                    Size = new Vector2(14)
                }
            };

            Action = () => Playing.Toggle();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            HoverColour = colours.Yellow;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Playing.BindValueChanged(updateState, true);
        }

        protected override void Update()
        {
            base.Update();

            if (Playing.Value && previewTrack != null && previewTrack.TrackLoaded)
                progress.Value = previewTrack.CurrentTime / previewTrack.Length;
            else
                progress.Value = 0;
        }

        private void updateState(ValueChangedEvent<bool> playing)
        {
            icon.Icon = playing.NewValue ? FontAwesome.Solid.Stop : FontAwesome.Solid.Play;

            if (!playing.NewValue)
            {
                stopPreview();
                return;
            }

            if (previewTrack == null)
            {
                toggleLoading(true);

                LoadComponentAsync(previewTrack = previewTrackManager.Get(beatmapSetInfo), onPreviewLoaded);
            }
            else
                tryStartPreview();
        }

        private void stopPreview()
        {
            toggleLoading(false);
            Playing.Value = false;
            previewTrack?.Stop();
        }

        private void onPreviewLoaded(PreviewTrack loadedPreview)
        {
            // Make sure that we schedule to after the next audio frame to fix crashes in single-threaded execution.
            // See: https://github.com/ppy/osu-framework/issues/4692
            Schedule(() =>
            {
                // another async load might have completed before this one.
                // if so, do not make any changes.
                if (loadedPreview != previewTrack)
                {
                    loadedPreview.Dispose();
                    return;
                }

                AddInternal(loadedPreview);
                toggleLoading(false);

                loadedPreview.Stopped += () => Schedule(() => Playing.Value = false);

                if (Playing.Value)
                    tryStartPreview();
            });
        }

        private void tryStartPreview()
        {
            if (previewTrack?.Start() == false)
                Playing.Value = false;
        }

        private void toggleLoading(bool loading)
        {
            Enabled.Value = !loading;
            icon.FadeTo(loading ? 0 : 1, BeatmapCard.TRANSITION_DURATION, Easing.OutQuint);
            loadingSpinner.State.Value = loading ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
