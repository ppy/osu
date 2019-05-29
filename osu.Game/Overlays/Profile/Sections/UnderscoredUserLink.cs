// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;

namespace osu.Game.Overlays.Profile.Sections
{
    public class UnderscoredUserLink : UnderscoredLinkContainer
    {
        private readonly long userId;

        public UnderscoredUserLink(long userId)
        {
            this.userId = userId;
        }

        [BackgroundDependencyLoader(true)]
        private void load(UserProfileOverlay userProfileOverlay)
        {
            ClickAction = () => userProfileOverlay?.ShowUser(userId);
        }
    }
}
