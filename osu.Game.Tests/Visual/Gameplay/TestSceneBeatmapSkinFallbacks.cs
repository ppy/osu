// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Lists;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Database;
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
    public partial class TestSceneBeatmapSkinFallbacks : OsuPlayerTestScene
    {
        private ISkin currentBeatmapSkin = null!;

        [Resolved]
        private SkinManager skinManager { get; set; } = null!;

        protected override bool HasCustomSteps => true;

        [Test]
        public void TestEmptyLegacyBeatmapSkinFallsBack()
        {
            CreateSkinTest(TrianglesSkin.CreateInfo(), () => new LegacyBeatmapSkin(new BeatmapInfo(), null));
            AddUntilStep("wait for hud load", () => Player.ChildrenOfType<SkinComponentsContainer>().All(c => c.ComponentsLoaded));
            AddAssert("hud from default skin", () => AssertComponentsFromExpectedSource(SkinComponentsContainerLookup.TargetArea.MainHUDComponents, skinManager.CurrentSkin.Value));
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

        protected bool AssertComponentsFromExpectedSource(SkinComponentsContainerLookup.TargetArea target, ISkin expectedSource)
        {
            var targetContainer = Player.ChildrenOfType<SkinComponentsContainer>().First(s => s.Lookup.Target == target);
            var actualComponentsContainer = targetContainer.ChildrenOfType<Container>().SingleOrDefault(c => c.Parent == targetContainer);

            if (actualComponentsContainer == null)
                return false;

            var actualInfo = actualComponentsContainer.CreateSerialisedInfo();

            var expectedComponentsContainer = expectedSource.GetDrawableComponent(new SkinComponentsContainerLookup(target)) as Container;
            if (expectedComponentsContainer == null)
                return false;

            var expectedComponentsAdjustmentContainer = new DependencyProvidingContainer
            {
                Position = actualComponentsContainer.Parent!.ToSpaceOfOtherDrawable(actualComponentsContainer.DrawPosition, Content),
                Size = actualComponentsContainer.DrawSize,
                Child = expectedComponentsContainer,
                // proxy the same required dependencies that `actualComponentsContainer` is using.
                CachedDependencies = new (Type, object)[]
                {
                    (typeof(ScoreProcessor), actualComponentsContainer.Dependencies.Get<ScoreProcessor>()),
                    (typeof(HealthProcessor), actualComponentsContainer.Dependencies.Get<HealthProcessor>()),
                    (typeof(GameplayState), actualComponentsContainer.Dependencies.Get<GameplayState>()),
                    (typeof(IGameplayClock), actualComponentsContainer.Dependencies.Get<IGameplayClock>()),
                    (typeof(InputCountController), actualComponentsContainer.Dependencies.Get<InputCountController>())
                },
            };

            Add(expectedComponentsAdjustmentContainer);
            expectedComponentsAdjustmentContainer.UpdateSubTree();
            var expectedInfo = expectedComponentsContainer.CreateSerialisedInfo();
            Remove(expectedComponentsAdjustmentContainer, true);

            return almostEqual(actualInfo, expectedInfo);
        }

        private static bool almostEqual(SerialisedDrawableInfo drawableInfo, SerialisedDrawableInfo? other) =>
            other != null
            && drawableInfo.Type == other.Type
            && drawableInfo.Anchor == other.Anchor
            && drawableInfo.Origin == other.Origin
            && Precision.AlmostEquals(drawableInfo.Position, other.Position, 1)
            && Precision.AlmostEquals(drawableInfo.Scale, other.Scale)
            && Precision.AlmostEquals(drawableInfo.Rotation, other.Rotation)
            && drawableInfo.Children.SequenceEqual(other.Children, new FuncEqualityComparer<SerialisedDrawableInfo>(almostEqual));

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard? storyboard = null)
            => new CustomSkinWorkingBeatmap(beatmap, storyboard, Clock, Audio, currentBeatmapSkin);

        protected override Ruleset CreatePlayerRuleset() => new TestOsuRuleset();

        private class CustomSkinWorkingBeatmap : ClockBackedTestWorkingBeatmap
        {
            private readonly ISkin beatmapSkin;

            public CustomSkinWorkingBeatmap(IBeatmap beatmap, Storyboard? storyboard, IFrameBasedClock referenceClock, AudioManager audio, ISkin beatmapSkin)
                : base(beatmap, storyboard, referenceClock, audio)
            {
                this.beatmapSkin = beatmapSkin;
            }

            protected internal override ISkin GetSkin() => beatmapSkin;
        }

        private class TestOsuRuleset : OsuRuleset
        {
            public override ISkin CreateSkinTransformer(ISkin skin, IBeatmap beatmap) => new TestOsuLegacySkinTransformer(skin);

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
