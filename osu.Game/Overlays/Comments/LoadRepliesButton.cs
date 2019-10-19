// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osuTK.Graphics;

namespace osu.Game.Overlays.Comments
{
    public class LoadRepliesButton : GetCommentRepliesButton
    {
        public LoadRepliesButton(Comment comment)
            : base(comment)
        {
            IdleColour = OsuColour.Gray(0.7f);
            HoverColour = Color4.White;
        }

        protected override void OnLoadingFinished() => Hide();

        protected override string ButtonText() => @"[+] load replies";
    }
}
