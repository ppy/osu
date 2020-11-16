// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Tests.Visual.UserInterface;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneMetronomeBeatContainer : TestSceneBeatSyncedContainer
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);

            Beatmap.Value.Track.Start();
            Beatmap.Value.Track.Seek(Beatmap.Value.Beatmap.HitObjects.First().StartTime - 1000);

            var tickFrequency = new Bindable<ModMetronome.TickFrequency>(ModMetronome.TickFrequency.One);
            var highlightFirstBeat = new Bindable<bool>(true);
            Add(new ModMetronome<HitObject>.MetronomeBeatContainer(tickFrequency, highlightFirstBeat));

            AddStep("change tick frequency to One", () => tickFrequency.Value = ModMetronome.TickFrequency.One);
            AddStep("change tick frequency to Two", () => tickFrequency.Value = ModMetronome.TickFrequency.Two);
            AddStep("change tick frequency to Four", () => tickFrequency.Value = ModMetronome.TickFrequency.Four);

            AddStep("disable highlightFirstBeat", () => highlightFirstBeat.Value = false);
            AddStep("enable highlightFirstBeat", () => highlightFirstBeat.Value = true);
        }
    }
}
