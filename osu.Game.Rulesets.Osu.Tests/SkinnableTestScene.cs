// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Text.RegularExpressions;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests
{
    public abstract class SkinnableTestScene : OsuGridTestScene
    {
        private Skin metricsSkin;
        private Skin defaultSkin;
        private Skin specialSkin;

        protected SkinnableTestScene()
            : base(2, 2)
        {
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, SkinManager skinManager)
        {
            var dllStore = new DllResourceStore("osu.Game.Rulesets.Osu.Tests.dll");

            metricsSkin = new TestLegacySkin(new SkinInfo(), new NamespacedResourceStore<byte[]>(dllStore, "Resources/metrics_skin"), audio, true);
            defaultSkin = skinManager.GetSkin(DefaultLegacySkin.Info);
            specialSkin = new TestLegacySkin(new SkinInfo(), new NamespacedResourceStore<byte[]>(dllStore, "Resources/special_skin"), audio, true);
        }

        public void SetContents(Func<Drawable> creationFunction)
        {
            Cell(0).Child = createProvider(null, creationFunction);
            Cell(1).Child = createProvider(metricsSkin, creationFunction);
            Cell(2).Child = createProvider(defaultSkin, creationFunction);
            Cell(3).Child = createProvider(specialSkin, creationFunction);
        }

        private Drawable createProvider(Skin skin, Func<Drawable> creationFunction)
        {
            var mainProvider = new SkinProvidingContainer(skin);

            return mainProvider
                .WithChild(new SkinProvidingContainer(Ruleset.Value.CreateInstance().CreateLegacySkinProvider(mainProvider))
                {
                    Child = creationFunction()
                });
        }

        private class TestLegacySkin : LegacySkin
        {
            private readonly bool extrapolateAnimations;

            public TestLegacySkin(SkinInfo skin, IResourceStore<byte[]> storage, AudioManager audioManager, bool extrapolateAnimations)
                : base(skin, storage, audioManager, "skin.ini")
            {
                this.extrapolateAnimations = extrapolateAnimations;
            }

            public override Texture GetTexture(string componentName)
            {
                // extrapolate frames to test longer animations
                if (extrapolateAnimations)
                {
                    var match = Regex.Match(componentName, "-([0-9]*)");

                    if (match.Length > 0 && int.TryParse(match.Groups[1].Value, out var number) && number < 60)
                        return base.GetTexture(componentName.Replace($"-{number}", $"-{number % 2}"));
                }

                return base.GetTexture(componentName);
            }
        }
    }
}
