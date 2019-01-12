﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Screens.Play;
using osuTK.Input;

namespace osu.Game.Screens.Select
{
    public class PlaySongSelect : SongSelect
    {
        private bool removeAutoModOnResume;
        private OsuScreen player;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BeatmapOptions.AddButton(@"Edit", @"beatmap", FontAwesome.fa_pencil, colours.Yellow, () =>
            {
                ValidForResume = false;
                Edit();
            }, Key.Number3);
        }

        protected override void OnResuming(Screen last)
        {
            player = null;

            if (removeAutoModOnResume)
            {
                var autoType = Ruleset.Value.CreateInstance().GetAutoplayMod().GetType();
                ModSelect.DeselectTypes(new[] { autoType }, true);
                removeAutoModOnResume = false;
            }

            base.OnResuming(last);
        }

        protected override bool OnStart()
        {
            if (player != null) return false;

            // Ctrl+Enter should start map with autoplay enabled.
            if (GetContainingInputManager().CurrentState?.Keyboard.ControlPressed == true)
            {
                var auto = Ruleset.Value.CreateInstance().GetAutoplayMod();
                var autoType = auto.GetType();

                var mods = SelectedMods.Value;
                if (mods.All(m => m.GetType() != autoType))
                {
                    SelectedMods.Value = mods.Append(auto);
                    removeAutoModOnResume = true;
                }
            }

            Beatmap.Value.Track.Looping = false;
            Beatmap.Disabled = true;

            SampleConfirm?.Play();

            LoadComponentAsync(player = new PlayerLoader(() => new Player()), l =>
            {
                if (IsCurrentScreen) Push(player);
            });

            return true;
        }
    }
}
