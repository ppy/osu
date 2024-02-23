// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.OnlinePlay.Match
{
    public partial class RoomModSelectOverlay : UserModSelectOverlay
    {
        public RoomModSelectOverlay(OverlayColourScheme colourScheme = OverlayColourScheme.Plum)
            : base(colourScheme)
        {
        }

        [Resolved(CanBeNull = true)]
        private IBindable<PlaylistItem>? selectedItem { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        protected override BeatmapAttributesDisplay GetBeatmapAttributesDisplay => new RoomBeatmapAttributesDisplay
        {
            Anchor = Anchor.BottomRight,
            Origin = Anchor.BottomRight,
            BeatmapInfo = { Value = Beatmap?.BeatmapInfo }
        };

        private readonly List<Mod> roomMods = new List<Mod>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedItem?.BindValueChanged(_ =>
            {
                roomMods.Clear();

                if (selectedItem?.Value != null)
                {
                    var rulesetInstance = rulesets.GetRuleset(selectedItem.Value.RulesetID)?.CreateInstance();
                    Debug.Assert(rulesetInstance != null);
                    roomMods.AddRange(selectedItem.Value.RequiredMods.Select(m => m.ToMod(rulesetInstance)));
                }

                SelectedMods.TriggerChange();
            });
        }

        protected override IEnumerable<Mod> AllSelectedMods => roomMods.Concat(base.AllSelectedMods);
    }

    public partial class RoomBeatmapAttributesDisplay : BeatmapAttributesDisplay
    {
        [Resolved(CanBeNull = true)]
        private IBindable<PlaylistItem>? selectedItem { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        private readonly List<Mod> roomMods = new List<Mod>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedItem?.BindValueChanged(_ =>
            {
                roomMods.Clear();

                if (selectedItem?.Value != null)
                {
                    var rulesetInstance = rulesets.GetRuleset(selectedItem.Value.RulesetID)?.CreateInstance();
                    Debug.Assert(rulesetInstance != null);
                    roomMods.AddRange(selectedItem.Value.RequiredMods.Select(m => m.ToMod(rulesetInstance)));
                }

                Mods.TriggerChange();
            });
        }

        protected override IEnumerable<Mod> SelectedMods => roomMods.Concat(base.SelectedMods);
    }
}
