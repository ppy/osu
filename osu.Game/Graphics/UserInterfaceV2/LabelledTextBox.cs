// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Graphics.UserInterfaceV2
{
    public class LabelledTextBox : LabelledComponent<OsuTextBox, string>
    {
        public event TextBox.OnCommitHandler OnCommit;

        public LabelledTextBox()
            : base(false)
        {
        }

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

        public Container TabbableContentContainer
        {
            set => Component.TabbableContentContainer = value;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Component.BorderColour = colours.Blue;
        }

        protected virtual OsuTextBox CreateTextBox() => new OsuTextBox
        {
            CommitOnFocusLost = true,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.X,
            CornerRadius = CORNER_RADIUS,
        };

        protected override OsuTextBox CreateComponent() => CreateTextBox().With(t =>
        {
            t.OnCommit += (sender, newText) => OnCommit?.Invoke(sender, newText);
        });
    }
}
