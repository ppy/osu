// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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

        public string ShortTitle => "song selection";
        public override string Title => ShortTitle.Humanize();

        [Resolved(typeof(Room))]
        protected Bindable<PlaylistItem> CurrentItem { get; private set; }

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
                Beatmap = Beatmap.Value.BeatmapInfo,
                Ruleset = Ruleset.Value,
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
                Ruleset.Value = CurrentItem.Value.Ruleset;
                Beatmap.Value = beatmaps.GetWorkingBeatmap(CurrentItem.Value.Beatmap);
                Mods.Value = CurrentItem.Value.RequiredMods?.ToArray() ?? Array.Empty<Mod>();
            }

            Beatmap.Disabled = true;
            Ruleset.Disabled = true;
            Mods.Disabled = true;

            return false;
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            Beatmap.Disabled = false;
            Ruleset.Disabled = false;
            Mods.Disabled = false;
        }
    }
}
