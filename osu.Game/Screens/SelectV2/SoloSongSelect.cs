// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Screens;
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
