// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Overlays.Dashboard.Friends
{
    public partial class FriendsOnlineStatusItem : OverlayStreamItem<OnlineStatus>
    {
        public readonly IBindable<int> UserCount = new Bindable<int>();

        public FriendsOnlineStatusItem(OnlineStatus value)
            : base(value)
        {
            MainText = value.GetLocalisableDescription();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            UserCount.BindValueChanged(count => AdditionalText = count.NewValue.ToString(), true);
        }

        protected override Color4 GetBarColour(OsuColour colours)
        {
            switch (Value)
            {
                case OnlineStatus.All:
                    return Color4.White;

                case OnlineStatus.Online:
                    return colours.GreenLight;

                case OnlineStatus.Offline:
                    return Color4.Black;

                default:
                    throw new ArgumentException($@"{Value} status does not provide a colour in {nameof(GetBarColour)}.");
            }
        }
    }
}
