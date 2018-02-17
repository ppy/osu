// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Screens.Play;
using OpenTK;

namespace osu.Game.Rulesets.Osu.UI.Overlays
{
    public class OsuResumeOverlay : PauseContainer.ResumeOverlay
    {
        private readonly PassThroughInputManager rulesetInputManager;
        private GameplayCursor.OsuClickToResumeCursor clickToResumeCursor;

        public OsuResumeOverlay(PassThroughInputManager rulesetInputManager, Action resumeAction, Action escAction)
            : base(resumeAction, escAction)
        {
            this.rulesetInputManager = rulesetInputManager;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            rulesetInputManager.Add(clickToResumeCursor = new GameplayCursor.OsuClickToResumeCursor(ResumeAction));
            Add(rulesetInputManager);
        }

        public override string Header => "Click the orange cursor to resume";
        public override string Description => string.Empty;

        public override void SetResumeButtonPosition(Vector2 newPosition) => clickToResumeCursor.MoveTo(newPosition);
    }
}
