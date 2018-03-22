using osu.Game.Rulesets.Objects.Drawables;
using Symcol.Rulesets.Core.HitObjects;
using System.ComponentModel;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Skinning;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Vitaru.Objects.Drawables
{
    public class DrawableVitaruHitObject : DrawableSymcolHitObject<VitaruHitObject>
    {
        public static float TIME_PREEMPT = 600;
        public static float TIME_FADEIN = 300;
        public static float TIME_FADEOUT = 1200;

        public readonly Framework.Graphics.Containers.Container ParentContainer;

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            base.SkinChanged(skin, allowFallback);

            if (HitObject is IHasComboInformation combo && HitObject.ColorOverride == Color4.White)
                AccentColour = skin.GetValue<SkinConfiguration, Color4>(s => s.ComboColours.Count > 0 ? s.ComboColours[combo.ComboIndex % s.ComboColours.Count] : (Color4?)null) ?? Color4.White;
            else
                AccentColour = HitObject.ColorOverride;
        }

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
