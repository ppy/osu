// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Screens.Edit.Audio;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneHitSoundTrackVolumeControl : HitSoundTrackTestScene
    {
        protected override HitSoundTrackMode[] GetModes() => [HitSoundTrackMode.Volume];

        private HitSoundTrackSamplePointBlueprintContainer getSpecificModeTrack(HitSoundTrackMode mode)
        {
            return this.ChildrenOfType<HitSoundTrackSamplePointBlueprintContainer>().First(c => c.Mode == mode);
        }

        private void addRequireTestSteps(IList<HitSampleInfo> samples, Drawable target)
        {
            AddStep("click on volume of 50", () =>
            {
                InputManager.MoveMouseTo(target);
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("checks is volume being set", () => samples.All(sample => Precision.AlmostEquals(sample.Volume, 50, 2)));

            AddStep("click on volume of 100", () =>
            {
                Vector2 topCentre = new Vector2(target.ScreenSpaceDrawQuad.Centre.X, target.ScreenSpaceDrawQuad.Centre.Y - (target.ScreenSpaceDrawQuad.Height / 2) + 1);
                InputManager.MoveMouseTo(topCentre);
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("checks is volume being set", () => samples.All(sample => Precision.AlmostEquals(sample.Volume, 100, 2)));

            AddStep("click on volume of 0", () =>
            {
                Vector2 bottomCentre = new Vector2(target.ScreenSpaceDrawQuad.Centre.X, target.ScreenSpaceDrawQuad.Centre.Y + (target.ScreenSpaceDrawQuad.Height / 2) - 1);
                InputManager.MoveMouseTo(bottomCentre);
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("checks is volume being set", () => samples.All(sample => Precision.AlmostEquals(sample.Volume, 0, 2)));

            AddStep("press on 100", () =>
            {
                Vector2 topCentre = new Vector2(target.ScreenSpaceDrawQuad.Centre.X, target.ScreenSpaceDrawQuad.Centre.Y - (target.ScreenSpaceDrawQuad.Height / 2) + 1);
                InputManager.PressButton(MouseButton.Left);
                InputManager.MoveMouseTo(topCentre);
            });
            AddAssert("checks is volume being unchanged", () => samples.All(sample => Precision.AlmostEquals(sample.Volume, 0, 2)));

            AddStep("release on 50", () =>
            {
                InputManager.MoveMouseTo(target);
                InputManager.ReleaseButton(MouseButton.Left);
            });
            AddAssert("checks is volume being set", () => samples.All(sample => Precision.AlmostEquals(sample.Volume, 50, 2)));

            AddStep("scroll up", () =>
            {
                InputManager.MoveMouseTo(target);
                InputManager.ScrollVerticalBy(1);
            });
            AddAssert("checks is volume being set", () => samples.All(sample => Precision.AlmostEquals(sample.Volume, 55, 2)));

            AddStep("scroll up with shift pressed", () =>
            {
                InputManager.MoveMouseTo(target);
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.ScrollVerticalBy(1);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });
            AddAssert("checks is volume being set", () => samples.All(sample => Precision.AlmostEquals(sample.Volume, 65, 2)));

            AddStep("scroll down", () =>
            {
                InputManager.MoveMouseTo(target);
                InputManager.ScrollVerticalBy(-1);
            });
            AddAssert("checks is volume being set", () => samples.All(sample => Precision.AlmostEquals(sample.Volume, 60, 2)));

            AddStep("scroll down with shift pressed", () =>
            {
                InputManager.MoveMouseTo(target);
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.ScrollVerticalBy(-1);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });
            AddAssert("checks is volume being set", () => samples.All(sample => Precision.AlmostEquals(sample.Volume, 50, 2)));
        }

        [Test]
        public void TestSamplePointVolumeControl()
        {
            var samplePointBlueprint = getSpecificModeTrack(HitSoundTrackMode.Volume).Children.First(p => p.GetType() == typeof(HitSoundTrackSamplePointVolumeControlBlueprint));
            addRequireTestSteps(samplePointBlueprint.HitObject.Samples, samplePointBlueprint);
        }

        [Test]
        public void TestNodeSamplePointVolumeControl()
        {
            var samplePointBlueprint = (NodeHitSoundTrackSamplePointVolumeControlBlueprint)getSpecificModeTrack(HitSoundTrackMode.Volume).Children.First(p => p.GetType() == typeof(NodeHitSoundTrackSamplePointVolumeControlBlueprint));
            addRequireTestSteps(samplePointBlueprint.HasRepeat.NodeSamples[0], samplePointBlueprint);
        }
    }
}
