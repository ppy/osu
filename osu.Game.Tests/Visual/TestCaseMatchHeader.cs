// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Multi.Match.Components;

namespace osu.Game.Tests.Visual
{
    public class TestCaseMatchHeader : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Header)
        };

        private readonly Bindable<BeatmapInfo> beatmap = new Bindable<BeatmapInfo>();
        private readonly Bindable<GameType> type = new Bindable<GameType>();
        private readonly Bindable<IEnumerable<Mod>> mods = new Bindable<IEnumerable<Mod>>();

        public TestCaseMatchHeader()
        {
            var header = new Header(new Room());

            header.Beatmap.BindTo(beatmap);
            header.Type.BindTo(type);
            header.Mods.BindTo(mods);

            beatmap.Value = new BeatmapInfo
            {
                Metadata = new BeatmapMetadata
                {
                    Title = "Title",
                    Artist = "Artist",
                    AuthorString = "Author",
                },
                Version = "Version",
                Ruleset = new OsuRuleset().RulesetInfo
            };

            type.Value = new GameTypeTimeshift();
            mods.Value = new Mod[]
            {
                new OsuModDoubleTime(),
                new OsuModNoFail(),
                new OsuModRelax(),
            };

            Child = header;
        }
    }
}
