// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets;
using osu.Game.Online.Rooms;
using osu.Game.Beatmaps;
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

        protected override void UpdateRankingInformation()
        {
            if (RankingInformationDisplay == null)
                return;

            double multiplier = 1.0;

            foreach (var mod in SelectedMods.Value)
                multiplier *= mod.ScoreMultiplier;

            RankingInformationDisplay.Ranked.Value = SelectedMods.Value.All(m => m.Ranked);

            if (multiplayerRoomItem?.Value != null)
            {
                Ruleset ruleset = game.Ruleset.Value.CreateInstance();
                var multiplayerRoomMods = multiplayerRoomItem.Value.RequiredMods.Select(m => m.ToMod(ruleset));

                foreach (var mod in multiplayerRoomMods)
                    multiplier *= mod.ScoreMultiplier;

                RankingInformationDisplay.Ranked.Value = RankingInformationDisplay.Ranked.Value && multiplayerRoomMods.All(m => m.Ranked);
            }

            RankingInformationDisplay.ModMultiplier.Value = multiplier;
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

        protected override double GetRate()
        {
            double rate = base.GetRate();
            Ruleset ruleset = GameRuleset.Value.CreateInstance();

            if (multiplayerRoomItem?.Value != null)
            {
                var multiplayerRoomMods = multiplayerRoomItem.Value.RequiredMods.Select(m => m.ToMod(ruleset));
                foreach (var mod in multiplayerRoomMods.OfType<IApplicableToRate>())
                    rate = mod.ApplyToRate(0, rate);
            }

            return rate;
        }

        protected override BeatmapDifficulty GetDifficulty()
        {
            BeatmapDifficulty originalDifficulty = base.GetDifficulty();
            Ruleset ruleset = GameRuleset.Value.CreateInstance();

            if (multiplayerRoomItem?.Value != null)
            {
                var multiplayerRoomMods = multiplayerRoomItem.Value.RequiredMods.Select(m => m.ToMod(ruleset));
                foreach (var mod in multiplayerRoomMods.OfType<IApplicableToDifficulty>())
                    mod.ApplyToDifficulty(originalDifficulty);
            }

            return originalDifficulty;
        }
    }
}
