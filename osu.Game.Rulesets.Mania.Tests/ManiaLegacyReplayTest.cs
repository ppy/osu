// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Replays;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class ManiaLegacyReplayTest
    {
        [TestCase(ManiaAction.Key1)]
        [TestCase(ManiaAction.Key1, ManiaAction.Key2)]
        [TestCase(ManiaAction.Key5)]
        [TestCase(ManiaAction.Key9)]
        public void TestEncodeDecodeSingleStage(params ManiaAction[] actions)
        {
            var beatmap = new ManiaBeatmap(new StageDefinition(9));

            var frame = new ManiaReplayFrame(0, actions);
            var legacyFrame = frame.ToLegacy(beatmap);

            var decodedFrame = new ManiaReplayFrame();
            decodedFrame.FromLegacy(legacyFrame, beatmap);

            Assert.That(decodedFrame.Actions, Is.EquivalentTo(frame.Actions));
        }

        [TestCase(ManiaAction.Key1)]
        [TestCase(ManiaAction.Key1, ManiaAction.Key2)]
        [TestCase(ManiaAction.Key3)]
        [TestCase(ManiaAction.Key8)]
        [TestCase(ManiaAction.Key3, ManiaAction.Key8)]
        [TestCase(ManiaAction.Key3, ManiaAction.Key6)]
        [TestCase(ManiaAction.Key10)]
        public void TestEncodeDecodeDualStage(params ManiaAction[] actions)
        {
            var beatmap = new ManiaBeatmap(new StageDefinition(5));
            beatmap.Stages.Add(new StageDefinition(5));

            var frame = new ManiaReplayFrame(0, actions);
            var legacyFrame = frame.ToLegacy(beatmap);

            var decodedFrame = new ManiaReplayFrame();
            decodedFrame.FromLegacy(legacyFrame, beatmap);

            Assert.That(decodedFrame.Actions, Is.EquivalentTo(frame.Actions));
        }
    }
}
