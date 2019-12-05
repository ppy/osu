// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests;

namespace osu.Game.Overlays.Comments
{
    public class CommentBundleParameters
    {
        /// <summary>
        /// The type of resource to get comments for.
        /// </summary>
        public CommentableType Type { get; private set; }

        /// <summary>
        /// The id of the resource to get comments for.
        /// </summary>
        public long Id { get; private set; }

        public CommentBundleParameters(CommentableType type, long id)
        {
            Type = type;
            Id = id;
        }
    }
}
