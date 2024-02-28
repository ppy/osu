// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Rulesets.Edit.Checks
{
    public class CheckBackgroundPresence : CheckFilePresence
    {
        protected override CheckCategory Category => CheckCategory.Resources;
        protected override string TypeOfFile => "background";
        protected override string GetFilename(IBeatmap beatmap) => beatmap.Metadata.BackgroundFile;
    }
}
