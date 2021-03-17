// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Match;

namespace osu.Game.Screens.OnlinePlay
{
    /// <summary>
    /// An <see cref="OnlinePlayComposite"/> with additional logic tracking the currently-selected <see cref="PlaylistItem"/> inside a <see cref="RoomSubScreen"/>.
    /// </summary>
    public class RoomSubScreenComposite : OnlinePlayComposite
    {
        [Resolved]
        private IBindable<PlaylistItem> subScreenSelectedItem { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            subScreenSelectedItem.BindValueChanged(_ => UpdateSelectedItem(), true);
        }

        protected override void UpdateSelectedItem()
        {
            if (RoomID.Value == null)
            {
                // If the room hasn't been created yet, fall-back to the base logic.
                base.UpdateSelectedItem();
                return;
            }

            SelectedItem.Value = subScreenSelectedItem.Value;
        }
    }
}
