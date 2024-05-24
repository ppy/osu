// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// An overlay which can be used to require further user actions before gameplay is resumed.
    /// </summary>
    public abstract partial class ResumeOverlay : VisibilityContainer
    {
        public GameplayCursorContainer GameplayCursor { get; set; }

        /// <summary>
        /// The action to be performed to complete resuming.
        /// </summary>
        public Action ResumeAction { private get; set; }

        public virtual CursorContainer LocalCursor => null;

        protected const float TRANSITION_TIME = 500;

        protected abstract LocalisableString Message { get; }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

        protected ResumeOverlay()
        {
            RelativeSizeAxes = Axes.Both;
        }

        protected void Resume()
        {
            ResumeAction?.Invoke();
            Hide();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AddRange(new Drawable[]
            {
                new OsuSpriteText
                {
                    RelativePositionAxes = Axes.Both,
                    Y = 0.4f,
                    Text = Message,
                    Font = OsuFont.GetFont(size: 30),
                    Spacing = new Vector2(5, 0),
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Colour = colours.Yellow,
                    Shadow = true,
                    ShadowColour = new Color4(0, 0, 0, 0.25f)
                }
            });
        }

        protected override void PopIn() => this.FadeIn(TRANSITION_TIME, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(TRANSITION_TIME, Easing.OutQuint);
    }
}
