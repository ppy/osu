// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
        private void load(AudioManager audio)
        {
            var skins = new SkinManager(LocalStorage, ContextFactory, null, audio);

            metricsSkin = getSkinFromResources(skins, "metrics_skin");
            defaultSkin = getSkinFromResources(skins, "default_skin");
            specialSkin = getSkinFromResources(skins, "special_skin");
        }

        public void SetContents(Func<Drawable> creationFunction)
        {
            Cell(0).Child = new LocalSkinOverrideContainer(null) { RelativeSizeAxes = Axes.Both }.WithChild(creationFunction());
            Cell(1).Child = new LocalSkinOverrideContainer(metricsSkin) { RelativeSizeAxes = Axes.Both }.WithChild(creationFunction());
            Cell(2).Child = new LocalSkinOverrideContainer(defaultSkin) { RelativeSizeAxes = Axes.Both }.WithChild(creationFunction());
            Cell(3).Child = new LocalSkinOverrideContainer(specialSkin) { RelativeSizeAxes = Axes.Both }.WithChild(creationFunction());
        }

        private static Skin getSkinFromResources(SkinManager skins, string name)
        {
            using (var storage = new DllResourceStore("osu.Game.Rulesets.Osu.Tests.dll"))
            {
                var tempName = Path.GetTempFileName();

                File.Delete(tempName);
                Directory.CreateDirectory(tempName);

                var files = storage.GetAvailableResources().Where(f => f.StartsWith($"Resources/{name}"));

                foreach (var file in files)
                    using (var stream = storage.GetStream(file))
                    using (var newFile = File.Create(Path.Combine(tempName, Path.GetFileName(file))))
                        stream.CopyTo(newFile);

                return skins.GetSkin(skins.Import(tempName).Result);
            }
        }
    }
}
