// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API
{
    public interface ILocalUserState
    {
        IBindable<APIUser> User { get; }
        IBindableList<APIRelation> Friends { get; }
        IBindableList<APIRelation> Blocks { get; }
        IBindableList<int> FavouriteBeatmapSets { get; }

        void UpdateFriends();
        void UpdateBlocks();
        void UpdateFavouriteBeatmapSets();
    }
}
