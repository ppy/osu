// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Screens.Ranking
{
    public record UserTag
    {
        public long Id { get; }
        public string Name { get; }
        public string Description { get; }

        public BindableInt VoteCount { get; } = new BindableInt();
        public BindableBool Voted { get; } = new BindableBool();

        public UserTag(APITag tag)
        {
            Id = tag.Id;
            Name = tag.Name;
            Description = tag.Description;
        }
    }
}
