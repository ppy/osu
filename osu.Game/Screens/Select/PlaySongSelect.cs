// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Overlays.Mods;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Select
{
    public class PlaySongSelect : SongSelect
    {
        private OsuScreen player;
        private readonly ModSelectOverlay modSelect;
        private readonly BeatmapDetailArea beatmapDetails;

        public PlaySongSelect()
        {
            FooterPanels.Add(modSelect = new ModSelectOverlay
            {
                RelativeSizeAxes = Axes.X,
                Origin = Anchor.BottomCentre,
                Anchor = Anchor.BottomCentre,
            });

            LeftContent.Add(beatmapDetails = new BeatmapDetailArea
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding { Top = 10, Right = 5 },
            });

            beatmapDetails.Leaderboard.ScoreSelected += s => Push(new Results(s));
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

        protected override void OnBeatmapChanged(WorkingBeatmap beatmap)
        {
            beatmap?.Mods.BindTo(modSelect.SelectedMods);

            beatmapDetails.Beatmap = beatmap;

            base.OnBeatmapChanged(beatmap);
        }

        protected override void OnResuming(Screen last)
        {
            player = null;
            base.OnResuming(last);
        }

        protected override bool OnExiting(Screen next)
        {
            if (modSelect.State == Visibility.Visible)
            {
                modSelect.Hide();
                return true;
            }

            return base.OnExiting(next);
        }

        protected override void OnSelected()
        {
            if (player != null) return;

            LoadComponentAsync(player = new PlayerLoader(new Player
            {
                Beatmap = Beatmap, //eagerly set this so it's present before push.
            }), l => Push(player));
        }
    }
}
