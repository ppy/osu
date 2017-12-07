// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Users;

namespace osu.Game.Online.API.Requests
{
    public class GetUserRequest : APIRequest<User>
    {
        private long? userId;

        /// <param name="userId">The user's ID.</param>
        public GetUserRequest(long? userId = null)
        {
            this.userId = userId;
        }

        // Prefer ID over name
        protected override string Target => userId.HasValue ? $@"users/{userId}" : @"me";
    }
}
