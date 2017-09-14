// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;

namespace osu.Game.Online.API.Requests
{
    public class GetUserScoresRequest : APIRequest<List<OnlineScore>>
    {
        private readonly long userId;
        private readonly ScoreType type;

        public GetUserScoresRequest(long userId, ScoreType type)
        {
            this.userId = userId;
            this.type = type;
        }

        protected override string Target => $@"users/{userId}/scores/{type.ToString().ToLower()}";
    }

    public enum ScoreType
    {
        Best,
        Firsts,
        Recent
    }
}