// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.Multi.Screens.Match
{
    public class Match : MultiplayerScreen
    {
        private readonly Room room;
        private readonly Bindable<BeatmapInfo> beatmapBind = new Bindable<BeatmapInfo>();

        public override string Title => room.Name.Value;

        public Match(Room room)
        {
            this.room = room;
            Header header;

            Children = new Drawable[]
            {
                header = new Header(),
            };

            header.BeatmapButton.Action = () =>
            {
                Push(new MatchSongSelect());
            };

            beatmapBind.ValueChanged += b =>
            {
                header.BeatmapSet = b.BeatmapSet;
            };

            beatmapBind.BindTo(room.Beatmap);
        }
    }
}
