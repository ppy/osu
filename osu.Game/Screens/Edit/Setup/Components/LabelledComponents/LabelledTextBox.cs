// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Setup.Components.LabelledComponents
{
    public class LabelledTextBox : LabelledComponent
    {
        public event TextBox.OnCommitHandler OnCommit;

        public bool ReadOnly
        {
            set => Component.ReadOnly = value;
        }

        public string PlaceholderText
        {
            set => Component.PlaceholderText = value;
        }

        public string Text
        {
            set => Component.Text = value;
        }

        protected new OsuTextBox Component => (OsuTextBox)base.Component;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Component.BorderColour = colours.Blue;
        }

        protected override bool ExpandComponent => false;

        protected override Drawable CreateComponent() => new OsuTextBox
        {
            Anchor = Anchor.TopLeft,
            Origin = Anchor.TopLeft,
            RelativeSizeAxes = Axes.X,
            CornerRadius = CORNER_RADIUS,
        }.With(t => t.OnCommit += (sender, newText) => OnCommit?.Invoke(sender, newText));
    }
}
