// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.OnlinePlay.Match
{
    public partial class RoomModSelectOverlay : UserModSelectOverlay
    {
        [Resolved]
        private IBindable<PlaylistItem> selectedItem { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        private readonly List<Mod> roomMods = new List<Mod>();

        public RoomModSelectOverlay()
            : base(OverlayColourScheme.Plum)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedItem.BindValueChanged(_ =>
            {
                roomMods.Clear();

                if (selectedItem.Value is PlaylistItem item)
                {
                    var rulesetInstance = rulesets.GetRuleset(item.RulesetID)?.CreateInstance();
                    Debug.Assert(rulesetInstance != null);
                    roomMods.AddRange(item.RequiredMods.Select(m => m.ToMod(rulesetInstance)));
                }

                SelectedMods.TriggerChange();
            });
        }

        protected override void UpdateOverlayInformation(IReadOnlyList<Mod> mods)
            => base.UpdateOverlayInformation(roomMods.Concat(mods).ToList());
    }
}
