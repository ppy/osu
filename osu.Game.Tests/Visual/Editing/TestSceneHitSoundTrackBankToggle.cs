// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Humanizer;
using NUnit.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Testing;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit.Audio;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneHitSoundTrackBankToggle : HitSoundTrackTestScene
    {
        protected override HitSoundTrackMode[] GetModes() => [HitSoundTrackMode.Sample, HitSoundTrackMode.NormalBank, HitSoundTrackMode.AdditionBank];

        private HitSoundTrackSamplePointBlueprintContainer getSpecificModeTrack(HitSoundTrackMode mode)
        {
            return this.ChildrenOfType<HitSoundTrackSamplePointBlueprintContainer>().First(c => c.Mode == mode);
        }

        [Test]
        public void TestToggleBank()
        {
            var samplePointBlueprint = getSpecificModeTrack(HitSoundTrackMode.NormalBank).Children.First(p => p.GetType() == typeof(HitSoundTrackSamplePointBlueprint));
            int row = 0;
            samplePointBlueprint.Children.ForEach(point =>
            {
                row++;
                AddStep($"move to {row.ToOrdinalWords()} sample point", () => InputManager.MoveMouseTo(point));
                AddStep($"click on {row.ToOrdinalWords()} sample point", () => InputManager.Click(MouseButton.Left));
                AddAssert($"checks is {point.Target} bank being used", () => SamplePointPiece.GetBankValue(samplePointBlueprint.HitObject.Samples) == point.Target);
            });
        }

        [Test]
        public void TestToggleExtendableBank()
        {
            var samplePointBlueprint = getSpecificModeTrack(HitSoundTrackMode.NormalBank).Children.First(p => p.GetType() == typeof(ExtendableHitSoundTrackSamplePointBlueprint));
            int row = 0;
            samplePointBlueprint.Children.ForEach(point =>
            {
                row++;
                AddStep($"move to {row.ToOrdinalWords()} extendable sample point", () => InputManager.MoveMouseTo(point));
                AddStep($"click on {row.ToOrdinalWords()} extendable sample point", () => InputManager.Click(MouseButton.Left));
                AddAssert($"checks is {point.Target} bank being used", () => SamplePointPiece.GetBankValue(samplePointBlueprint.HitObject.Samples) == point.Target);
            });
        }

        [Test]
        public void TestToggleNodeBank()
        {
            var samplePointBlueprint = getSpecificModeTrack(HitSoundTrackMode.NormalBank).Children.Where(p => p.GetType() == typeof(NodeHitSoundTrackSamplePointBlueprint)).ElementAt(1);
            int row = 0;
            samplePointBlueprint.Children.ForEach(point =>
            {
                row++;
                AddStep($"move to {row.ToOrdinalWords()} node sample point", () => InputManager.MoveMouseTo(point));
                AddStep($"click on {row.ToOrdinalWords()} node sample point", () => InputManager.Click(MouseButton.Left));
                AddAssert($"checks is {point.Target} bank being added", () =>
                {
                    if (samplePointBlueprint.HitObject is IHasRepeats repeats)
                    {
                        return SamplePointPiece.GetBankValue(repeats.NodeSamples[1]) == point.Target;
                    }
                    return false;
                });
            });
        }
    }
}
