// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Sections
{
    public abstract class PaginatedContainerWithHeader<TModel> : PaginatedContainer<TModel>
    {
        private readonly string headerText;
        private readonly CounterVisibilityState counterVisibilityState;

        private PaginatedContainerHeader header;

        public PaginatedContainerWithHeader(Bindable<User> user, string headerText, CounterVisibilityState counterVisibilityState, string missing = "")
            : base(user, missing)
        {
            this.headerText = headerText;
            this.counterVisibilityState = counterVisibilityState;
        }

        protected override Drawable CreateHeaderContent => header = new PaginatedContainerHeader(headerText, counterVisibilityState);

        protected override void OnUserChanged(User user)
        {
            base.OnUserChanged(user);
            header.Current.Value = GetCount(user);
        }

        protected virtual int GetCount(User user) => 0;
    }
}
