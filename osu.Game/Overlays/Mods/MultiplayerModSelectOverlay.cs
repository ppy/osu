// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays.Mods
{
    public partial class MultiplayerModSelectOverlay : UserModSelectOverlay
    {
        public MultiplayerModSelectOverlay(OverlayColourScheme colourScheme = OverlayColourScheme.Plum)
            : base(colourScheme)
        {
        }

        [Resolved(CanBeNull = true)]
        private IBindable<PlaylistItem>? multiplayerRoomItem { get; set; }

        [Resolved]
        private OsuGameBase game { get; set; } = null!;

        protected override BeatmapAttributesDisplay GetBeatmapAttributesDisplay => new MultiplayerBeatmapAttributesDisplay
        {
            Anchor = Anchor.BottomRight,
            Origin = Anchor.BottomRight,
            BeatmapInfo = { Value = Beatmap?.BeatmapInfo }
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            multiplayerRoomItem?.BindValueChanged(_ => SelectedMods.TriggerChange());
        }

        protected override IEnumerable<Mod> AllSelectedMods
        {
            get
            {
                IEnumerable<Mod> allMods = SelectedMods.Value;

                if (multiplayerRoomItem?.Value != null)
                {
                    Ruleset ruleset = game.Ruleset.Value.CreateInstance();
                    var multiplayerRoomMods = multiplayerRoomItem.Value.RequiredMods.Select(m => m.ToMod(ruleset));
                    allMods = allMods.Concat(multiplayerRoomMods);
                }

                return allMods;
            }
        }
    }

    public partial class MultiplayerBeatmapAttributesDisplay : BeatmapAttributesDisplay
    {
        [Resolved(CanBeNull = true)]
        private IBindable<PlaylistItem>? multiplayerRoomItem { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            multiplayerRoomItem?.BindValueChanged(_ => Mods.TriggerChange());
        }

        protected override IEnumerable<Mod> SelectedMods
        {
            get
            {
                IEnumerable<Mod> selectedMods = Mods.Value;

                if (multiplayerRoomItem?.Value != null)
                {
                    Ruleset ruleset = GameRuleset.Value.CreateInstance();
                    var multiplayerRoomMods = multiplayerRoomItem.Value.RequiredMods.Select(m => m.ToMod(ruleset));
                    selectedMods = selectedMods.Concat(multiplayerRoomMods);
                }

                return selectedMods;
            }
        }
    }
}
