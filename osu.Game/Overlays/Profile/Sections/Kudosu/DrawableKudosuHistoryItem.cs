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

        protected override string GetString()
        {
            switch (Item.Source)
            {
                case KudosuSource.AllowKudosu:
                    switch (Item.Action)
                    {
                        case KudosuAction.Give:
                            return "Received {amount} from kudosu deny repeal of modding post {post}";
                    }

                    break;

                case KudosuSource.DenyKudosu:
                    switch (Item.Action)
                    {
                        case KudosuAction.Reset:
                            return "Denied {amount} from modding post {post}";
                    }

                    break;

                case KudosuSource.Delete:
                    switch (Item.Action)
                    {
                        case KudosuAction.Reset:
                            return "Lost {amount} from modding post deletion of {post}";
                    }

                    break;

                case KudosuSource.Restore:
                    switch (Item.Action)
                    {
                        case KudosuAction.Give:
                            return "Received {amount} from modding post restoration of {post}";
                    }

                    break;

                case KudosuSource.Vote:
                    switch (Item.Action)
                    {
                        case KudosuAction.Give:
                            return "Received {amount} from obtaining votes in modding post of {post}";

                        case KudosuAction.Reset:
                            return "Lost {amount} from losing votes in modding post of {post}";
                    }

                    break;

                case KudosuSource.Recalculate:
                    switch (Item.Action)
                    {
                        case KudosuAction.Give:
                            return "Received {amount} from votes recalculation in modding post of {post}";

                        case KudosuAction.Reset:
                            return "Lost {amount} from votes recalculation in modding post of {post}";
                    }

                    break;

                case KudosuSource.Forum:

                    switch (Item.Action)
                    {
                        case KudosuAction.Give:
                            return "Received {amount} from {giver} for a post at {post}";

                        case KudosuAction.Reset:
                            return "Kudosu reset by {giver} for the post {post}";

                        case KudosuAction.Revoke:
                            return "Denied kudosu by {giver} for the post {post}";
                    }

                    break;
            }

            return "Unknown event({amount} change)";
        }

        protected override void HandleVariable(string name)
        {
            switch (name)
            {
                case "amount":
                    AddColoredText($"{Math.Abs(Item.Amount)} kudosu", ColourProvider.Light1);
                    return;

                case "giver":
                    AddUserLink(Item.Giver?.Username, Item.Giver?.Url);
                    return;

                case "post":
                    AddLink(Item.Post.Title, LinkAction.External, Item.Post.Url);
                    return;
            }
        }
    }
}
