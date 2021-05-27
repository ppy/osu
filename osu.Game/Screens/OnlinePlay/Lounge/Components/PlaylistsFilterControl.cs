// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public class PlaylistsFilterControl : FilterControl
    {
        private readonly Dropdown<PlaylistsCategory> dropdown;

        public PlaylistsFilterControl()
        {
            AddInternal(dropdown = new SlimEnumDropdown<PlaylistsCategory>
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.TopRight,
                RelativeSizeAxes = Axes.None,
                Width = 160,
                X = -HORIZONTAL_PADDING,
                Y = -30
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            dropdown.Current.BindValueChanged(_ => UpdateFilter());
        }

        protected override FilterCriteria CreateCriteria()
        {
            var criteria = base.CreateCriteria();

            switch (dropdown.Current.Value)
            {
                case PlaylistsCategory.Normal:
                    criteria.Category = "normal";
                    break;

                case PlaylistsCategory.Spotlight:
                    criteria.Category = "spotlight";
                    break;
            }

            return criteria;
        }

        private enum PlaylistsCategory
        {
            Any,
            Normal,
            Spotlight
        }
    }
}
