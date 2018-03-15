// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Osu.UI.Overlays
{
    public class OsuResumeOverlay : PauseContainer.ResumeOverlay
    {
        private GameplayCursor.OsuClickToResumeCursor clickToResumeCursor;
        private bool needToMoveCursor = true;

        public OsuResumeOverlay(PassThroughInputManager rulesetInputManager, CursorContainer cursor, Action resumeAction, Action escAction)
            : base(rulesetInputManager, cursor, resumeAction, escAction)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            RulesetInputManager.Add(clickToResumeCursor = new GameplayCursor.OsuClickToResumeCursor(() =>
            {
                needToMoveCursor = true;
                ResumeAction?.Invoke();
            }));
            Add(RulesetInputManager);
        }

        public override string Header => "Click the orange cursor to resume";
        public override string Description => string.Empty;

        public override void Show()
        {
            if (Cursor != null && needToMoveCursor)
            {
                clickToResumeCursor.MoveTo(Cursor.ActiveCursor.Position);
                needToMoveCursor = false;
            }

            base.Show();
        }
    }
}
