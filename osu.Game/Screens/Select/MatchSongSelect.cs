// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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

        public string ShortTitle => "song selection";
        public override string Title => ShortTitle.Humanize();

        [Resolved(typeof(Room))]
        protected Bindable<PlaylistItem> CurrentItem { get; private set; }

        [Resolved]
        private Bindable<IEnumerable<Mod>> selectedMods { get; set; }

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

            item.RequiredMods.AddRange(SelectedMods.Value);

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
                Beatmap.Value.Mods.Value = selectedMods.Value = CurrentItem.Value.RequiredMods ?? Enumerable.Empty<Mod>();
            }

            Beatmap.Disabled = true;
            Ruleset.Disabled = true;

            return false;
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            Beatmap.Disabled = false;
            Ruleset.Disabled = false;
        }
    }
}
