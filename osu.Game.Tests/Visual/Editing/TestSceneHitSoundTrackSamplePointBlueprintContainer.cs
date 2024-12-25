// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using osu.Framework.Testing;
using osu.Game.Screens.Edit.Audio;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneHitSoundTrackSamplePointGeneration : HitSoundTrackTestScene
    {
        protected override HitSoundTrackMode[] GetModes() => [HitSoundTrackMode.Sample];

        [Test]
        public void TestSamplePointGeneration()
        {
            SetHitCircles(1);
            AddUntilStep("1 sample point blueprint generated", () => this.ChildrenOfType<HitSoundTrackSamplePointBlueprint>().Count() == 1);

            SetHitCircles(10);
            AddUntilStep("10 sample point blueprint generated", () => this.ChildrenOfType<HitSoundTrackSamplePointBlueprint>().Count() == 10);

            SetSpinners(10);
            AddUntilStep("no sample point blueprint generated", () => !this.ChildrenOfType<HitSoundTrackSamplePointBlueprint>().Any(p => p.GetType() == typeof(HitSoundTrackSamplePointBlueprint)));

            SetSliders(10);
            AddUntilStep("no sample point blueprint generated", () => !this.ChildrenOfType<HitSoundTrackSamplePointBlueprint>().Any(p => p.GetType() == typeof(HitSoundTrackSamplePointBlueprint)));

            SetSliders(10, 4);
            AddUntilStep("no sample point blueprint generated", () => !this.ChildrenOfType<HitSoundTrackSamplePointBlueprint>().Any(p => p.GetType() == typeof(HitSoundTrackSamplePointBlueprint)));
        }

        [Test]
        public void TestNodeSamplePointGeneration()
        {
            SetSliders(1);
            AddUntilStep("2 node sample point blueprint generated", () => this.ChildrenOfType<NodeHitSoundTrackSamplePointBlueprint>().Count() == 2);

            SetSliders(10);
            AddUntilStep("20 node sample point blueprint generated", () => this.ChildrenOfType<NodeHitSoundTrackSamplePointBlueprint>().Count() == 20);

            SetSliders(1, 4);
            AddUntilStep("4 node sample point blueprint generated", () => this.ChildrenOfType<NodeHitSoundTrackSamplePointBlueprint>().Count() == 4);

            SetSliders(10, 4);
            AddUntilStep("40 node sample point blueprint generated", () => this.ChildrenOfType<NodeHitSoundTrackSamplePointBlueprint>().Count() == 40);

            SetHitCircles(10);
            AddUntilStep("no node sample point blueprint generated", () => !this.ChildrenOfType<NodeHitSoundTrackSamplePointBlueprint>().Any());

            SetSpinners(10);
            AddUntilStep("no node sample point blueprint generated", () => !this.ChildrenOfType<NodeHitSoundTrackSamplePointBlueprint>().Any());
        }

        [Test]
        public void TestExtendableSamplePointGeneration()
        {
            SetSliders(1);
            AddUntilStep("1 extendable sample point blueprint generated", () => this.ChildrenOfType<ExtendableHitSoundTrackSamplePointBlueprint>().Count() == 1);

            SetSliders(10);
            AddUntilStep("10 extendable sample point blueprint generated", () => this.ChildrenOfType<ExtendableHitSoundTrackSamplePointBlueprint>().Count() == 10);

            SetSliders(1, 4);
            AddUntilStep("1 extendable sample point blueprint generated", () => this.ChildrenOfType<ExtendableHitSoundTrackSamplePointBlueprint>().Count() == 1);

            SetSliders(10, 4);
            AddUntilStep("10 extendable sample point blueprint generated", () => this.ChildrenOfType<ExtendableHitSoundTrackSamplePointBlueprint>().Count() == 10);

            SetSpinners(1);
            AddUntilStep("1 extendable sample point blueprint generated", () => this.ChildrenOfType<ExtendableHitSoundTrackSamplePointBlueprint>().Count() == 1);

            SetSpinners(10);
            AddUntilStep("10 extendable sample point blueprint generated", () => this.ChildrenOfType<ExtendableHitSoundTrackSamplePointBlueprint>().Count() == 10);

            SetHitCircles(1);
            AddUntilStep("no extendable sample point blueprint generated", () => !this.ChildrenOfType<ExtendableHitSoundTrackSamplePointBlueprint>().Any());

            SetHitCircles(10);
            AddUntilStep("no extendable sample point blueprint generated", () => !this.ChildrenOfType<ExtendableHitSoundTrackSamplePointBlueprint>().Any());
        }
    }
}
