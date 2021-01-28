// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users;

namespace osu.Game.Overlays.Profile.Sections.Kudosu
{
    public class PaginatedKudosuHistoryContainer : PaginatedProfileSubsection<APIKudosuHistory>
    {
        public PaginatedKudosuHistoryContainer(Bindable<User> user)
            : base(user, missingText: "This user hasn't received any kudosu!")
        {
            ItemsPerPage = 5;
        }

        protected override APIRequest<List<APIKudosuHistory>> CreateRequest()
            => new GetUserKudosuHistoryRequest(User.Value.Id, VisiblePages++, ItemsPerPage);

        protected override Drawable CreateDrawableItem(APIKudosuHistory item) => new DrawableKudosuHistoryItem(item);
    }
}
