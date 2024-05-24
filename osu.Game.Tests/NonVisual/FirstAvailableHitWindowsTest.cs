// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using osu.Framework.Audio;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;

namespace osu.Game.Tests.NonVisual
{
    public partial class FirstAvailableHitWindowsTest
    {
        private TestDrawableRuleset testDrawableRuleset;

        [SetUp]
        public void Setup()
        {
            testDrawableRuleset = new TestDrawableRuleset();
        }

        [Test]
        public void TestResultIfOnlyParentHitWindowIsEmpty()
        {
            var testObject = new TestHitObject(HitWindows.Empty);
            HitObject nested = new TestHitObject(new HitWindows());
            testObject.AddNested(nested);
            testDrawableRuleset.HitObjects = new List<HitObject> { testObject };

            Assert.AreSame(testDrawableRuleset.FirstAvailableHitWindows, nested.HitWindows);
        }

        [Test]
        public void TestResultIfParentHitWindowsIsNotEmpty()
        {
            var testObject = new TestHitObject(new HitWindows());
            HitObject nested = new TestHitObject(new HitWindows());
            testObject.AddNested(nested);
            testDrawableRuleset.HitObjects = new List<HitObject> { testObject };

            Assert.AreSame(testDrawableRuleset.FirstAvailableHitWindows, testObject.HitWindows);
        }

        [Test]
        public void TestResultIfParentAndChildHitWindowsAreEmpty()
        {
            var firstObject = new TestHitObject(HitWindows.Empty);
            HitObject nested = new TestHitObject(HitWindows.Empty);
            firstObject.AddNested(nested);

            var secondObject = new TestHitObject(new HitWindows());
            testDrawableRuleset.HitObjects = new List<HitObject> { firstObject, secondObject };

            Assert.AreSame(testDrawableRuleset.FirstAvailableHitWindows, secondObject.HitWindows);
        }

        [Test]
        public void TestResultIfAllHitWindowsAreEmpty()
        {
            var firstObject = new TestHitObject(HitWindows.Empty);
            HitObject nested = new TestHitObject(HitWindows.Empty);
            firstObject.AddNested(nested);

            testDrawableRuleset.HitObjects = new List<HitObject> { firstObject };

            Assert.IsNull(testDrawableRuleset.FirstAvailableHitWindows);
        }

        [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
        private partial class TestDrawableRuleset : DrawableRuleset
        {
            public List<HitObject> HitObjects;
            public override IEnumerable<HitObject> Objects => HitObjects;

            public override event Action<JudgementResult> NewResult
            {
                add => throw new InvalidOperationException($"{nameof(NewResult)} operations not supported in test context");
                remove => throw new InvalidOperationException($"{nameof(NewResult)} operations not supported in test context");
            }

            public override event Action<JudgementResult> RevertResult
            {
                add => throw new InvalidOperationException($"{nameof(RevertResult)} operations not supported in test context");
                remove => throw new InvalidOperationException($"{nameof(RevertResult)} operations not supported in test context");
            }

            public override IAdjustableAudioComponent Audio { get; }
            public override Playfield Playfield { get; }
            public override Container Overlays { get; }
            public override Container FrameStableComponents { get; }
            public override IFrameStableClock FrameStableClock { get; }
            internal override bool FrameStablePlayback { get; set; }
            public override bool AllowBackwardsSeeks { get; set; }
            public override IReadOnlyList<Mod> Mods { get; }

            public override double GameplayStartTime { get; }
            public override GameplayCursorContainer Cursor { get; }

            public TestDrawableRuleset()
                : base(new OsuRuleset())
            {
            }

            public override void SetReplayScore(Score replayScore) => throw new NotImplementedException();

            public override void SetRecordTarget(Score score) => throw new NotImplementedException();

            public override void RequestResume(Action continueResume) => throw new NotImplementedException();

            public override void CancelResume() => throw new NotImplementedException();
        }

        public class TestHitObject : HitObject
        {
            public TestHitObject(HitWindows hitWindows)
            {
                HitWindows = hitWindows;
                HitWindows.SetDifficulty(0.5f);
            }

            public new void AddNested(HitObject nested) => base.AddNested(nested);
        }
    }
}
