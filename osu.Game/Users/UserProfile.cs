// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Users.Profile;

namespace osu.Game.Users
{
    public class UserProfile : FocusedOverlayContainer
    {
        private readonly User user;
        private ProfileSection lastSection;

        public const float CONTENT_X_MARGIN = 50;

        public UserProfile(User user)
        {
            this.user = user;
            var tab = new OsuTabControl<ProfileSection>();
            var sections = new ProfileSection[] { };

            Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = OsuColour.Gray(0.2f)
            });

            var sectionsContainer = new SectionsContainer
            {
                RelativeSizeAxes = Axes.Both,
                ExpandableHeader = new UserPageHeader(user),
                FixedHeader = tab,
                HeaderBackground = new Box
                {
                    Colour = OsuColour.Gray(34),
                    RelativeSizeAxes = Axes.Both
                },
                Sections = sections
            };
            Add(sectionsContainer);

            sectionsContainer.SelectedSection.ValueChanged += s =>
            {
                if (lastSection != s)
                {
                    lastSection = s as ProfileSection;
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
