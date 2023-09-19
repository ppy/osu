// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Humanizer;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Game.Graphics;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    /// <summary>
    /// A pill that displays the playlist item count.
    /// </summary>
    public partial class PlaylistCountPill : OnlinePlayPill
    {
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

            TextFlow.Clear();
            TextFlow.AddText(activeItems.ToLocalisableString(), s => s.Font = s.Font.With(weight: FontWeight.Bold));
            TextFlow.AddText(" ");
            TextFlow.AddText("Beatmap".ToQuantity(activeItems, ShowQuantityAs.None));
        }
    }
}
