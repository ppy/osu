// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect
{
    public partial class MatchmakingSelectPanelBeatmap : MatchmakingSelectPanel
    {
        private readonly APIBeatmap beatmap;
        private readonly Mod[] mods;

        public MatchmakingSelectPanelBeatmap(MatchmakingPlaylistItem item)
            : base(item.PlaylistItem)
        {
            beatmap = item.Beatmap;
            mods = item.Mods;
        }

        private CardContent content = null!;
        private Sample? resultSample;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            resultSample = audio.Samples.Get(@"Multiplayer/Matchmaking/Selection/roulette-result");

            Add(content = new CardContentBeatmap(beatmap, mods));
        }

        public override void PresentAsChosenBeatmap(MatchmakingPlaylistItem playlistItem)
        {
            ShowChosenBorder();
            this.MoveTo(Vector2.Zero, 1000, Easing.OutExpo)
                .ScaleTo(1.5f, 1000, Easing.OutExpo);

            resultSample?.Play();
        }

        public override void AddUser(APIUser user)
        {
            content.SelectionOverlay.AddUser(user);
        }

        public override void RemoveUser(APIUser user)
        {
            content.SelectionOverlay.RemoveUser(user.Id);
        }
    }
}
