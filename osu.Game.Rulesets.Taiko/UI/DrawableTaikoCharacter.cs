using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Rulesets.Taiko.UI
{
    public sealed class DrawableTaikoCharacter : BeatSyncedContainer
    {
        private static DefaultTaikoDonTextureAnimation idleDrawable, clearDrawable, kiaiDrawable, failDrawable;

        private TaikoDonAnimationState state;

        public DrawableTaikoCharacter()
        {
            RelativeSizeAxes = Axes.Both;
            //Size = new Vector2(1f, 2.5f);
            //Origin = Anchor.BottomLeft;
            var xd = new Vector2(1);
        }

        private DefaultTaikoDonTextureAnimation getStateDrawable() => State switch
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
                idleDrawable = new DefaultTaikoDonTextureAnimation(TaikoDonAnimationState.Idle),
                clearDrawable = new DefaultTaikoDonTextureAnimation(TaikoDonAnimationState.Clear),
                kiaiDrawable = new DefaultTaikoDonTextureAnimation(TaikoDonAnimationState.Kiai),
                failDrawable = new DefaultTaikoDonTextureAnimation(TaikoDonAnimationState.Fail),
            };

            // sets the state, to make sure we have the correct sprite loaded and set.
            State = TaikoDonAnimationState.Idle;
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);

            getStateDrawable().Move();

            //var signature = timingPoint.TimeSignature == TimeSignatures.SimpleQuadruple ? 4 : 3;
            //var length = timingPoint.BeatLength;
            //var rate = 1000d / length;
            //adjustableClock.Rate = rate;
            //
            //// Start animating on the first beat.
            //if (beatIndex < 1)
            //    adjustableClock.Start();
            // Logger.GetLogger(LoggingTarget.Information).Add($"Length = {length}ms | Rate = {rate}x | BPM = {timingPoint.BPM} / {signature}");
        }
    }
}
