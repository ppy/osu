// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Configuration;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneSongSelectSwitchRuleset : SongSelectTestScene
    {
        private void changeBeatmapFromRuleset(int rulesetId)
        {
            var beatmapInfo = Beatmaps.GetAllUsableBeatmapSets().SelectMany(s => s.Beatmaps).First(b => b.Ruleset.OnlineID == rulesetId);
            Beatmap.Value = Beatmaps.GetWorkingBeatmap(beatmapInfo);
        }

        [Test]
        public void TestSwitchRuleset()
        {
            ImportBeatmapForRuleset(0);
            ImportBeatmapForRuleset(1);
            LoadSongSelect();

            AddStep("disable converts", () => Config.SetValue(OsuSetting.ShowConvertedBeatmaps, false));
            AddStep("change beatmap to taiko beatmap", () => changeBeatmapFromRuleset(1));
            WaitForFiltering();
            AddAssert("current ruleset is taiko", () => Ruleset.Value.OnlineID, () => Is.EqualTo(1));

            AddStep("change beatmap to osu! beatmap", () => changeBeatmapFromRuleset(0));
            WaitForFiltering();
            AddAssert("current ruleset is osu!", () => Ruleset.Value.OnlineID, () => Is.EqualTo(0));

            AddStep("enable converts", () => Config.SetValue(OsuSetting.ShowConvertedBeatmaps, true));
            WaitForFiltering();
            AddStep("change beatmap to taiko beatmap", () => changeBeatmapFromRuleset(1));

            AddAssert("current ruleset is taiko", () => Ruleset.Value.OnlineID, () => Is.EqualTo(1));
            WaitForFiltering();
            AddStep("change beatmap to osu! beatmap", () => changeBeatmapFromRuleset(0));
            WaitForFiltering();
            AddAssert("current ruleset is taiko (converted)", () => Ruleset.Value.OnlineID, () => Is.EqualTo(1));
        }

        [Test]
        public void TestNotSwitchRulesetIfBeatmapSetHasCurrentRulesetBeatmaps()
        {
            ImportBeatmapForRuleset(0, 1);
            ImportBeatmapForRuleset(2);
            LoadSongSelect();

            ChangeRuleset(1);
            AddStep("change beatmap to osu! beatmap", () => changeBeatmapFromRuleset(0));
            WaitForFiltering();
            AddAssert("current ruleset is taiko", () => Ruleset.Value.OnlineID, () => Is.EqualTo(1));

            AddStep("change beatmap to catch beatmap", () => changeBeatmapFromRuleset(2));
            WaitForFiltering();
            AddAssert("current ruleset is catch", () => Ruleset.Value.OnlineID, () => Is.EqualTo(2));
        }
    }
}
