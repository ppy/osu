// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;

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

            PlaylistItemStats.BindValueChanged(_ => updateCount());
            Playlist.BindCollectionChanged((_, _) => updateCount(), true);
        }

        private void updateCount()
        {
            int activeItems = Playlist.Count > 0 || PlaylistItemStats.Value == null
                // For now, use the playlist as the source of truth if it has any items.
                // This allows the count to display correctly on the room screen (after joining a room).
                ? Playlist.Count(i => !i.Expired)
                : PlaylistItemStats.Value.CountActive;

            count.Clear();
            count.AddText(activeItems.ToLocalisableString(), s => s.Font = s.Font.With(weight: FontWeight.Bold));
            count.AddText(" ");
            count.AddText("Beatmap".ToQuantity(activeItems, ShowQuantityAs.None));
        }
    }
}
