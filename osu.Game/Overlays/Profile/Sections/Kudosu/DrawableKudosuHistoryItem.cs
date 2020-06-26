// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using System;

namespace osu.Game.Overlays.Profile.Sections.Kudosu
{
    public class DrawableKudosuHistoryItem : DrawableHistoryItem<APIKudosuHistory>
    {
        public DrawableKudosuHistoryItem(APIKudosuHistory history)
            : base(history)
        {
        }

        protected override DateTimeOffset GetDate() => Item.CreatedAt;

        protected override void CreateMessage()
        {
            switch (Item.Source)
            {
                case KudosuSource.AllowKudosu:
                    switch (Item.Action)
                    {
                        case KudosuAction.Give:
                            addTextWithAmount("Received ", "from kudosu deny repeal of modding post ");
                            return;
                    }

                    break;

                case KudosuSource.DenyKudosu:
                    switch (Item.Action)
                    {
                        case KudosuAction.Reset:
                            addTextWithAmount("Denied ", "from modding post ");
                            return;
                    }

                    break;

                case KudosuSource.Delete:
                    switch (Item.Action)
                    {
                        case KudosuAction.Reset:
                            addTextWithAmount("Lost ", "from modding post deletion of ");
                            return;
                    }

                    break;

                case KudosuSource.Restore:
                    switch (Item.Action)
                    {
                        case KudosuAction.Give:
                            addTextWithAmount("Received ", "from modding post restoration of ");
                            return;
                    }

                    break;

                case KudosuSource.Vote:
                    switch (Item.Action)
                    {
                        case KudosuAction.Give:
                            addTextWithAmount("Received ", "from obtaining votes in modding post of ");
                            return;

                        case KudosuAction.Reset:
                            addTextWithAmount("Lost ", "from losing votes in modding post of ");
                            return;
                    }

                    break;

                case KudosuSource.Recalculate:
                    switch (Item.Action)
                    {
                        case KudosuAction.Give:
                            addTextWithAmount("Received ", "from votes recalculation in modding post of ");
                            return;

                        case KudosuAction.Reset:
                            addTextWithAmount("Lost ", "from votes recalculation in modding post of ");
                            return;
                    }

                    break;

                case KudosuSource.Forum:

                    switch (Item.Action)
                    {
                        case KudosuAction.Give:
                            addTextWithAmount("Received ", "from ", false);
                            addGiverLink();
                            AddText(" for a post at ");
                            createPostLink();
                            return;

                        case KudosuAction.Reset:
                            AddText("Kudosu reset by ");
                            addGiverLink();
                            AddText(" for the post ");
                            createPostLink();
                            return;

                        case KudosuAction.Revoke:
                            AddText("Denied kudosu by ");
                            addGiverLink();
                            AddText(" for the post ");
                            createPostLink();
                            return;
                    }

                    break;
            }

            addTextWithAmount("Unknown event (", "change)", false);
        }

        private void createPostLink() => AddLink(Item.Post.Title, LinkAction.External, Item.Post.Url);

        private void addGiverLink() => AddUserLink(Item.Giver?.Username, Item.Giver?.Url);

        private void addTextWithAmount(string prefix, string suffix, bool addPostLink = true)
        {
            string amount = $"{Math.Abs(Item.Amount)} kudosu";

            AddText(prefix);
            AddColoredText($"{amount} ", ColourProvider.Light1);
            AddText(suffix);

            if (addPostLink)
                createPostLink();
        }
    }
}
