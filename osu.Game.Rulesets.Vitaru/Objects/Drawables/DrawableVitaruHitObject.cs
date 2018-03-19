using osu.Game.Rulesets.Objects.Drawables;
using System.ComponentModel;

namespace osu.Game.Rulesets.Vitaru.Objects.Drawables
{
    public class DrawableVitaruHitObject : DrawableHitObject<VitaruHitObject>
    {
        public static float TIME_PREEMPT = 600;
        public static float TIME_FADEIN = 300;
        public static float TIME_FADEOUT = 1200;

        public readonly Framework.Graphics.Containers.Container ParentContainer;

        public DrawableVitaruHitObject(VitaruHitObject hitObject, Framework.Graphics.Containers.Container parent) : base(hitObject)
        {
            ParentContainer = parent;

            if (hitObject.Ar != -1)
            {
                TIME_PREEMPT = hitObject.Ar;
                TIME_FADEOUT = hitObject.Ar * 2;
                TIME_FADEIN = hitObject.Ar / 2;
            }
        }

        protected sealed override void UpdateState(ArmedState state) { }
    }

    public enum ComboResult
    {
        [Description(@"")]
        None,
        [Description(@"Good")]
        Good,
        [Description(@"Amazing")]
        Perfect
    }
}
