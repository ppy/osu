// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match.Playlist
{
    public class MultiplayerPlaylistTabControl : OsuTabControl<MultiplayerPlaylistDisplayMode>
    {
        public int queueListCount;

        public MultiplayerPlaylistTabControl()
        {
            queueListCount = 0;
        }

        protected override TabItem<MultiplayerPlaylistDisplayMode> CreateTabItem(MultiplayerPlaylistDisplayMode value)
        {
            if (value == MultiplayerPlaylistDisplayMode.Queue)
                return new QueueTabItem(value, queueListCount);
            return new OsuTabItem(value);
        }

        private class QueueTabItem : OsuTabItem
        {
            public QueueTabItem(MultiplayerPlaylistDisplayMode value, int count)
                : base(value)
            {
                Text.Text += " (" + count + ")";
            }
        }
    }
}
