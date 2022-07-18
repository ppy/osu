// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Lists;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Skinning.Legacy;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using osu.Game.Storyboards;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneBeatmapSkinFallbacks : OsuPlayerTestScene
    {
        private ISkin currentBeatmapSkin;

        [Resolved]
        private SkinManager skinManager { get; set; }

        protected override bool HasCustomSteps => true;

        [Test]
        public void TestEmptyLegacyBeatmapSkinFallsBack()
        {
            CreateSkinTest(DefaultSkin.CreateInfo(), () => new LegacyBeatmapSkin(new BeatmapInfo(), null));
            AddUntilStep("wait for hud load", () => Player.ChildrenOfType<SkinnableTargetContainer>().All(c => c.ComponentsLoaded));
            AddAssert("hud from default skin", () => AssertComponentsFromExpectedSource(SkinnableTarget.MainHUDComponents, skinManager.CurrentSkin.Value));
        }

        protected void CreateSkinTest(SkinInfo gameCurrentSkin, Func<ISkin> getBeatmapSkin)
        {
            CreateTest(() =>
            {
                AddStep("setup skins", () =>
                {
                    skinManager.CurrentSkinInfo.Value = gameCurrentSkin.ToLiveUnmanaged();
                    currentBeatmapSkin = getBeatmapSkin();
                });
            });
        }

        protected bool AssertComponentsFromExpectedSource(SkinnableTarget target, ISkin expectedSource)
        {
            var actualComponentsContainer = Player.ChildrenOfType<SkinnableTargetContainer>().First(s => s.Target == target)
                                                  .ChildrenOfType<SkinnableTargetComponentsContainer>().SingleOrDefault();

            if (actualComponentsContainer == null)
                return false;

            var actualInfo = actualComponentsContainer.CreateSkinnableInfo();

            var expectedComponentsContainer = (SkinnableTargetComponentsContainer)expectedSource.GetDrawableComponent(new SkinnableTargetComponent(target));
            if (expectedComponentsContainer == null)
                return false;

            var expectedComponentsAdjustmentContainer = new DependencyProvidingContainer
            {
                Position = actualComponentsContainer.Parent.ToSpaceOfOtherDrawable(actualComponentsContainer.DrawPosition, Content),
                Size = actualComponentsContainer.DrawSize,
                Child = expectedComponentsContainer,
                // proxy the same required dependencies that `actualComponentsContainer` is using.
                CachedDependencies = new (Type, object)[]
                {
                    (typeof(ScoreProcessor), actualComponentsContainer.Dependencies.Get<ScoreProcessor>()),
                    (typeof(HealthProcessor), actualComponentsContainer.Dependencies.Get<HealthProcessor>()),
                    (typeof(GameplayState), actualComponentsContainer.Dependencies.Get<GameplayState>()),
                    (typeof(GameplayClock), actualComponentsContainer.Dependencies.Get<GameplayClock>())
                },
            };

            Add(expectedComponentsAdjustmentContainer);
            expectedComponentsAdjustmentContainer.UpdateSubTree();
            var expectedInfo = expectedComponentsContainer.CreateSkinnableInfo();
            Remove(expectedComponentsAdjustmentContainer);

            return almostEqual(actualInfo, expectedInfo);
        }

        private static bool almostEqual(SkinnableInfo info, SkinnableInfo other) =>
            other != null
            && info.Type == other.Type
            && info.Anchor == other.Anchor
            && info.Origin == other.Origin
            && Precision.AlmostEquals(info.Position, other.Position, 1)
            && Precision.AlmostEquals(info.Scale, other.Scale)
            && Precision.AlmostEquals(info.Rotation, other.Rotation)
            && info.Children.SequenceEqual(other.Children, new FuncEqualityComparer<SkinnableInfo>(almostEqual));

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null)
            => new CustomSkinWorkingBeatmap(beatmap, storyboard, Clock, Audio, currentBeatmapSkin);

        protected override Ruleset CreatePlayerRuleset() => new TestOsuRuleset();

        private class CustomSkinWorkingBeatmap : ClockBackedTestWorkingBeatmap
        {
            private readonly ISkin beatmapSkin;

            public CustomSkinWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard, IFrameBasedClock referenceClock, AudioManager audio, ISkin beatmapSkin)
                : base(beatmap, storyboard, referenceClock, audio)
            {
                this.beatmapSkin = beatmapSkin;
            }

            protected internal override ISkin GetSkin() => beatmapSkin;
        }

        private class TestOsuRuleset : OsuRuleset
        {
            public override ISkin CreateLegacySkinProvider(ISkin skin, IBeatmap beatmap) => new TestOsuLegacySkinTransformer(skin);

            private class TestOsuLegacySkinTransformer : OsuLegacySkinTransformer
            {
                public TestOsuLegacySkinTransformer(ISkin skin)
                    : base(skin)
                {
                }
            }
        }
    }
}
