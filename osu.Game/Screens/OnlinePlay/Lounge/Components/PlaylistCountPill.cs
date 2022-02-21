// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    /// <summary>
    /// A pill that displays the playlist item count.
    /// </summary>
    public class PlaylistCountPill : OnlinePlayComposite
    {
        private OsuTextFlowContainer count;

        public PlaylistCountPill()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new PillContainer
            {
                Child = count = new OsuTextFlowContainer(s => s.Font = OsuFont.GetFont(size: 12))
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            PlaylistItemStats.BindValueChanged(updateCount, true);
        }

        private void updateCount(ValueChangedEvent<Room.RoomPlaylistItemStats> valueChangedEvent)
        {
            int activeItems = valueChangedEvent.NewValue.CountActive;

            count.Clear();
            count.AddText(activeItems.ToLocalisableString(), s => s.Font = s.Font.With(weight: FontWeight.Bold));
            count.AddText(" ");
            count.AddText("Beatmap".ToQuantity(activeItems, ShowQuantityAs.None));
        }
    }
}
