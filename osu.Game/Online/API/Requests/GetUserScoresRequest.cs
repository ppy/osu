// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;

namespace osu.Game.Online.API.Requests
{
    public class GetUserScoresRequest : APIRequest<List<OnlineScore>>
    {
        private readonly long userId;
        private readonly ScoreType type;
        private readonly Mode playMode;
        private readonly int offset;

        public GetUserScoresRequest(long userId, ScoreType type, Mode playMode = Mode.Default, int offset = 0)
        {
            this.userId = userId;
            this.type = type;
            this.playMode = playMode;
            this.offset = offset;
        }

        protected override string Target => playMode == Mode.Default ? $@"users/{userId}/scores/{type.ToString().ToLower()}?offset={offset}"
            : $@"users/{userId}/scores/{type.ToString().ToLower()}?mode={playMode.ToString().ToLower()}&offset={offset}";
    }

    public enum ScoreType
    {
        Best,
        Firsts,
        Recent
    }
}