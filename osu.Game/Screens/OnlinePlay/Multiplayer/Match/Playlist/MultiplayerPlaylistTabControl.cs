// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Match.Playlist
{
    public class MultiplayerPlaylistTabControl : OsuTabControl<MultiplayerPlaylistDisplayMode>
    {
        public Bindable<int> queueListCount = new Bindable<int>(0);

        public MultiplayerPlaylistTabControl()
        {
        }

        protected override TabItem<MultiplayerPlaylistDisplayMode> CreateTabItem(MultiplayerPlaylistDisplayMode value)
        {
            if (value == MultiplayerPlaylistDisplayMode.Queue)
                return new QueueTabItem(value, queueListCount);
            return new OsuTabItem(value);
        }

        private class QueueTabItem : OsuTabItem
        {
            public QueueTabItem(MultiplayerPlaylistDisplayMode value, Bindable<int> queueListCount)
                : base(value)
            {
                queueListCount.BindValueChanged(
                    _ => Text.Text = "Queue (" + queueListCount.Value + ")", true);
            }
        }
    }
}
