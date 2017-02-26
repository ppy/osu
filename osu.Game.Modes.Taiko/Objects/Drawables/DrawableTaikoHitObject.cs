using osu.Game.Modes.Objects.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Modes.Objects;
using osu.Framework.Graphics.Transformations;
using OpenTK;
using osu.Framework.Graphics;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public class DrawableTaikoHitObject : DrawableHitObject
    {
        public const float TIME_PREEMPT = 600;

        public DrawableTaikoHitObject(TaikoHitObject hitObject)
            : base(hitObject)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            RelativePositionAxes = Axes.X;
        }

        public override JudgementInfo CreateJudgementInfo() => new TaikoJudgementInfo { MaxScore = TaikoScoreResult.Great };

        /// <summary>
        /// Todo: Remove
        /// </summary>
        protected override void LoadComplete()
        {
            if (Judgement == null)
                Judgement = CreateJudgementInfo();

            UpdateState(State);
        }

        protected override void UpdateState(ArmedState state)
        {
            if (!IsLoaded)
                return;

            Flush();

            UpdateInitialState();

            Delay(HitObject.StartTime - Time.Current - TIME_PREEMPT, true);

            UpdatePreemptState();

            Delay(TIME_PREEMPT, true);
        }

        protected virtual void UpdateInitialState()
        {
            MoveToX(1.0f);
        }

        protected virtual void UpdatePreemptState()
        {
            MoveConstantSpeed(new Vector2(-1f, 0), 100000);
        }

        public void MoveConstantSpeed(Vector2 speed, double duration)
        {
            UpdateTransformsOfType(typeof(TransformMoveConstantSpeedPosition));
            TransformVectorTo(Position, new Vector2(), duration, EasingTypes.None, new TransformMoveConstantSpeedPosition() { Speed = speed });
        }

        class TransformMoveConstantSpeedPosition : TransformVector
        {
            /// <summary>
            /// Speed in <see cref="Position"/> units per second.
            /// </summary>
            public Vector2 Speed;

            /// <summary>
            /// Value of the offset.
            /// </summary>
            protected override Vector2 CurrentValue
            {
                get
                {
                    double time = Time?.Current ?? 0;

                    if (time < StartTime)
                        return Vector2.Zero;

                    double seconds = (Math.Min(EndTime, time) - StartTime) / 1000;
                    return Speed * (float)seconds;
                }
            }

            public override void Apply(Drawable d)
            {
                base.Apply(d);
                d.Position = StartValue + CurrentValue;
            }
        }
    }
}
