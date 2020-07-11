// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;

namespace osu.Game.Overlays.Comments.Buttons
{
    public class ShowRepliesButton : CommentRepliesButton
    {
        private readonly int count;

        public ShowRepliesButton(int count)
        {
            this.count = count;
        }

        protected override string GetText() => "reply".ToQuantity(count);
    }
}
