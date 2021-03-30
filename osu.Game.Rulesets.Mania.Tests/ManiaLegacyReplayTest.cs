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
        [TestCase(ManiaAction.Special1)]
        [TestCase(ManiaAction.Key8)]
        public void TestEncodeDecodeSingleStage(params ManiaAction[] actions)
        {
            var beatmap = new ManiaBeatmap(new StageDefinition { Columns = 9 });

            var frame = new ManiaReplayFrame(0, actions);
            var legacyFrame = frame.ToLegacy(beatmap);

            var decodedFrame = new ManiaReplayFrame();
            decodedFrame.FromLegacy(legacyFrame, beatmap);

            Assert.That(decodedFrame.Actions, Is.EquivalentTo(frame.Actions));
        }

        [TestCase(ManiaAction.Key1)]
        [TestCase(ManiaAction.Key1, ManiaAction.Key2)]
        [TestCase(ManiaAction.Special1)]
        [TestCase(ManiaAction.Special2)]
        [TestCase(ManiaAction.Special1, ManiaAction.Special2)]
        [TestCase(ManiaAction.Special1, ManiaAction.Key5)]
        [TestCase(ManiaAction.Key8)]
        public void TestEncodeDecodeDualStage(params ManiaAction[] actions)
        {
            var beatmap = new ManiaBeatmap(new StageDefinition { Columns = 5 });
            beatmap.Stages.Add(new StageDefinition { Columns = 5 });

            var frame = new ManiaReplayFrame(0, actions);
            var legacyFrame = frame.ToLegacy(beatmap);

            var decodedFrame = new ManiaReplayFrame();
            decodedFrame.FromLegacy(legacyFrame, beatmap);

            Assert.That(decodedFrame.Actions, Is.EquivalentTo(frame.Actions));
        }
    }
}
