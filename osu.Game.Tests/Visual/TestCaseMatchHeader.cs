// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.GameTypes;
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

        public TestCaseMatchHeader()
        {
            var room = new Room();

            var header = new Header(room);

            room.Playlist.Add(new PlaylistItem
            {
                Beatmap = new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata
                    {
                        Title = "Title",
                        Artist = "Artist",
                        AuthorString = "Author",
                    },
                    Version = "Version",
                    Ruleset = new OsuRuleset().RulesetInfo
                },
                RequiredMods =
                {
                    new OsuModDoubleTime(),
                    new OsuModNoFail(),
                    new OsuModRelax(),
                }
            });

            room.Type.Value = new GameTypeTimeshift();

            Child = header;
        }
    }
}
