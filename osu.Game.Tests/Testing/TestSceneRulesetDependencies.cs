// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Configuration.Tracking;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Testing
{
    /// <summary>
    /// A test scene ensuring the dependencies for the
    /// provided ruleset below are cached at the base implementation.
    /// </summary>
    [HeadlessTest]
    public partial class TestSceneRulesetDependencies : OsuTestScene
    {
        protected override Ruleset CreateRuleset() => new TestRuleset();

        [Test]
        public void TestRetrieveTexture()
        {
            AddAssert("ruleset texture retrieved", () =>
                Dependencies.Get<TextureStore>().Get(@"test-image") != null);
        }

        [Test]
        public void TestRetrieveSample()
        {
            AddAssert("ruleset sample retrieved", () =>
                Dependencies.Get<ISampleStore>().Get(@"test-sample") != null);
        }

        [Test]
        public void TestRetrieveShader()
        {
            AddStep("ruleset shaders retrieved without error", () =>
            {
                Dependencies.Get<ShaderManager>().GetRawData(@"sh_TestVertex.vs");
                Dependencies.Get<ShaderManager>().GetRawData(@"sh_TestFragment.fs");
                Dependencies.Get<ShaderManager>().Load(@"TestVertex", @"TestFragment");
                Dependencies.Get<ShaderManager>().Load(VertexShaderDescriptor.TEXTURE_2, @"TestFragment");
            });
        }

        [Test]
        public void TestResolveConfigManager()
        {
            AddAssert("ruleset config resolved", () =>
                Dependencies.Get<TestRulesetConfigManager>() != null);
        }

        public class TestRuleset : Ruleset
        {
            public override string Description => string.Empty;
            public override string ShortName => string.Empty;

            public TestRuleset()
            {
                // temporary ID to let RulesetConfigCache pass our
                // config manager to the ruleset dependencies.
                RulesetInfo.OnlineID = -1;
            }

            public override IResourceStore<byte[]> CreateResourceStore() => new NamespacedResourceStore<byte[]>(TestResources.GetStore(), @"Resources");
            public override IRulesetConfigManager CreateConfig(SettingsStore? settings) => new TestRulesetConfigManager();

            public override IEnumerable<Mod> GetModsFor(ModType type) => Array.Empty<Mod>();
            public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null) => null!;
            public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => null!;
            public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => null!;
        }

        private class TestRulesetConfigManager : IRulesetConfigManager
        {
            public void Load()
            {
            }

            public bool Save() => true;

            public TrackedSettings CreateTrackedSettings() => new TrackedSettings();

            public void LoadInto(TrackedSettings settings)
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
