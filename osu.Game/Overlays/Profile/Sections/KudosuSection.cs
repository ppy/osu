// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays.Profile.Sections.Kudosu;

namespace osu.Game.Overlays.Profile.Sections
{
    public class KudosuSection : ProfileSection
    {
        public override string Title => "Kudosu!";

        public override string Identifier => "kudosu";

        public KudosuSection()
        {
            Children = new[]
            {
                new KudosuInfo(User),
            };
        }
    }
}
