// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Tests.Visual.UserInterface;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneNightcoreBeatContainer : TestSceneBeatSyncedContainer
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);

            Beatmap.Value.Track.Start();
            Beatmap.Value.Track.Seek(Beatmap.Value.Beatmap.HitObjects.First().StartTime - 1000);

            Add(new ModNightcore<HitObject>.NightcoreBeatContainer());

            AddStep("change signature to quadruple", () => Beatmap.Value.Beatmap.ControlPointInfo.TimingPoints.ForEach(p => p.TimeSignature = TimeSignature.SimpleQuadruple));
            AddStep("change signature to triple", () => Beatmap.Value.Beatmap.ControlPointInfo.TimingPoints.ForEach(p => p.TimeSignature = TimeSignature.SimpleTriple));
        }
    }
}
