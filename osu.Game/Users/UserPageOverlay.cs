// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Users.UserPage;

namespace osu.Game.Users
{
    public class UserPageOverlay : FocusedOverlayContainer
    {
        private readonly User user;
        private UserPageSection lastSection;
        public UserPageOverlay(User user)
        {
            this.user = user;
            var tab = new OsuTabControl<UserPageSection>();
            var sections = new UserPageSection[] { };
            var sectionsContainer = new SectionsContainer
            {
                ExpandableHeader = new UserPageHeader(user),
                FixedHeader = tab,
                Sections = sections
            };

            Add(sectionsContainer);

            sectionsContainer.SelectedSection.ValueChanged += s =>
            {
                if (lastSection != s)
                {
                    lastSection = s as UserPageSection;
                    tab.Current.Value = lastSection;
                }
            };

            tab.Current.ValueChanged += s =>
            {
                if (lastSection != s)
                {
                    lastSection = s;
                    sectionsContainer.ScrollContainer.ScrollIntoView(lastSection);
                }
            };
        }
    }
}
