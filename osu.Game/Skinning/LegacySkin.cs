// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Audio;
using osu.Game.Rulesets.Scoring;
using osuTK.Graphics;

namespace osu.Game.Skinning
{
    public class LegacySkin : Skin
    {
        protected TextureStore Textures;

        protected IResourceStore<SampleChannel> Samples;

        public LegacySkin(SkinInfo skin, IResourceStore<byte[]> storage, AudioManager audioManager)
            : this(skin, new LegacySkinResourceStore<SkinFileInfo>(skin, storage), audioManager, "skin.ini")
        {
            // defaults should only be applied for non-beatmap skins (which are parsed via this constructor).
            if (!Configuration.CustomColours.ContainsKey("SliderBall")) Configuration.CustomColours["SliderBall"] = new Color4(2, 170, 255, 255);
        }

        protected LegacySkin(SkinInfo skin, IResourceStore<byte[]> storage, AudioManager audioManager, string filename)
            : base(skin)
        {
            Stream stream = storage.GetStream(filename);
            if (stream != null)
                using (StreamReader reader = new StreamReader(stream))
                    Configuration = new LegacySkinDecoder().Decode(reader);
            else
                Configuration = new DefaultSkinConfiguration();

            Samples = audioManager.GetSampleStore(storage);
            Textures = new TextureStore(new TextureLoaderStore(storage));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Textures?.Dispose();
            Samples?.Dispose();
        }

        public override Drawable GetDrawableComponent(ISkinComponent component)
        {
            switch (component)
            {
                case GameplaySkinComponent<HitResult> resultComponent:
                    switch (resultComponent.Component)
                    {
                        case HitResult.Miss:
                            return this.GetAnimation("hit0", true, false);

                        case HitResult.Meh:
                            return this.GetAnimation("hit50", true, false);

                        case HitResult.Good:
                            return this.GetAnimation("hit100", true, false);

                        case HitResult.Great:
                            return this.GetAnimation("hit300", true, false);
                    }

                    break;
            }

            return this.GetAnimation(component.LookupName, false, false);
        }

        public override Texture GetTexture(string componentName)
        {
            componentName = getFallbackName(componentName);

            float ratio = 2;
            var texture = Textures.Get($"{componentName}@2x");

            if (texture == null)
            {
                ratio = 1;
                texture = Textures.Get(componentName);
            }

            if (texture != null)
                texture.ScaleAdjust = ratio;

            return texture;
        }

        public override SampleChannel GetSample(ISampleInfo sampleInfo)
        {
            foreach (var lookup in sampleInfo.LookupNames)
            {
                var sample = Samples.Get(getFallbackName(lookup));

                if (sample != null)
                    return sample;
            }

            if (sampleInfo is HitSampleInfo hsi)
                // Try fallback to non-bank samples.
                return Samples.Get(hsi.Name);

            return null;
        }

        private string getFallbackName(string componentName)
        {
            string lastPiece = componentName.Split('/').Last();
            return componentName.StartsWith("Gameplay/taiko/") ? "taiko-" + lastPiece : lastPiece;
        }
    }
}
