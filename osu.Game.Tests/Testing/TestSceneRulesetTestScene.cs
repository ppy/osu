// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration.Tracking;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Testing;
using osu.Game.Rulesets.UI;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Testing
{
    public class TestSceneRulesetTestScene : OsuTestScene, IRulesetTestScene
    {
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
        public void TestResolveConfigManager()
        {
            AddAssert("ruleset config resolved", () =>
                Dependencies.Get<TestRulesetConfigManager>() != null);
        }

        public Ruleset CreateRuleset() => new TestRuleset();

        private class TestRuleset : Ruleset
        {
            public override string Description => string.Empty;
            public override string ShortName => string.Empty;

            public TestRuleset()
            {
                // temporary ID to let RulesetConfigCache pass our
                // config manager to the ruleset dependencies.
                RulesetInfo.ID = -1;
            }

            public override IResourceStore<byte[]> CreateResourceStore() => new NamespacedResourceStore<byte[]>(TestResources.GetStore(), @"Resources");
            public override IRulesetConfigManager CreateConfig(SettingsStore settings) => new TestRulesetConfigManager();

            public override IEnumerable<Mod> GetModsFor(ModType type) => throw new NotImplementedException();
            public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null) => throw new NotImplementedException();
            public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => throw new NotImplementedException();
            public override DifficultyCalculator CreateDifficultyCalculator(WorkingBeatmap beatmap) => throw new NotImplementedException();
        }

        private class TestRulesetConfigManager : IRulesetConfigManager
        {
            public void Load() => throw new NotImplementedException();
            public bool Save() => throw new NotImplementedException();
            public TrackedSettings CreateTrackedSettings() => throw new NotImplementedException();
            public void LoadInto(TrackedSettings settings) => throw new NotImplementedException();
            public void Dispose() => throw new NotImplementedException();
        }
    }
}
