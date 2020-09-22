// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Audio;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Skinning
{
    public class TaikoLegacySkinTransformer : LegacySkinTransformer
    {
        public TaikoLegacySkinTransformer(ISkinSource source)
            : base(source)
        {
        }

        public override Drawable GetDrawableComponent(ISkinComponent component)
        {
            if (!(component is TaikoSkinComponent taikoComponent))
                return null;

            switch (taikoComponent.Component)
            {
                case TaikoSkinComponents.DrumRollBody:
                    if (GetTexture("taiko-roll-middle") != null)
                        return new LegacyDrumRoll();

                    return null;

                case TaikoSkinComponents.InputDrum:
                    if (GetTexture("taiko-bar-left") != null)
                        return new LegacyInputDrum();

                    return null;

                case TaikoSkinComponents.CentreHit:
                case TaikoSkinComponents.RimHit:

                    if (GetTexture("taikohitcircle") != null)
                        return new LegacyHit(taikoComponent.Component);

                    return null;

                case TaikoSkinComponents.DrumRollTick:
                    return this.GetAnimation("sliderscorepoint", false, false);

                case TaikoSkinComponents.HitTarget:
                    if (GetTexture("taikobigcircle") != null)
                        return new TaikoLegacyHitTarget();

                    return null;

                case TaikoSkinComponents.PlayfieldBackgroundRight:
                    if (GetTexture("taiko-bar-right") != null)
                        return new TaikoLegacyPlayfieldBackgroundRight();

                    return null;

                case TaikoSkinComponents.PlayfieldBackgroundLeft:
                    // This is displayed inside LegacyInputDrum. It is required to be there for layout purposes (can be seen on legacy skins).
                    if (GetTexture("taiko-bar-right") != null)
                        return Drawable.Empty();

                    return null;

                case TaikoSkinComponents.BarLine:
                    if (GetTexture("taiko-barline") != null)
                        return new LegacyBarLine();

                    return null;

                case TaikoSkinComponents.TaikoExplosionGood:
                case TaikoSkinComponents.TaikoExplosionGoodStrong:
                case TaikoSkinComponents.TaikoExplosionGreat:
                case TaikoSkinComponents.TaikoExplosionGreatStrong:
                case TaikoSkinComponents.TaikoExplosionMiss:

                    var sprite = this.GetAnimation(getHitName(taikoComponent.Component), true, false);
                    if (sprite != null)
                        return new LegacyHitExplosion(sprite);

                    return null;

                case TaikoSkinComponents.Scroller:
                    if (GetTexture("taiko-slider") != null)
                        return new LegacyTaikoScroller();

                    return null;

                case TaikoSkinComponents.Mascot:
                    return new DrawableTaikoMascot();
            }

            return Source.GetDrawableComponent(component);
        }

        private string getHitName(TaikoSkinComponents component)
        {
            switch (component)
            {
                case TaikoSkinComponents.TaikoExplosionMiss:
                    return "taiko-hit0";

                case TaikoSkinComponents.TaikoExplosionGood:
                    return "taiko-hit100";

                case TaikoSkinComponents.TaikoExplosionGoodStrong:
                    return "taiko-hit100k";

                case TaikoSkinComponents.TaikoExplosionGreat:
                    return "taiko-hit300";

                case TaikoSkinComponents.TaikoExplosionGreatStrong:
                    return "taiko-hit300k";
            }

            throw new ArgumentOutOfRangeException(nameof(component), "Invalid result type");
        }

        public override SampleChannel GetSample(ISampleInfo sampleInfo) => Source.GetSample(new LegacyTaikoSampleInfo(sampleInfo));

        public override IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => Source.GetConfig<TLookup, TValue>(lookup);

        private class LegacyTaikoSampleInfo : ISampleInfo
        {
            private readonly ISampleInfo source;

            public LegacyTaikoSampleInfo(ISampleInfo source)
            {
                this.source = source;
            }

            public IEnumerable<string> LookupNames
            {
                get
                {
                    foreach (var name in source.LookupNames)
                        yield return $"taiko-{name}";

                    foreach (var name in source.LookupNames)
                        yield return name;
                }
            }

            public int Volume => source.Volume;
        }
    }
}
