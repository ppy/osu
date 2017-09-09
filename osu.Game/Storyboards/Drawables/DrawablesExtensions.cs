using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;

namespace osu.Game.Storyboards.Drawables
{
    public static class DrawablesExtensions
    {
        /// <summary>
        /// Adjusts <see cref="Drawable.BlendingMode"/> after a delay.
        /// </summary>
        /// <returns>A <see cref="TransformSequence{T}"/> to which further transforms can be added.</returns>
        public static TransformSequence<T> TransformBlendingMode<T>(this T drawable, BlendingMode newValue, double delay = 0)
            where T : Drawable
            => drawable.TransformTo(drawable.PopulateTransform(new TransformBlendingMode(), newValue, delay));
    }

    public class TransformBlendingMode : Transform<BlendingMode, Drawable>
    {
        private BlendingMode valueAt(double time)
            => time < EndTime ? StartValue : EndValue;

        public override string TargetMember => nameof(Drawable.BlendingMode);

        protected override void Apply(Drawable d, double time) => d.BlendingMode = valueAt(time);
        protected override void ReadIntoStartValue(Drawable d) => StartValue = d.BlendingMode;
    }
}
