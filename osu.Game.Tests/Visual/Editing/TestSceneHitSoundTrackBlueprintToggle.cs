// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Humanizer;
using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit.Audio;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneHitSoundTrackBlueprintToggle : HitSoundTrackTestScene
    {
        protected override HitSoundTrackMode[] GetModes() => [HitSoundTrackMode.Sample, HitSoundTrackMode.NormalBank, HitSoundTrackMode.AdditionBank];

        private HitSoundTrackSamplePointBlueprintContainer getSpecificModeTrack(HitSoundTrackMode mode)
        {
            return this.ChildrenOfType<HitSoundTrackSamplePointBlueprintContainer>().First(c => c.Mode == mode);
        }

        [Test]
        public void TestToggleSamplePoint()
        {
            var samplePointBlueprint = getSpecificModeTrack(HitSoundTrackMode.Sample).Children.First(p => p.GetType() == typeof(HitSoundTrackSamplePointBlueprint));
            int row = 0;
            AddStep("clear samples", samplePointBlueprint.HitObject.Samples.Clear);
            samplePointBlueprint.ChildrenOfType<HitSoundTrackSamplePointToggle>().ForEach(point =>
            {
                row++;
                AddStep($"move to {row.ToOrdinalWords()} sample point", () => InputManager.MoveMouseTo(point));
                AddStep($"click on {row.ToOrdinalWords()} sample point", () => InputManager.Click(MouseButton.Left));
                AddAssert($"checks is {point.Target} being added", () => samplePointBlueprint.HitObject.Samples.Any(sample => sample.Name == point.Target));
            });
        }

        [Test]
        public void TestToggleNodeSamplePoint()
        {
            var samplePointBlueprint = getSpecificModeTrack(HitSoundTrackMode.Sample).Children.Where(p => p.GetType() == typeof(NodeHitSoundTrackSamplePointBlueprint)).ElementAt(1);
            int row = 0;
            samplePointBlueprint.ChildrenOfType<HitSoundTrackSamplePointToggle>().ForEach(point =>
            {
                row++;
                AddStep($"move to {row.ToOrdinalWords()} node sample point", () => InputManager.MoveMouseTo(point));
                AddStep($"click on {row.ToOrdinalWords()} node sample point", () => InputManager.Click(MouseButton.Left));
                AddAssert($"checks is {point.Target} being added", () =>
                {
                    if (samplePointBlueprint.HitObject is IHasRepeats repeats)
                        return repeats.NodeSamples[1].Any(sample => sample.Name == point.Target);

                    return false;
                });
            });
        }

        [Test]
        public void TestToggleBank()
        {
            var samplePointBlueprint = getSpecificModeTrack(HitSoundTrackMode.NormalBank).Children.First(p => p.GetType() == typeof(HitSoundTrackSamplePointBlueprint));
            int row = 0;
            samplePointBlueprint.ChildrenOfType<HitSoundTrackSamplePointToggle>().ForEach(point =>
            {
                row++;
                AddStep($"move to {row.ToOrdinalWords()} sample point", () => InputManager.MoveMouseTo(point));
                AddStep($"click on {row.ToOrdinalWords()} sample point", () => InputManager.Click(MouseButton.Left));
                AddAssert($"checks is {point.Target} bank being used", () => SamplePointPiece.GetBankValue(samplePointBlueprint.HitObject.Samples) == point.Target);
            });
        }

        [Test]
        public void TestToggleNodeBank()
        {
            var samplePointBlueprint = getSpecificModeTrack(HitSoundTrackMode.NormalBank).Children.Where(p => p.GetType() == typeof(NodeHitSoundTrackSamplePointBlueprint)).ElementAt(1);
            int row = 0;
            samplePointBlueprint.ChildrenOfType<HitSoundTrackSamplePointToggle>().ForEach(point =>
            {
                row++;
                AddStep($"move to {row.ToOrdinalWords()} node sample point", () => InputManager.MoveMouseTo(point));
                AddStep($"click on {row.ToOrdinalWords()} node sample point", () => InputManager.Click(MouseButton.Left));
                AddAssert($"checks is {point.Target} bank being added", () =>
                {
                    if (samplePointBlueprint.HitObject is IHasRepeats repeats)
                        return SamplePointPiece.GetBankValue(repeats.NodeSamples[1]) == point.Target;

                    return false;
                });
            });
        }

        [Test]
        public void TestToggleAdditionBank()
        {
            var samplePointBlueprint = getSpecificModeTrack(HitSoundTrackMode.AdditionBank).Children.First(p => p.GetType() == typeof(HitSoundTrackSamplePointBlueprint));
            AddStep("add an sample", () => samplePointBlueprint.HitObject.Samples.Add(new HitSampleInfo(HitSampleInfo.HIT_WHISTLE, editorAutoBank: true)));
            int row = 0;
            samplePointBlueprint.ChildrenOfType<HitSoundTrackSamplePointToggle>().ForEach(point =>
            {
                row++;
                AddStep($"move to {row.ToOrdinalWords()} sample point", () => InputManager.MoveMouseTo(point));
                AddStep($"click on {row.ToOrdinalWords()} sample point", () => InputManager.Click(MouseButton.Left));
                AddAssert($"checks is {point.Target} addition bank being used", () => SamplePointPiece.GetAdditionBankValue(samplePointBlueprint.HitObject.Samples) == point.Target);
            });
        }

        [Test]
        public void TestToggleNodeAdditionBank()
        {
            var samplePointBlueprint = getSpecificModeTrack(HitSoundTrackMode.AdditionBank).Children.First(p => p.GetType() == typeof(NodeHitSoundTrackSamplePointBlueprint));
            int row = 0;
            samplePointBlueprint.ChildrenOfType<HitSoundTrackSamplePointToggle>().ForEach(point =>
            {
                row++;
                AddStep($"move to {row.ToOrdinalWords()} node sample point", () => InputManager.MoveMouseTo(point));
                AddStep($"click on {row.ToOrdinalWords()} node sample point", () => InputManager.Click(MouseButton.Left));
                AddAssert($"checks is {point.Target} addition bank being added", () =>
                {
                    if (samplePointBlueprint.HitObject is IHasRepeats repeats)
                        return SamplePointPiece.GetAdditionBankValue(repeats.NodeSamples[0]) == point.Target;

                    return false;
                });
            });
        }
    }
}
