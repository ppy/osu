// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Screens.Edit.Audio;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneHitSoundTrackBlueprintGeneration : HitSoundTrackTestScene
    {
        protected override HitSoundTrackMode[] GetModes() => [HitSoundTrackMode.Sample, HitSoundTrackMode.NormalBank, HitSoundTrackMode.AdditionBank, HitSoundTrackMode.Volume];

        [Test]
        public void TestBlueprintGeneration()
        {
            SetHitCircles(1);
            AddUntilStep("3 sample point blueprint generated", () => this.ChildrenOfType<HitSoundTrackSamplePointBlueprint>().Count(blueprint => blueprint.GetType() == typeof(HitSoundTrackSamplePointBlueprint)) == 3);
            AddUntilStep("no node sample point blueprint generated", () => !this.ChildrenOfType<NodeHitSoundTrackSamplePointBlueprint>().Any());
            AddUntilStep("1 sample point volume blueprint generated", () => this.ChildrenOfType<HitSoundTrackSamplePointVolumeControlBlueprint>().Count(blueprint => blueprint.GetType() == typeof(HitSoundTrackSamplePointVolumeControlBlueprint)) == 1);
            AddUntilStep("no node sample point volume blueprint generated", () => !this.ChildrenOfType<NodeHitSoundTrackSamplePointVolumeControlBlueprint>().Any());

            SetHitCircles(10);
            AddUntilStep("30 sample point blueprint generated", () => this.ChildrenOfType<HitSoundTrackSamplePointBlueprint>().Count(blueprint => blueprint.GetType() == typeof(HitSoundTrackSamplePointBlueprint)) == 30);
            AddUntilStep("no node sample point blueprint generated", () => !this.ChildrenOfType<NodeHitSoundTrackSamplePointBlueprint>().Any());
            AddUntilStep("10 sample point volume blueprint generated", () => this.ChildrenOfType<HitSoundTrackSamplePointVolumeControlBlueprint>().Count(blueprint => blueprint.GetType() == typeof(HitSoundTrackSamplePointVolumeControlBlueprint)) == 10);
            AddUntilStep("no node sample point volume blueprint generated", () => !this.ChildrenOfType<NodeHitSoundTrackSamplePointVolumeControlBlueprint>().Any());

            SetSliders(1);
            AddUntilStep("3 sample point blueprint generated", () => this.ChildrenOfType<HitSoundTrackSamplePointBlueprint>().Count(blueprint => blueprint.GetType() == typeof(HitSoundTrackSamplePointBlueprint)) == 3);
            AddUntilStep("6 node sample point blueprint generated", () => this.ChildrenOfType<NodeHitSoundTrackSamplePointBlueprint>().Count() == 6);
            AddUntilStep("1 sample point volume blueprint generated", () => this.ChildrenOfType<HitSoundTrackSamplePointVolumeControlBlueprint>().Count(blueprint => blueprint.GetType() == typeof(HitSoundTrackSamplePointVolumeControlBlueprint)) == 1);
            AddUntilStep("2 node sample point volume blueprint generated", () => this.ChildrenOfType<NodeHitSoundTrackSamplePointVolumeControlBlueprint>().Count() == 2);

            SetSliders(10);
            AddUntilStep("30 sample point blueprint generated", () => this.ChildrenOfType<HitSoundTrackSamplePointBlueprint>().Count(blueprint => blueprint.GetType() == typeof(HitSoundTrackSamplePointBlueprint)) == 30);
            AddUntilStep("60 node sample point blueprint generated", () => this.ChildrenOfType<NodeHitSoundTrackSamplePointBlueprint>().Count() == 60);
            AddUntilStep("10 sample point volume blueprint generated", () => this.ChildrenOfType<HitSoundTrackSamplePointVolumeControlBlueprint>().Count(blueprint => blueprint.GetType() == typeof(HitSoundTrackSamplePointVolumeControlBlueprint)) == 10);
            AddUntilStep("20 node sample point volume blueprint generated", () => this.ChildrenOfType<NodeHitSoundTrackSamplePointVolumeControlBlueprint>().Count() == 20);

            SetSliders(1, 4);
            AddUntilStep("3 sample point blueprint generated", () => this.ChildrenOfType<HitSoundTrackSamplePointBlueprint>().Count(blueprint => blueprint.GetType() == typeof(HitSoundTrackSamplePointBlueprint)) == 3);
            AddUntilStep("12 node sample point blueprint generated", () => this.ChildrenOfType<NodeHitSoundTrackSamplePointBlueprint>().Count() == 12);
            AddUntilStep("1 sample point volume blueprint generated", () => this.ChildrenOfType<HitSoundTrackSamplePointVolumeControlBlueprint>().Count(blueprint => blueprint.GetType() == typeof(HitSoundTrackSamplePointVolumeControlBlueprint)) == 1);
            AddUntilStep("4 node sample point volume blueprint generated", () => this.ChildrenOfType<NodeHitSoundTrackSamplePointVolumeControlBlueprint>().Count() == 4);

            SetSliders(10, 4);
            AddUntilStep("30 sample point blueprint generated", () => this.ChildrenOfType<HitSoundTrackSamplePointBlueprint>().Count(blueprint => blueprint.GetType() == typeof(HitSoundTrackSamplePointBlueprint)) == 30);
            AddUntilStep("120 node sample point blueprint generated", () => this.ChildrenOfType<NodeHitSoundTrackSamplePointBlueprint>().Count() == 120);
            AddUntilStep("10 sample point volume blueprint generated", () => this.ChildrenOfType<HitSoundTrackSamplePointVolumeControlBlueprint>().Count(blueprint => blueprint.GetType() == typeof(HitSoundTrackSamplePointVolumeControlBlueprint)) == 10);
            AddUntilStep("40 node sample point volume blueprint generated", () => this.ChildrenOfType<NodeHitSoundTrackSamplePointVolumeControlBlueprint>().Count() == 40);

            SetSpinners(1);
            AddUntilStep("3 sample point blueprint generated", () => this.ChildrenOfType<HitSoundTrackSamplePointBlueprint>().Count(blueprint => blueprint.GetType() == typeof(HitSoundTrackSamplePointBlueprint)) == 3);
            AddUntilStep("no node sample point blueprint generated", () => !this.ChildrenOfType<NodeHitSoundTrackSamplePointBlueprint>().Any());
            AddUntilStep("1 sample point volume blueprint generated", () => this.ChildrenOfType<HitSoundTrackSamplePointVolumeControlBlueprint>().Count(blueprint => blueprint.GetType() == typeof(HitSoundTrackSamplePointVolumeControlBlueprint)) == 1);
            AddUntilStep("no node sample point volume blueprint generated", () => !this.ChildrenOfType<NodeHitSoundTrackSamplePointVolumeControlBlueprint>().Any());

            SetSpinners(10);
            AddUntilStep("30 sample point blueprint generated", () => this.ChildrenOfType<HitSoundTrackSamplePointBlueprint>().Count(blueprint => blueprint.GetType() == typeof(HitSoundTrackSamplePointBlueprint)) == 30);
            AddUntilStep("no node sample point blueprint generated", () => !this.ChildrenOfType<NodeHitSoundTrackSamplePointBlueprint>().Any());
            AddUntilStep("10 sample point volume blueprint generated", () => this.ChildrenOfType<HitSoundTrackSamplePointVolumeControlBlueprint>().Count(blueprint => blueprint.GetType() == typeof(HitSoundTrackSamplePointVolumeControlBlueprint)) == 10);
            AddUntilStep("no node sample point volume blueprint generated", () => !this.ChildrenOfType<NodeHitSoundTrackSamplePointVolumeControlBlueprint>().Any());
        }
    }
}
