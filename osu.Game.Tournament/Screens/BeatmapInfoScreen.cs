// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Screens;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;

namespace osu.Game.Tournament.Screens
{
    public abstract class BeatmapInfoScreen : OsuScreen
    {
        protected readonly SongBar SongBar;

        protected BeatmapInfoScreen()
        {
            Add(SongBar = new SongBar
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
            });
        }

        [BackgroundDependencyLoader]
        private void load(FileBasedIPC ipc)
        {
            ipc.Beatmap.BindValueChanged(beatmapChanged, true);
            ipc.Mods.BindValueChanged(modsChanged, true);
        }

        private void modsChanged(LegacyMods mods)
        {
            SongBar.Mods = mods;
        }

        private void beatmapChanged(BeatmapInfo beatmap)
        {
            SongBar.FadeInFromZero(300, Easing.OutQuint);
            SongBar.Beatmap = beatmap;
        }
    }
}
