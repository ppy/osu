// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Header
{
    public abstract class UserBindingComponent : CompositeDrawable
    {
        public readonly Bindable<User> User = new Bindable<User>();

        protected override void LoadComplete()
        {
            base.LoadComplete();
            User.BindValueChanged(user => UpdateUser(user.NewValue), true);
        }

        protected abstract void UpdateUser(User user);
    }
}
