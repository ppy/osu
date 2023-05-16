// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Game.Graphics;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public partial class RoomSpecialCategoryPill : OnlinePlayPill
    {
        [Resolved]
        private OsuColour colours { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            pill.Background.Colour = colours.Pink;
            pill.Background.Alpha = 1;

            Category.BindValueChanged(c =>
            {
                textFlow.Clear();
                textFlow.AddText(c.NewValue.GetLocalisableDescription());

                var backgroundColour = colours.ForRoomCategory(Category.Value);
                if (backgroundColour != null)
                    pill.Background.Colour = backgroundColour.Value;
            }, true);
        }
    }
}
