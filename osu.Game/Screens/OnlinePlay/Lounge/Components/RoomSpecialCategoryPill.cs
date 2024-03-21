// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public partial class RoomSpecialCategoryPill : OnlinePlayPill
    {
        [Resolved]
        private OsuColour colours { get; set; } = null!;

        protected override FontUsage Font => base.Font.With(weight: FontWeight.SemiBold);

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Pill.Background.Alpha = 1;
            TextFlow.Colour = Color4.Black;

            Category.BindValueChanged(c =>
            {
                TextFlow.Text = c.NewValue.GetLocalisableDescription();
                Pill.Background.Colour = colours.ForRoomCategory(c.NewValue) ?? colours.Pink;
            }, true);
        }
    }
}
