// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Screens;
using osu.Game.Screens.Backgrounds;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Select;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Edit
{
    public class EditorScreen : OsuScreen
    {
        private EditSongSelect songSelect;

        private WorkingBeatmap currentBeatmap;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap);

        internal override bool ShowOverlays => false;

        public EditorScreen()
        {
            Children = new Drawable[]
            {
                new TwoLayerButton
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Colour = Color4.Yellow,
                    Action = delegate { Push(songSelect); },
                    Text = "Song Select",
                }
            };
        }

        private void changeBackground(WorkingBeatmap beatmap)
        {
            var backgroundModeBeatmap = Background as BackgroundScreenBeatmap;
            if (backgroundModeBeatmap != null)
            {
                backgroundModeBeatmap.Beatmap = beatmap;
                backgroundModeBeatmap.FadeTo(1, 250);
            }
        }

        protected override void OnResuming(Screen last)
        {
            if (songSelect?.SelectedBeatmap != null)
            {
                currentBeatmap = songSelect.SelectedBeatmap;
                changeBackground(currentBeatmap);
            }
            songSelect = new EditSongSelect();
            Beatmap.Value.Track?.Stop();
            base.OnResuming(last);
        }
        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            if (songSelect ?.SelectedBeatmap != null)
            {
                currentBeatmap = songSelect.SelectedBeatmap;
                changeBackground(currentBeatmap);
            }
            songSelect = new EditSongSelect();
            Background.FadeColour(Color4.DarkGray, 500);
            Beatmap.Value.Track?.Stop();
        }
        protected override bool OnExiting(Screen next)
        {
            Background.FadeColour(Color4.White, 500);
            Beatmap.Value.Track?.Start();
            return base.OnExiting(next);
        }
    }
}
