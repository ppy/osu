// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Screens.Play;
using OpenTK;

namespace osu.Game.Rulesets.Osu.UI.Overlays
{
    public class OsuResumeOverlay : PauseContainer.ResumeOverlay
    {
        public OsuResumeOverlay(Action resumeAction, Action escAction)
            : base(resumeAction, escAction)
        {
        }

        public override string Header => "Click the orange cursor to resume";
        public override string Description => string.Empty;

        public override void SetResumeButtonPosition(Vector2 newPosition)
        {
            throw new NotImplementedException();
        }
    }
}
