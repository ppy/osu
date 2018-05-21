// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
