// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.Cursor;

namespace osu.Game.Overlays.Rankings
{
    public class HumanizedNumber : OsuSpriteText, IHasTooltip
    {
        private const int k = 1000;

        public string TooltipText => value < k ? "" : $"{value:N0}";

        private readonly long value;

        public HumanizedNumber(long value)
        {
            this.value = value;

            Text = numberToText(value);
        }

        private string numberToText(long value)
        {
            var suffixes = new[]
            {
                "",
                "k",
                "million",
                "billion",
                "trillion",
            };

            if (value < k)
                return value.ToString();

            int i = (int)Math.Floor(Math.Log(value) / Math.Log(k));
            return $"{value / Math.Pow(k, i):F2} {suffixes[i]}";
        }
    }
}
