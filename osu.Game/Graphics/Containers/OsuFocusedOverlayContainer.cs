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
        private Sample samplePopIn;
        private Sample samplePopOut;
        protected virtual string PopInSampleName => "UI/overlay-pop-in";
        protected virtual string PopOutSampleName => "UI/overlay-pop-out";

        protected override bool BlockScrollInput => false;

        protected override bool BlockNonPositionalInput => true;

        /// <summary>
        /// Temporary to allow for overlays in the main screen content to not dim themselves.
        /// Should be eventually replaced by dimming which is aware of the target dim container (traverse parent for certain interface type?).
        /// </summary>
        protected virtual bool DimMainContent => true;

        [Resolved(CanBeNull = true)]
        private OsuGame game { get; set; }

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; }

        protected readonly IBindable<OverlayActivation> OverlayActivationMode = new Bindable<OverlayActivation>(OverlayActivation.All);

        [BackgroundDependencyLoader(true)]
        private void load(AudioManager audio)
        {
            samplePopIn = audio.Samples.Get(PopInSampleName);
            samplePopOut = audio.Samples.Get(PopOutSampleName);
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

        private bool closeOnMouseUp;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            closeOnMouseUp = !base.ReceivePositionalInputAt(e.ScreenSpaceMousePosition);

            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (closeOnMouseUp && !base.ReceivePositionalInputAt(e.ScreenSpaceMousePosition))
                Hide();

            base.OnMouseUp(e);
        }

        public virtual bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.Back:
                    Hide();
                    return true;

                case GlobalAction.Select:
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        protected override void UpdateState(ValueChangedEvent<Visibility> state)
        {
            bool didChange = state.NewValue != state.OldValue;

            switch (state.NewValue)
            {
                case Visibility.Visible:
                    if (OverlayActivationMode.Value == OverlayActivation.Disabled)
                    {
                        // todo: visual/audible feedback that this operation could not complete.
                        State.Value = Visibility.Hidden;
                        return;
                    }

                    if (didChange)
                        samplePopIn?.Play();

                    if (BlockScreenWideMouse && DimMainContent) game?.AddBlockingOverlay(this);
                    break;

                case Visibility.Hidden:
                    if (didChange)
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
