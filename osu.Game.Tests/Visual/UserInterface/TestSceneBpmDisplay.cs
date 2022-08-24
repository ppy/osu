// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneBpmDisplay : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        [Test]
        public void TestBpmDisplay()
        {
            BpmDisplay bpmDisplay = null!;

            int getDisplayedBpm() => bpmDisplay.ChildrenOfType<RollingCounter<int>>().Single().Current.Value;

            AddStep("create content", () =>
            {
                Child = bpmDisplay = new BpmDisplay()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                };
            });
            AddUntilStep("display hidden", () => bpmDisplay.Alpha == 0f);
            AddStep("load beatmap", () =>
            {
                var beatmap = CreateBeatmap(new OsuRuleset().RulesetInfo);
                beatmap.ControlPointInfo.Add(0, new TimingControlPoint
                {
                    BeatLength = 500d,
                    TimeSignature = TimeSignature.SimpleQuadruple
                });
                Beatmap.Value = CreateWorkingBeatmap(beatmap);
            });
            AddUntilStep("display shown", () => bpmDisplay.Alpha == 1f);
            AddAssert("bpm is 128", () => getDisplayedBpm() == 128);
            AddStep("select DT", () => SelectedMods.Value = new[] { new OsuModDoubleTime() });
            AddAssert("bpm is 192", () => getDisplayedBpm() == 192); //128*1.5
            AddStep("select HT", () => SelectedMods.Value = new[] { new OsuModHalfTime() });
            AddAssert("bpm is 96", () => getDisplayedBpm() == 96); //128*0.75
        }
    }
}
