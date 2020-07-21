// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Comments
{
    public class CommentsShowMoreButton : ShowMoreButton
    {
        public readonly BindableInt Current = new BindableInt();

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Height = 20;

            IdleColour = colourProvider.Background2;
            HoverColour = colourProvider.Background1;
            ChevronIconColour = colourProvider.Foreground1;
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
