// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using Humanizer;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi;

namespace osu.Game.Screens.Select
{
    public class MatchSongSelect : SongSelect, IMultiplayerSubScreen
    {
        public Action<PlaylistItem> Selected;

        public string ShortTitle => "song selection";
        public override string Title => ShortTitle.Humanize();

        protected override bool OnStart()
        {
            var item = new PlaylistItem
            {
                Beatmap = Beatmap.Value.BeatmapInfo,
                Ruleset = Ruleset.Value,
                RulesetID = Ruleset.Value.ID ?? 0
            };

            item.RequiredMods.AddRange(SelectedMods.Value);

            Selected?.Invoke(item);

            if (IsCurrentScreen)
                Exit();

            return true;
        }
    }
}
