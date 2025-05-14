// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Screens.Edit;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.SelectV2
{
    public partial class SoloSongSelect : SongSelect
    {
        protected override bool OnStart()
        {
            this.Push(new PlayerLoaderV2(() => new SoloPlayer()));
            return false;
        }

        #region Beatmap management

        /// <summary>
        /// Opens beatmap editor with the given beatmap.
        /// </summary>
        public void Edit(BeatmapInfo beatmap)
        {
            // Forced refetch is important here to guarantee correct invalidation across all difficulties.
            Beatmap.Value = Beatmaps.GetWorkingBeatmap(beatmap, true);

            this.Push(new EditorLoader());
        }

        public override IEnumerable<MenuItem> CreateForwardNavigationMenuItemsForBeatmap(BeatmapInfo beatmap) => new[]
        {
            new OsuMenuItem(ButtonSystemStrings.Play.ToSentence(), MenuItemType.Highlighted, () => FinaliseSelection(beatmap)),
            new OsuMenuItem(ButtonSystemStrings.Edit.ToSentence(), MenuItemType.Standard, () => Edit(beatmap)),
        };

        #endregion

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
