// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Screens.Ladder.Components;

namespace osu.Game.Tournament.Tests
{
    public class TestCaseLadderManager : OsuTestCase
    {
        [Cached]
        private readonly LadderManager manager;

        public TestCaseLadderManager()
        {
            var teams = JsonConvert.DeserializeObject<List<TournamentTeam>>(File.ReadAllText(@"teams.json"));
            var ladder = File.Exists(@"bracket.json") ? JsonConvert.DeserializeObject<LadderInfo>(File.ReadAllText(@"bracket.json")) : new LadderInfo();

            Child = manager = new LadderManager(ladder, teams);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            File.WriteAllText(@"bracket.json", JsonConvert.SerializeObject(manager.Info,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                }));
        }
    }
}
