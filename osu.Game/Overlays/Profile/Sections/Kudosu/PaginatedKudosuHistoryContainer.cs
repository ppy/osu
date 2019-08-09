// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Online.API.Requests;
using osu.Game.Users;
using System.Linq;
using osu.Framework.Bindables;

namespace osu.Game.Overlays.Profile.Sections.Kudosu
{
    public class PaginatedKudosuHistoryContainer : PaginatedContainer
    {
        private GetUserKudosuHistoryRequest request;

        public PaginatedKudosuHistoryContainer(Bindable<User> user, string header, string missing)
            : base(user, header, missing)
        {
            ItemsPerPage = 5;
        }

        protected override void ShowMore()
        {
            request = new GetUserKudosuHistoryRequest(User.Value.Id, VisiblePages++, ItemsPerPage);
            request.Success += items => Schedule(() =>
            {
                MoreButton.FadeTo(items.Count == ItemsPerPage ? 1 : 0);
                MoreButton.IsLoading = false;

                if (!items.Any() && VisiblePages == 1)
                {
                    MissingText.Show();
                    return;
                }

                MissingText.Hide();

                foreach (var item in items)
                    ItemsContainer.Add(new DrawableKudosuHistoryItem(item));
            });

            Api.Queue(request);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            request?.Cancel();
        }
    }
}
