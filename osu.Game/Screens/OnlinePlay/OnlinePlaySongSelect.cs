// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Online.Rooms;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Select;
using osu.Game.Utils;

namespace osu.Game.Screens.OnlinePlay
{
    public abstract class OnlinePlaySongSelect : SongSelect, IOnlinePlaySubScreen
    {
        public string ShortTitle => "song selection";

        public override string Title => ShortTitle.Humanize();

        public override bool AllowEditing => false;

        [Resolved(typeof(Room), nameof(Room.Playlist))]
        protected BindableList<PlaylistItem> Playlist { get; private set; }

        private readonly Bindable<IReadOnlyList<Mod>> freeMods = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());
        private readonly FreeModSelectOverlay freeModSelectOverlay;

        private WorkingBeatmap initialBeatmap;
        private RulesetInfo initialRuleset;
        private IReadOnlyList<Mod> initialMods;
        private bool itemSelected;

        protected OnlinePlaySongSelect()
        {
            Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING };

            freeModSelectOverlay = new FreeModSelectOverlay
            {
                SelectedMods = { BindTarget = freeMods },
                IsValidMod = IsValidFreeMod,
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            initialBeatmap = Beatmap.Value;
            initialRuleset = Ruleset.Value;
            initialMods = Mods.Value.ToList();

            FooterPanels.Add(freeModSelectOverlay);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // At this point, Mods contains both the required and allowed mods. For selection purposes, it should only contain the required mods.
            // Similarly, freeMods is currently empty but should only contain the allowed mods.
            Mods.Value = Playlist.FirstOrDefault()?.RequiredMods.Select(m => m.CreateCopy()).ToArray() ?? Array.Empty<Mod>();
            freeMods.Value = Playlist.FirstOrDefault()?.AllowedMods.Select(m => m.CreateCopy()).ToArray() ?? Array.Empty<Mod>();

            Ruleset.BindValueChanged(onRulesetChanged);
        }

        private void onRulesetChanged(ValueChangedEvent<RulesetInfo> ruleset)
        {
            freeMods.Value = Array.Empty<Mod>();
        }

        protected sealed override bool OnStart()
        {
            itemSelected = true;

            var item = new PlaylistItem();

            item.Beatmap.Value = Beatmap.Value.BeatmapInfo;
            item.Ruleset.Value = Ruleset.Value;

            item.RequiredMods.Clear();
            item.RequiredMods.AddRange(Mods.Value.Select(m => m.CreateCopy()));

            item.AllowedMods.Clear();
            item.AllowedMods.AddRange(freeMods.Value.Select(m => m.CreateCopy()));

            SelectItem(item);
            return true;
        }

        /// <summary>
        /// Invoked when the user has requested a selection of a beatmap.
        /// </summary>
        /// <param name="item">The resultant <see cref="PlaylistItem"/>. This item has not yet been added to the <see cref="Room"/>'s.</param>
        protected abstract void SelectItem(PlaylistItem item);

        public override bool OnBackButton()
        {
            if (freeModSelectOverlay.State.Value == Visibility.Visible)
            {
                freeModSelectOverlay.Hide();
                return true;
            }

            return base.OnBackButton();
        }

        public override bool OnExiting(IScreen next)
        {
            if (!itemSelected)
            {
                Beatmap.Value = initialBeatmap;
                Ruleset.Value = initialRuleset;
                Mods.Value = initialMods;
            }

            return base.OnExiting(next);
        }

        protected override ModSelectOverlay CreateModSelectOverlay() => new LocalPlayerModSelectOverlay
        {
            IsValidMod = IsValidMod
        };

        protected override IEnumerable<(FooterButton, OverlayContainer)> CreateFooterButtons()
        {
            var buttons = base.CreateFooterButtons().ToList();
            buttons.Insert(buttons.FindIndex(b => b.Item1 is FooterButtonMods) + 1, (new FooterButtonFreeMods { Current = freeMods }, freeModSelectOverlay));
            return buttons;
        }

        /// <summary>
        /// Checks whether a given <see cref="Mod"/> is valid for global selection.
        /// </summary>
        /// <param name="mod">The <see cref="Mod"/> to check.</param>
        /// <returns>Whether <paramref name="mod"/> is a valid mod for online play.</returns>
        protected virtual bool IsValidMod(Mod mod) => mod.HasImplementation && !ModUtils.FlattenMod(mod).Any(m => m is ModAutoplay);

        /// <summary>
        /// Checks whether a given <see cref="Mod"/> is valid for per-player free-mod selection.
        /// </summary>
        /// <param name="mod">The <see cref="Mod"/> to check.</param>
        /// <returns>Whether <paramref name="mod"/> is a selectable free-mod.</returns>
        protected virtual bool IsValidFreeMod(Mod mod) => IsValidMod(mod);
    }
}
