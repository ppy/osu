// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Mods;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select.Leaderboards;

namespace osu.Game.Screens.Select
{
    public class PlaySongSelect : SongSelect
    {
        private OsuScreen player;
        private ModSelectOverlay modSelect;
        private Leaderboard leaderboard;

        public PlaySongSelect()
        {
            Add(modSelect = new ModSelectOverlay
            {
                RelativeSizeAxes = Axes.X,
                Origin = Anchor.BottomCentre,
                Anchor = Anchor.BottomCentre,
                Margin = new MarginPadding { Bottom = 50 }
            });

            LeftContent.Add(leaderboard = new Leaderboard
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding { Top = 5 },
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Footer.AddButton(@"mods", colours.Yellow, modSelect.ToggleVisibility, Key.F1, float.MaxValue);

            BeatmapOptions.AddButton(@"Remove", @"from unplayed", FontAwesome.fa_times_circle_o, colours.Purple, null, Key.Number1);
            BeatmapOptions.AddButton(@"Clear", @"local scores", FontAwesome.fa_eraser, colours.Purple, null, Key.Number2);
            BeatmapOptions.AddButton(@"Edit", @"Beatmap", FontAwesome.fa_pencil, colours.Yellow, () =>
            {
                ValidForResume = false;
                Push(new Editor());
            }, Key.Number3);
        }

        private GetScoresRequest getScoresRequest;

        protected override void OnBeatmapChanged(WorkingBeatmap beatmap)
        {
            beatmap?.Mods.BindTo(modSelect.SelectedMods);

            updateLeaderboard(beatmap);

            base.OnBeatmapChanged(beatmap);
        }

        private void updateLeaderboard(WorkingBeatmap beatmap)
        {
            leaderboard.Scores = null;
            getScoresRequest?.Cancel();

            if (beatmap?.BeatmapInfo == null) return;

            getScoresRequest = new GetScoresRequest(beatmap.BeatmapInfo);
            getScoresRequest.Success += r => leaderboard.Scores = r.Scores;
            Game.API.Queue(getScoresRequest);
        }

        protected override void OnResuming(Screen last)
        {
            player = null;
            base.OnResuming(last);
        }

        protected override void OnSelected()
        {
            if (player != null) return;

            (player = new PlayerLoader(new Player
            {
                Beatmap = Beatmap, //eagerly set this so it's present before push.
            })).LoadAsync(Game, l => Push(player));
        }
    }
}
