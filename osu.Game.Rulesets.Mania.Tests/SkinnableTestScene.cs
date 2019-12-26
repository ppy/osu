// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    public abstract class SkinnableTestScene : OsuGridTestScene
    {
        private Skin defaultSkin;

        protected SkinnableTestScene()
            : base(1, 2)
        {
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, SkinManager skinManager)
        {
            defaultSkin = skinManager.GetSkin(DefaultLegacySkin.Info);
        }

        public void SetContents(Func<Drawable> creationFunction)
        {
            Cell(0).Child = createProvider(null, creationFunction);
            Cell(1).Child = createProvider(defaultSkin, creationFunction);
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
    }
}
