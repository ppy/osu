// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Audio;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning
{
    public class ManiaLegacySkinTransformer : ISkin
    {
        private readonly ISkin source;

        public ManiaLegacySkinTransformer(ISkin source)
        {
            this.source = source;
        }

        public Drawable GetDrawableComponent(ISkinComponent component)
        {
            switch (component)
            {
                case GameplaySkinComponent<HitResult> resultComponent:
                    return getResult(resultComponent);
            }

            return null;
        }

        private Drawable getResult(GameplaySkinComponent<HitResult> resultComponent)
        {
            switch (resultComponent.Component)
            {
                case HitResult.Miss:
                    return this.GetAnimation("mania-hit0", true, true);

                case HitResult.Meh:
                    return this.GetAnimation("mania-hit50", true, true);

                case HitResult.Ok:
                    return this.GetAnimation("mania-hit100", true, true);

                case HitResult.Good:
                    return this.GetAnimation("mania-hit200", true, true);

                case HitResult.Great:
                    return this.GetAnimation("mania-hit300", true, true);

                case HitResult.Perfect:
                    return this.GetAnimation("mania-hit300g", true, true);
            }

            return null;
        }

        public Texture GetTexture(string componentName) => source.GetTexture(componentName);

        public SampleChannel GetSample(ISampleInfo sample) => source.GetSample(sample);

        public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) =>
            source.GetConfig<TLookup, TValue>(lookup);
    }
}
