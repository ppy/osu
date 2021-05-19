// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Lists;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Skinning.Legacy;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;
using osu.Game.Storyboards;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneBeatmapSkinFallbacks : OsuPlayerTestScene
    {
        private ISkin currentBeatmapSkin;

        [Resolved]
        private SkinManager skinManager { get; set; }

        [Cached]
        private ScoreProcessor scoreProcessor = new ScoreProcessor();

        [Cached(typeof(HealthProcessor))]
        private HealthProcessor healthProcessor = new DrainingHealthProcessor(0);

        protected override bool HasCustomSteps => true;

        [Test]
        public void TestEmptyDefaultBeatmapSkinFallsBack()
        {
            CreateSkinTest(DefaultLegacySkin.Info, () => new TestWorkingBeatmap(CreateBeatmap(new OsuRuleset().RulesetInfo)).Skin);
            AddAssert("hud from default legacy skin", () => AssertComponentsFromExpectedSource(SkinnableTarget.MainHUDComponents, skinManager.CurrentSkin.Value));
        }

        [Test]
        public void TestEmptyLegacyBeatmapSkinFallsBack()
        {
            CreateSkinTest(SkinInfo.Default, () => new LegacyBeatmapSkin(new BeatmapInfo(), null, null));
            AddAssert("hud from default skin", () => AssertComponentsFromExpectedSource(SkinnableTarget.MainHUDComponents, skinManager.CurrentSkin.Value));
        }

        protected void CreateSkinTest(SkinInfo gameCurrentSkin, Func<ISkin> getBeatmapSkin)
        {
            CreateTest(() =>
            {
                AddStep("setup skins", () =>
                {
                    skinManager.CurrentSkinInfo.Value = gameCurrentSkin;
                    currentBeatmapSkin = getBeatmapSkin();
                });
            });
        }

        protected bool AssertComponentsFromExpectedSource(SkinnableTarget target, ISkin expectedSource)
        {
            var expectedComponentsContainer = (SkinnableTargetComponentsContainer)expectedSource.GetDrawableComponent(new SkinnableTargetComponent(target));

            Add(expectedComponentsContainer);
            expectedComponentsContainer?.UpdateSubTree();
            var expectedInfo = expectedComponentsContainer?.CreateSkinnableInfo();
            Remove(expectedComponentsContainer);

            var actualInfo = Player.ChildrenOfType<SkinnableTargetContainer>().First(s => s.Target == target)
                                   .ChildrenOfType<SkinnableTargetComponentsContainer>().Single().CreateSkinnableInfo();

            return almostEqual(actualInfo, expectedInfo, 2f);

            static bool almostEqual(SkinnableInfo info, SkinnableInfo other, float positionTolerance) =>
                other != null
                && info.Anchor == other.Anchor
                && info.Origin == other.Origin
                && Precision.AlmostEquals(info.Position, other.Position, positionTolerance)
                && info.Children.SequenceEqual(other.Children, new FuncEqualityComparer<SkinnableInfo>((s1, s2) => almostEqual(s1, s2, positionTolerance)));
        }

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

            protected override ISkin GetSkin() => beatmapSkin;
        }

        private class TestOsuRuleset : OsuRuleset
        {
            public override ISkin CreateLegacySkinProvider(ISkinSource source, IBeatmap beatmap) => new TestOsuLegacySkinTransformer(source);

            private class TestOsuLegacySkinTransformer : OsuLegacySkinTransformer
            {
                public TestOsuLegacySkinTransformer(ISkinSource source)
                    : base(source)
                {
                }

                public override Drawable GetDrawableComponent(ISkinComponent component)
                {
                    var drawable = base.GetDrawableComponent(component);
                    if (drawable != null)
                        return drawable;

                    // this isn't really supposed to make a difference from returning null,
                    // but it appears it does, returning null will skip over falling back to beatmap skin,
                    // while calling Source.GetDrawableComponent() doesn't.
                    return Source.GetDrawableComponent(component);
                }
            }
        }
    }
}
