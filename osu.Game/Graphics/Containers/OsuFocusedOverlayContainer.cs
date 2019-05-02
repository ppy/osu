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
    public class OsuFocusedOverlayContainer : FocusedOverlayContainer, IPreviewTrackOwner, IKeyBindingHandler<GlobalAction>
    {
        private SampleChannel samplePopIn;
        private SampleChannel samplePopOut;

        protected virtual bool PlaySamplesOnStateChange => true;

        protected override bool BlockNonPositionalInput => true;

        /// <summary>
        /// Temporary to allow for overlays in the main screen content to not dim theirselves.
        /// Should be eventually replaced by dimming which is aware of the target dim container (traverse parent for certain interface type?).
        /// </summary>
        protected virtual bool DimMainContent => true;

        [Resolved(CanBeNull = true)]
        private OsuGame osuGame { get; set; }

        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; }

        protected readonly Bindable<OverlayActivation> OverlayActivationMode = new Bindable<OverlayActivation>(OverlayActivation.All);

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.CacheAs<IPreviewTrackOwner>(this);
            return dependencies;
        }

        [BackgroundDependencyLoader(true)]
        private void load(AudioManager audio)
        {
            if (osuGame != null)
                OverlayActivationMode.BindTo(osuGame.OverlayActivationMode);

            samplePopIn = audio.Sample.Get(@"UI/overlay-pop-in");
            samplePopOut = audio.Sample.Get(@"UI/overlay-pop-out");

            StateChanged += onStateChanged;
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
            {
                State = Visibility.Hidden;
                return true;
            }

            return base.OnClick(e);
        }

        public virtual bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.Back:
                    State = Visibility.Hidden;
                    return true;
                case GlobalAction.Select:
                    return true;
            }

            return false;
        }

        public bool OnReleased(GlobalAction action) => false;

        private void onStateChanged(Visibility visibility)
        {
            switch (visibility)
            {
                case Visibility.Visible:
                    if (OverlayActivationMode.Value != OverlayActivation.Disabled)
                    {
                        if (PlaySamplesOnStateChange) samplePopIn?.Play();
                        if (BlockScreenWideMouse && DimMainContent) osuGame?.AddBlockingOverlay(this);
                    }
                    else
                        State = Visibility.Hidden;

                    break;
                case Visibility.Hidden:
                    if (PlaySamplesOnStateChange) samplePopOut?.Play();
                    if (BlockScreenWideMouse) osuGame?.RemoveBlockingOverlay(this);
                    break;
            }
        }

        protected override void PopOut()
        {
            base.PopOut();
            previewTrackManager.StopAnyPlaying(this);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            osuGame?.RemoveBlockingOverlay(this);
        }
    }
}
