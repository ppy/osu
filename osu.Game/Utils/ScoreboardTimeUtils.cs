// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Resources.Localisation.Web;

#nullable enable

namespace osu.Game.Utils
{
    public static class ScoreboardTimeUtils
    {

        private static string formatQuantity(string template, int quantity)
        {
            if (quantity <= 1)
                return $@"{quantity}{template}";

            return $@"{quantity}{template}s";
        }
    }
}
