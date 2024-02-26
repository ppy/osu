// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;

namespace osu.Game.Screens.OnlinePlay.Match
{
    public partial class RoomModSelectOverlay : UserModSelectOverlay
    {
        [Resolved]
        private IBindable<PlaylistItem> selectedItem { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        public RoomModSelectOverlay()
            : base(OverlayColourScheme.Plum)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedItem.BindValueChanged(v =>
            {
                if (v.NewValue is PlaylistItem item)
                {
                    var rulesetInstance = rulesets.GetRuleset(item.RulesetID)?.CreateInstance();
                    Debug.Assert(rulesetInstance != null);
                    ActiveMods.Value = item.RequiredMods.Select(m => m.ToMod(rulesetInstance)).Concat(SelectedMods.Value).ToList();
                }
                else
                    ActiveMods.Value = SelectedMods.Value;
            }, true);
        }
    }
}
