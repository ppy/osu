// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Osu.UI.Cursor;
using System;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModWayback : ModWayback, IUpdatableByPlayfield, IApplicableToBeatmap
    {
        public override LocalisableString Description => "Is the cursor there yet?";
        public override Type[] IncompatibleMods => new Type[] { typeof(ModNoScope), typeof(OsuModAutopilot), typeof(OsuModMagnetised) };

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
        }

        public void Update(Playfield playfield)
        {
            var osuPlayfield = (OsuPlayfield)playfield;
            Debug.Assert(osuPlayfield.Cursor != null);

            osuPlayfield.Cursor.Delayed = true;
			osuPlayfield.Cursor.FollowSpeed = FollowSpeed.Value;
            ((OsuCursorContainer)osuPlayfield.Cursor).CursorTrail.Alpha = 0;
            osuPlayfield.Smoke.Alpha = 0;
        }
    }
}
