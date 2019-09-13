// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osuTK;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;

namespace osu.Game.Graphics.Containers
{
    [Cached(typeof(IPreviewTrackOwner))]
    public abstract class OsuFocusedOverlayContainer : FocusedOverlayContainer, IPreviewTrackOwner, IKeyBindingHandler<GlobalAction>
    {
        private SampleChannel samplePopIn;
        private SampleChannel samplePopOut;

        protected override bool BlockNonPositionalInput => true;

        /// <summary>
        /// Temporary to allow for overlays in the main screen content to not dim theirselves.
        /// Should be eventually replaced by dimming which is aware of the target dim container (traverse parent for certain interface type?).
        /// </summary>
        protected virtual bool DimMainContent => true;

        [Resolved(CanBeNull = true)]
        private OsuGame game { get; set; }

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; }

        protected readonly Bindable<OverlayActivation> OverlayActivationMode = new Bindable<OverlayActivation>(OverlayActivation.All);

        [BackgroundDependencyLoader(true)]
        private void load(AudioManager audio)
        {
            samplePopIn = audio.Samples.Get(@"UI/overlay-pop-in");
            samplePopOut = audio.Samples.Get(@"UI/overlay-pop-out");
        }

        protected override void LoadComplete()
        {
            if (game != null)
                OverlayActivationMode.BindTo(game.OverlayActivationMode);

            OverlayActivationMode.BindValueChanged(mode =>
            {
                if (mode.NewValue == OverlayActivation.Disabled)
                    State.Value = Visibility.Hidden;
            }, true);

            base.LoadComplete();
        }

        /// <summary>
        /// Whether mouse input should be blocked screen-wide while this overlay is visible.
        /// Performing mouse actions outside of the valid extents will hide the overlay.
        /// </summary>
        public virtual bool BlockScreenWideMouse => BlockPositionalInput;

        // receive input outside our bounds so we can trigger a close event on ourselves.
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => BlockScreenWideMouse || base.ReceivePositionalInputAt(screenSpacePos);

        protected override bool OnClick(ClickEvent e)
        {
            if (!base.ReceivePositionalInputAt(e.ScreenSpaceMousePosition))
                Hide();

            return base.OnClick(e);
        }

        private bool closeOnDragEnd;

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (!base.ReceivePositionalInputAt(e.ScreenSpaceMousePosition))
                closeOnDragEnd = true;

            return base.OnDragStart(e);
        }

        protected override bool OnDragEnd(DragEndEvent e)
        {
            if (closeOnDragEnd)
            {
                Hide();
                closeOnDragEnd = false;
            }

            return base.OnDragEnd(e);
        }

        public virtual bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.Back:
                    Hide();
                    return true;

                case GlobalAction.Select:
                    return true;
            }

            return false;
        }

        public bool OnReleased(GlobalAction action) => false;

        protected override void UpdateState(ValueChangedEvent<Visibility> state)
        {
            switch (state.NewValue)
            {
                case Visibility.Visible:
                    if (OverlayActivationMode.Value == OverlayActivation.Disabled)
                    {
                        State.Value = Visibility.Hidden;
                        return;
                    }

                    samplePopIn?.Play();
                    if (BlockScreenWideMouse && DimMainContent) game?.AddBlockingOverlay(this);
                    break;

                case Visibility.Hidden:
                    samplePopOut?.Play();
                    if (BlockScreenWideMouse) game?.RemoveBlockingOverlay(this);
                    break;
            }

            base.UpdateState(state);
        }

        protected override void PopOut()
        {
            base.PopOut();
            previewTrackManager.StopAnyPlaying(this);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            game?.RemoveBlockingOverlay(this);
        }
    }
}
