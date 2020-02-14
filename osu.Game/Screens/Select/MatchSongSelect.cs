// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Multi;

namespace osu.Game.Screens.Select
{
    public class MatchSongSelect : SongSelect, IMultiplayerSubScreen
    {
        public Action<PlaylistItem> Selected;

        public string ShortTitle => "歌曲选择";
        public override string Title => ShortTitle.Humanize();

        [Resolved(typeof(Room))]
        protected Bindable<PlaylistItem> CurrentItem { get; private set; }

        public override bool AllowEditing => false;

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        public MatchSongSelect()
        {
            Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING };
        }

        protected override bool OnStart()
        {
            var item = new PlaylistItem
            {
                Beatmap = { Value = Beatmap.Value.BeatmapInfo },
                Ruleset = { Value = Ruleset.Value },
                RulesetID = Ruleset.Value.ID ?? 0
            };

            item.RequiredMods.AddRange(Mods.Value);

            Selected?.Invoke(item);

            if (this.IsCurrentScreen())
                this.Exit();

            return true;
        }

        public override bool OnExiting(IScreen next)
        {
            if (base.OnExiting(next))
                return true;

            if (CurrentItem.Value != null)
            {
                Ruleset.Value = CurrentItem.Value.Ruleset.Value;
                Beatmap.Value = beatmaps.GetWorkingBeatmap(CurrentItem.Value.Beatmap.Value);
                Mods.Value = CurrentItem.Value.RequiredMods?.ToArray() ?? Array.Empty<Mod>();
            }

            return false;
        }
    }
}