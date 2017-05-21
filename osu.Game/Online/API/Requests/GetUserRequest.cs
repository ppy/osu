// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Users;

namespace osu.Game.Online.API.Requests
{
    public class GetUserRequest : APIRequest<User>
    {
        private int? userId;

        public GetUserRequest(int? userId = null)
        {
            this.userId = userId;
        }

        protected override string Target => userId.HasValue ? $@"users/{userId}" : @"me";
    }
}
