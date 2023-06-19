// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Audio.Sample;
using osu.Game.Audio;
using osu.Game.Rulesets.Taiko.Audio;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Skinning.Default
{
    public class TaikoTrianglesSkinTransformer : SkinTransformer
    {
        public TaikoTrianglesSkinTransformer(ISkin skin)
            : base(skin)
        {
        }

        public override ISample? GetSample(ISampleInfo sampleInfo)
        {
            if (sampleInfo is TaikoHitSampleInfo taikoSample)
            {
                // Triangles skin doesn't have taiko samples, so immediately fall back to non-prefixed samples.
                return base.GetSample(new TaikoHitSampleInfo(taikoSample.Name, taikoSample.Bank, taikoSample.Suffix, taikoSample.Volume, false));
            }

            return base.GetSample(sampleInfo);
        }
    }
}
