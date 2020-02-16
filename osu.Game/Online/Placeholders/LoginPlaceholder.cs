// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Overlays;

namespace osu.Game.Online.Placeholders
{
    public sealed class LoginPlaceholder : Placeholder
    {
        [Resolved(CanBeNull = true)]
        private LoginOverlay login { get; set; }

        /// <summary>
        /// Usage: LoginPlaceholder => @"text";
        ///
        /// Example 1: 
        ///
        ///     Code : protected override string LoginPlaceholder => @"see leaderboard";\
        ///     Display : Please login to see leaderboard !
        ///
        /// Example 2:
        ///
        ///     Code : protected override string LoginPlaceholder => @""; //Use with a null or Empty string\
        ///     Display : Please login to check out this thing !
        /// </summary>
        public LoginPlaceholder(string actionMessage)
        {
            AddIcon(FontAwesome.Solid.UserLock, cp =>
            {
                cp.Font = cp.Font.With(size: TEXT_SIZE);
                cp.Padding = new MarginPadding { Right = 10 };
            });

            AddText("请登入来" + (string.IsNullOrEmpty(actionMessage) ? "浏览该项目" : actionMessage) + "!" );
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            this.ScaleTo(0.8f, 4000, Easing.OutQuint);
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            this.ScaleTo(1, 1000, Easing.OutElastic);
            base.OnMouseUp(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            login?.Show();
            return base.OnClick(e);
        }
    }
}