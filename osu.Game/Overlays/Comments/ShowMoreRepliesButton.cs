// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Framework.Allocation;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.Comments
{
    public class ShowMoreRepliesButton : GetCommentRepliesButton
    {
        public ShowMoreRepliesButton(Comment comment)
            : base(comment)
        {
            Margin = new MarginPadding { Vertical = 10, Left = 80 };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            IdleColour = colours.Blue;
            HoverColour = colours.BlueLighter;
        }

        protected override string ButtonText() => @"show more";
    }
}
