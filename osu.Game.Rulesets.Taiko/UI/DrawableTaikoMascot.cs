using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;

namespace osu.Game.Rulesets.Taiko.UI
{
    public sealed class DrawableTaikoMascot : BeatSyncedContainer
    {
        private static TaikoMascotTextureAnimation idleDrawable, clearDrawable, kiaiDrawable, failDrawable;
        private EffectControlPoint lastEffectControlPoint;
        private TaikoMascotAnimationState state;

        public Bindable<TaikoMascotAnimationState> PlayfieldState;

        /// <summary>
        /// Determines if there should be no "state logic", intended for testing.
        /// </summary>
        public bool Dumb { get; set; }

        public TaikoMascotAnimationState State
        {
            get => state;
            set
            {
                state = value;

                foreach (var child in InternalChildren)
                    child.Hide();

                var drawable = getStateDrawable(State);

                drawable?.Show();
            }
        }

        public DrawableTaikoMascot(TaikoMascotAnimationState startingState = TaikoMascotAnimationState.Idle)
        {
            RelativeSizeAxes = Axes.Both;
            PlayfieldState = new Bindable<TaikoMascotAnimationState>();
            PlayfieldState.BindValueChanged((b) =>
            {
                if (lastEffectControlPoint != null)
                    State = getFinalAnimationState(lastEffectControlPoint, b.NewValue);
            });

            State = startingState;
        }

        private TaikoMascotTextureAnimation getStateDrawable(TaikoMascotAnimationState state) => state switch
        {
            TaikoMascotAnimationState.Idle  => idleDrawable,
            TaikoMascotAnimationState.Clear => clearDrawable,
            TaikoMascotAnimationState.Kiai  => kiaiDrawable,
            TaikoMascotAnimationState.Fail  => failDrawable,
            _                               => null
        };

        private TaikoMascotAnimationState getFinalAnimationState(EffectControlPoint effectPoint, TaikoMascotAnimationState playfieldState)
        {
            if (playfieldState == TaikoMascotAnimationState.Fail)
                return playfieldState;

            return effectPoint.KiaiMode ? TaikoMascotAnimationState.Kiai : TaikoMascotAnimationState.Idle;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            InternalChildren = new[]
            {
                idleDrawable = new TaikoMascotTextureAnimation(TaikoMascotAnimationState.Idle),
                clearDrawable = new TaikoMascotTextureAnimation(TaikoMascotAnimationState.Clear),
                kiaiDrawable = new TaikoMascotTextureAnimation(TaikoMascotAnimationState.Kiai),
                failDrawable = new TaikoMascotTextureAnimation(TaikoMascotAnimationState.Fail),
            };

            // making sure we have the correct sprite set
            State = state;
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            if (!Dumb)
                State = getFinalAnimationState(lastEffectControlPoint = effectPoint, PlayfieldState.Value);

            if (State == TaikoMascotAnimationState.Clear)
                return;

            var drawable = getStateDrawable(State);
            drawable.Move();
        }
    }
}
