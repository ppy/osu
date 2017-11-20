// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Users;

namespace osu.Game.Online.API.Requests
{
    public class GetUserRequest : APIRequest<User>
    {
        private long? userId;
        private Mode userPlayMode;

        public GetUserRequest(long? userId = null, Mode userPlayMode = Mode.Default)
        {
            this.userId = userId;
            this.userPlayMode = userPlayMode;
        }

        protected override string Target => userId.HasValue ? (userPlayMode == Mode.Default ? $@"users/{userId}"
            : $@"users/{userId}/{userPlayMode.ToString().ToLower()}") : @"me";
    }

    public enum Mode
    {
        Default,
        Osu,
        Mania,
        Taiko,
        Fruits
    }
}
