// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Osu.UI
{
    public class OsuResumeOverlay : ResumeOverlay
    {
        private GameplayCursor.OsuClickToResumeCursor clickToResumeCursor;
        private PassThroughInputManager inputManager;
        public override string Header => "Click the orange cursor to resume";
        public override string Description => string.Empty;

        public override PassThroughInputManager InputManager
        {
            get => inputManager;
            set
            {
                inputManager = value;
                inputManager.Add(clickToResumeCursor = new GameplayCursor.OsuClickToResumeCursor(ResumeAction));
                Add(inputManager);
            }
        }

        public override void Show()
        {
            if (Cursor != null)
                clickToResumeCursor.MoveTo(Cursor.ActiveCursor.Position);

            base.Show();
        }
    }
}
