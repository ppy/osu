// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect
{
    public partial class MatchmakingSelectPanelBeatmap : MatchmakingSelectPanel
    {
        public new MatchmakingPlaylistItemBeatmap Item => (MatchmakingPlaylistItemBeatmap)base.Item;

        public MatchmakingSelectPanelBeatmap(MatchmakingPlaylistItemBeatmap item)
            : base(item) { }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            Add(content = new BeatmapCardMatchmakingBeatmapContent(Item.Beatmap, Item.Mods));
        }

        public override void PresentAsChosenBeatmap(MatchmakingPlaylistItemBeatmap item)
        {
            ShowChosenBorder();

            this.MoveTo(Vector2.Zero, 1000, Easing.OutExpo)
                .ScaleTo(1.5f, 1000, Easing.OutExpo);
        }

        private BeatmapCardMatchmakingBeatmapContent? content;

        protected override float AvatarOverlayOffset => content?.AvatarOffset ?? 0;
    }
}
