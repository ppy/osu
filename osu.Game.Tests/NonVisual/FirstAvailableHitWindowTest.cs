// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
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
    public class FirstAvailableHitWindowsTest
    {
        private TestDrawableRuleset testDrawableRuleset = new TestDrawableRuleset();

        [Test]
        public void TestResultIfOnlyParentHitWindowIsEmpty()
        {
            var testObject = new TestHitObject(hitWindows: HitWindows.Empty);
            HitObject nested = new TestHitObject(hitWindows: new HitWindows());
            testObject.AddNested(nested);
            testDrawableRuleset.HitObjects = new List<HitObject> { testObject };

            // If the parent window is empty, but its nested object isn't, return the nested object
            Assert.AreSame(testDrawableRuleset.FirstAvailableHitWindows, nested.HitWindows);
        }

        [Test]
        public void TestResultIfParentHitWindowsIsNotEmpty()
        {
            var testObject = new TestHitObject(hitWindows: new HitWindows());
            HitObject nested = new TestHitObject(hitWindows: new HitWindows());
            testObject.AddNested(nested);
            testDrawableRuleset.HitObjects = new List<HitObject> { testObject };

            // If the parent window is not empty, return that immediately
            Assert.AreSame(testDrawableRuleset.FirstAvailableHitWindows, testObject.HitWindows);
        }

        [Test]
        public void TestResultIfParentAndChildHitWindowsAreEmpty()
        {
            var firstObject = new TestHitObject(hitWindows: HitWindows.Empty);
            HitObject nested = new TestHitObject(hitWindows: HitWindows.Empty);
            firstObject.AddNested(nested);

            var secondObject = new TestHitObject(hitWindows: new HitWindows());
            testDrawableRuleset.HitObjects = new List<HitObject> { firstObject, secondObject };

            // If the parent and child windows are empty, return the next object if window isn't empty
            Assert.AreSame(testDrawableRuleset.FirstAvailableHitWindows, secondObject.HitWindows);
        }

        [Test]
        public void TestResultIfAllHitWindowsAreEmpty()
        {
            var firstObject = new TestHitObject(hitWindows: HitWindows.Empty);
            HitObject nested = new TestHitObject(hitWindows: HitWindows.Empty);
            firstObject.AddNested(nested);

            testDrawableRuleset.HitObjects = new List<HitObject> { firstObject };

            // If all windows are empty, this should return null
            Assert.IsNull(testDrawableRuleset.FirstAvailableHitWindows);
        }

        [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
        private class TestDrawableRuleset : DrawableRuleset
        {
            public List<HitObject> HitObjects;
            public override IEnumerable<HitObject> Objects => HitObjects;

            public override event Action<JudgementResult> NewResult;
            public override event Action<JudgementResult> RevertResult;

            public override Playfield Playfield { get; }
            public override Container Overlays { get; }
            public override Container FrameStableComponents { get; }
            public override IFrameStableClock FrameStableClock { get; }
            internal override bool FrameStablePlayback { get; set; }
            public override IReadOnlyList<Mod> Mods { get; }

            public override double GameplayStartTime { get; }
            public override GameplayCursorContainer Cursor { get; }

            public TestDrawableRuleset()
                : base(new OsuRuleset())
            {
                // won't compile without this.
                NewResult?.Invoke(null);
                RevertResult?.Invoke(null);
            }

            public override void SetReplayScore(Score replayScore) => throw new NotImplementedException();

            public override void SetRecordTarget(Score score) => throw new NotImplementedException();

            public override void RequestResume(Action continueResume) => throw new NotImplementedException();

            public override void CancelResume() => throw new NotImplementedException();
        }
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
