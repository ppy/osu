// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Screens.Play;
using osu.Game.Users;
using osuTK.Input;

namespace osu.Game.Screens.Select
{
    public class PlaySongSelect : SongSelect
    {
        private bool removeAutoModOnResume;
        private OsuScreen player;

        public override bool AllowExternalScreenChange => true;

        protected override UserActivity InitialActivity => new UserActivity.ChoosingBeatmap();

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BeatmapOptions.AddButton(@"Edit", @"beatmap", FontAwesome.Solid.PencilAlt, colours.Yellow, () =>
            {
                ValidForResume = false;
                Edit();
            }, Key.Number4);

            ((PlayBeatmapDetailArea)BeatmapDetails).Leaderboard.ScoreSelected += score => this.Push(new SoloResults(score));
        }

        protected override BeatmapDetailArea CreateBeatmapDetailArea() => new PlayBeatmapDetailArea();

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);

            player = null;

            if (removeAutoModOnResume)
            {
                var autoType = Ruleset.Value.CreateInstance().GetAutoplayMod().GetType();
                ModSelect.DeselectTypes(new[] { autoType }, true);
                removeAutoModOnResume = false;
            }
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    // this is a special hard-coded case; we can't rely on OnPressed (of SongSelect) as GlobalActionContainer is
                    // matching with exact modifier consideration (so Ctrl+Enter would be ignored).
                    FinaliseSelection();
                    return true;
            }

            return base.OnKeyDown(e);
        }

        protected override bool OnStart()
        {
            if (player != null) return false;

            // Ctrl+Enter should start map with autoplay enabled.
            if (GetContainingInputManager().CurrentState?.Keyboard.ControlPressed == true)
            {
                var auto = Ruleset.Value.CreateInstance().GetAutoplayMod();
                var autoType = auto.GetType();

                var mods = Mods.Value;

                if (mods.All(m => m.GetType() != autoType))
                {
                    Mods.Value = mods.Append(auto).ToArray();
                    removeAutoModOnResume = true;
                }
            }

            Beatmap.Value.Track.Looping = false;

            SampleConfirm?.Play();

            this.Push(player = new PlayerLoader(() => new Player()));

            return true;
        }
    }
}
