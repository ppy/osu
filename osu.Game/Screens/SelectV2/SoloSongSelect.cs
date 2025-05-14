// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.SelectV2
{
    public partial class SoloSongSelect : SongSelect
    {
        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        /// <summary>
        /// Opens beatmap editor with the given beatmap.
        /// </summary>
        public void Edit(BeatmapInfo beatmap)
        {
            // Forced refetch is important here to guarantee correct invalidation across all difficulties.
            Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmap, true);

            this.Push(new EditorLoader());
        }

        protected override bool OnStart()
        {
            this.Push(new PlayerLoaderV2(() => new SoloPlayer()));
            return false;
        }

        private partial class PlayerLoaderV2 : PlayerLoader
        {
            public override bool ShowFooter => true;

            public PlayerLoaderV2(Func<Player> createPlayer)
                : base(createPlayer)
            {
            }
        }
    }
}
