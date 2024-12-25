// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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
    public partial class TestSceneHitSoundTrackAdditionBankToggle : HitSoundTrackTestScene
    {
        protected override HitSoundTrackMode[] GetModes() => [HitSoundTrackMode.Sample, HitSoundTrackMode.NormalBank, HitSoundTrackMode.AdditionBank];

        private HitSoundTrackSamplePointBlueprintContainer getSpecificModeTrack(HitSoundTrackMode mode)
        {
            return this.ChildrenOfType<HitSoundTrackSamplePointBlueprintContainer>().First(c => c.Mode == mode);
        }

        private void addSample(IList<HitSampleInfo> sample, Type blueprintType)
        {
            var samplePointBlueprint = getSpecificModeTrack(HitSoundTrackMode.Sample).Children.First(p => p.GetType() == blueprintType);
            var firstSamplePoint = samplePointBlueprint.Children.First();
            AddStep($"add {firstSamplePoint.Target} sample", () =>
            {
                if (sample.Any(sample => sample.Name == firstSamplePoint.Target))
                    return;

                InputManager.MoveMouseTo(firstSamplePoint);
                InputManager.Click(MouseButton.Left);
            });
        }

        [Test]
        public void TestToggleBank()
        {
            var samplePointBlueprint = getSpecificModeTrack(HitSoundTrackMode.AdditionBank).Children.First(p => p.GetType() == typeof(HitSoundTrackSamplePointBlueprint));
            int row = 0;
            addSample(samplePointBlueprint.HitObject.Samples, typeof(HitSoundTrackSamplePointBlueprint));
            samplePointBlueprint.Children.ForEach(point =>
            {
                row++;
                AddStep($"move to {row.ToOrdinalWords()} sample point", () => InputManager.MoveMouseTo(point));
                AddStep($"click on {row.ToOrdinalWords()} sample point", () => InputManager.Click(MouseButton.Left));
                AddAssert($"checks is {point.Target} addition bank being used", () => SamplePointPiece.GetAdditionBankValue(samplePointBlueprint.HitObject.Samples) == point.Target);
            });
        }

        [Test]
        public void TestToggleExtendableBank()
        {
            var samplePointBlueprint = getSpecificModeTrack(HitSoundTrackMode.AdditionBank).Children.First(p => p.GetType() == typeof(ExtendableHitSoundTrackSamplePointBlueprint));
            int row = 0;
            addSample(samplePointBlueprint.HitObject.Samples, typeof(ExtendableHitSoundTrackSamplePointBlueprint));
            samplePointBlueprint.Children.ForEach(point =>
            {
                row++;
                AddStep($"move to {row.ToOrdinalWords()} extendable sample point", () => InputManager.MoveMouseTo(point));
                AddStep($"click on {row.ToOrdinalWords()} extendable sample point", () => InputManager.Click(MouseButton.Left));
                AddAssert($"checks is {point.Target} addition bank being used", () => SamplePointPiece.GetAdditionBankValue(samplePointBlueprint.HitObject.Samples) == point.Target);
            });
        }

        [Test]
        public void TestToggleNodeBank()
        {
            var samplePointBlueprint = getSpecificModeTrack(HitSoundTrackMode.AdditionBank).Children.First(p => p.GetType() == typeof(NodeHitSoundTrackSamplePointBlueprint));
            int row = 0;
            addSample(samplePointBlueprint.HitObject.Samples, typeof(NodeHitSoundTrackSamplePointBlueprint));
            samplePointBlueprint.Children.ForEach(point =>
            {
                row++;
                AddStep($"move to {row.ToOrdinalWords()} node sample point", () => InputManager.MoveMouseTo(point));
                AddStep($"click on {row.ToOrdinalWords()} node sample point", () => InputManager.Click(MouseButton.Left));
                AddAssert($"checks is {point.Target} addition bank being added", () =>
                {
                    if (samplePointBlueprint.HitObject is IHasRepeats repeats)
                    {
                        return SamplePointPiece.GetAdditionBankValue(repeats.NodeSamples[0]) == point.Target;
                    }
                    return false;
                });
            });
        }
    }
}
