// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Comments
{
    public partial class CommentsShowMoreButton : ShowMoreButton
    {
        public readonly BindableInt Current = new BindableInt();

        protected override void LoadComplete()
        {
            Current.BindValueChanged(onCurrentChanged, true);
            base.LoadComplete();
        }

        private void onCurrentChanged(ValueChangedEvent<int> count)
        {
            Text = new TranslatableString(@"_", "{0} ({1})",
                CommonStrings.ButtonsShowMore.ToUpper(), count.NewValue);
        }
    }
}
