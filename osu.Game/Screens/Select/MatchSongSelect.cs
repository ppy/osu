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
using osu.Game.Screens.Multi.Components;

namespace osu.Game.Screens.Select
{
    public class MatchSongSelect : SongSelect, IMultiplayerSubScreen
    {
        public Action<PlaylistItem> Selected;

        public string ShortTitle => "song selection";
        public override string Title => ShortTitle.Humanize();

        public override bool AllowEditing => false;

        [Resolved(typeof(Room), nameof(Room.Playlist))]
        protected BindableList<PlaylistItem> Playlist { get; private set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        public MatchSongSelect()
        {
            Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING };
        }

        protected override BeatmapDetailArea CreateBeatmapDetailArea() => new MatchBeatmapDetailArea();

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

            var firstItem = Playlist.FirstOrDefault();

            if (firstItem != null)
            {
                Ruleset.Value = firstItem.Ruleset.Value;
                Beatmap.Value = beatmaps.GetWorkingBeatmap(firstItem.Beatmap.Value);
                Mods.Value = firstItem.RequiredMods?.ToArray() ?? Array.Empty<Mod>();
            }

            return false;
        }
    }
}
