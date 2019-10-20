// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osuTK.Graphics;
using System.Collections.Generic;
using System.Linq;

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

        protected override void OnChildrenChanged(ValueChangedEvent<List<Comment>> children)
        {
            Alpha = children.NewValue.Any() || Comment.RepliesCount == 0 ? 0 : 1;
        }

        protected override string ButtonText() => @"[+] load replies";
    }
}
