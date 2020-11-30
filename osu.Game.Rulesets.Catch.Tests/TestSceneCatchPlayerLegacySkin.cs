// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.IO.Stores;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestSceneCatchPlayerLegacySkin : PlayerTestScene
    {
        private ISkinSource legacySkinSource;

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new SkinProvidingPlayer(legacySkinSource);

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuGameBase game)
        {
            var legacySkin = new DefaultLegacySkin(new NamespacedResourceStore<byte[]>(game.Resources, "Skins/Legacy"), audio);
            legacySkinSource = new SkinProvidingContainer(legacySkin);
        }

        protected override Ruleset CreatePlayerRuleset() => new CatchRuleset();

        public class SkinProvidingPlayer : TestPlayer
        {
            private readonly ISkinSource skinSource;

            public SkinProvidingPlayer(ISkinSource skinSource)
            {
                this.skinSource = skinSource;
            }

            protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            {
                var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
                dependencies.CacheAs(skinSource);
                return dependencies;
            }
        }
    }
}
