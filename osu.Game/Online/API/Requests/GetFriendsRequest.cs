﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Users;

namespace osu.Game.Online.API.Requests
{
    public class GetFriendsRequest : APIRequest<List<User>>
    {
        protected override string Target => @"friends";
    }
}
