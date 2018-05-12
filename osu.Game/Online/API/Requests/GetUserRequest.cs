// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
