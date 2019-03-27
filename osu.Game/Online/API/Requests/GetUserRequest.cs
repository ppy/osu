// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Users;

namespace osu.Game.Online.API.Requests
{
    public class GetUserRequest : APIRequest<User>
    {
        private readonly long? userId;

        public GetUserRequest(long? userId = null)
        {
            this.userId = userId;
        }

        protected override string Target => userId.HasValue ? $@"users/{userId}" : @"me";
    }
}
