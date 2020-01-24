// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Comments
{
    public class CommentsShowMoreButton : ShowMoreButton
    {
        public readonly BindableInt Current = new BindableInt();

        private readonly OverlayColourScheme colourScheme;

        public CommentsShowMoreButton(OverlayColourScheme colourScheme)
        {
            this.colourScheme = colourScheme;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            IdleColour = colours.ForOverlayElement(colourScheme, 0.1f, 0.3f);
            HoverColour = colours.ForOverlayElement(colourScheme, 0.1f, 0.4f);
            ChevronIconColour = colours.ForOverlayElement(colourScheme, 0.1f, 0.6f);
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
