// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Comments.Buttons
{
    public class LoadRepliesButton : LoadingButton
    {
        private ButtonContent content;

        public LoadRepliesButton()
        {
            AutoSizeAxes = Axes.Both;
        }

        protected override Drawable CreateContent() => content = new ButtonContent();

        protected override void OnLoadStarted() => content.ToggleTextVisibility(false);

        protected override void OnLoadFinished() => content.ToggleTextVisibility(true);

        private class ButtonContent : CommentRepliesButton
        {
            public ButtonContent()
            {
                Text = "load replies";
            }
        }
    }
}
