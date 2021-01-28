// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Game.Extensions;
using osu.Game.Graphics;

namespace osu.Game.Screens.Edit.Components
{
    public class TimeInfoContainer : BottomBarContainer
    {
        private readonly OsuSpriteText trackTimer;

        [Resolved]
        private EditorClock editorClock { get; set; }

        public TimeInfoContainer()
        {
            Children = new Drawable[]
            {
                trackTimer = new OsuSpriteText
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    // intentionally fudged centre to avoid movement of the number portion when
                    // going negative.
                    X = -35,
                    Font = OsuFont.GetFont(size: 25, fixedWidth: true),
                }
            };
        }

        protected override void Update()
        {
            base.Update();
            trackTimer.Text = editorClock.CurrentTime.ToEditorFormattedString();
        }
    }
}
