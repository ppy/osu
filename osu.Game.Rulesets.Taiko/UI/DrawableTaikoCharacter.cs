using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;

namespace osu.Game.Rulesets.Taiko.UI
{
    public sealed class DrawableTaikoCharacter : BeatSyncedContainer
    {
        private static TaikoDonTextureAnimation idleDrawable, clearDrawable, kiaiDrawable, failDrawable;

        private TaikoDonAnimationState state;

        public DrawableTaikoCharacter()
        {
            RelativeSizeAxes = Axes.Both;
        }

        private TaikoDonTextureAnimation getStateDrawable() => State switch
        {
            TaikoDonAnimationState.Idle  => idleDrawable,
            TaikoDonAnimationState.Clear => clearDrawable,
            TaikoDonAnimationState.Kiai  => kiaiDrawable,
            TaikoDonAnimationState.Fail  => failDrawable,
            _                            => null
        };

        public TaikoDonAnimationState State
        {
            get => state;
            set
            {
                state = value;

                foreach (var child in InternalChildren)
                    child.Hide();

                getStateDrawable().Show();
            }
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            InternalChildren = new[]
            {
                idleDrawable = new TaikoDonTextureAnimation(TaikoDonAnimationState.Idle),
                clearDrawable = new TaikoDonTextureAnimation(TaikoDonAnimationState.Clear),
                kiaiDrawable = new TaikoDonTextureAnimation(TaikoDonAnimationState.Kiai),
                failDrawable = new TaikoDonTextureAnimation(TaikoDonAnimationState.Fail),
            };

            // sets the state, to make sure we have the correct sprite loaded and set.
            State = TaikoDonAnimationState.Idle;
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            getStateDrawable().Move();
        }
    }
}
