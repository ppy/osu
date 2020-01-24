// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Comments
{
    public class CommentsShowMoreButton : ShowMoreButton
    {
        public readonly BindableInt Current = new BindableInt();

        public CommentsShowMoreButton()
        {
            IdleColour = OsuColour.Gray(0.3f);
            HoverColour = OsuColour.Gray(0.4f);
            ChevronIconColour = OsuColour.Gray(0.5f);
        }

        protected override void LoadComplete()
        {
            Current.BindValueChanged(onCurrentChanged, true);
            base.LoadComplete();
        }

        private void onCurrentChanged(ValueChangedEvent<int> count)
        {
            Text = $@"Show More ({count.NewValue})".ToUpper();
        }
    }
}
