// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Rulesets.Edit
{
    internal partial class ExpandableButton : RoundedButton, IExpandable
    {
        private float actualHeight;

        public override float Height
        {
            get => base.Height;
            set => base.Height = actualHeight = value;
        }

        private LocalisableString contractedLabelText;

        /// <summary>
        /// The label text to display when this button is in a contracted state.
        /// </summary>
        public LocalisableString ContractedLabelText
        {
            get => contractedLabelText;
            set
            {
                if (value == contractedLabelText)
                    return;

                contractedLabelText = value;

                if (!Expanded.Value)
                    Text = value;
            }
        }

        private LocalisableString expandedLabelText;

        /// <summary>
        /// The label text to display when this button is in an expanded state.
        /// </summary>
        public LocalisableString ExpandedLabelText
        {
            get => expandedLabelText;
            set
            {
                if (value == expandedLabelText)
                    return;

                expandedLabelText = value;

                if (Expanded.Value)
                    Text = value;
            }
        }

        public BindableBool Expanded { get; } = new BindableBool();

        [Resolved(canBeNull: true)]
        private IExpandingContainer? expandingContainer { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            expandingContainer?.Expanded.BindValueChanged(containerExpanded =>
            {
                Expanded.Value = containerExpanded.NewValue;
            }, true);

            Expanded.BindValueChanged(expanded =>
            {
                Text = expanded.NewValue ? expandedLabelText : contractedLabelText;

                if (expanded.NewValue)
                {
                    SpriteText.Anchor = Anchor.Centre;
                    SpriteText.Origin = Anchor.Centre;
                    SpriteText.Font = OsuFont.GetFont(weight: FontWeight.Bold);
                    base.Height = actualHeight;
                    Background.Show();
                    Triangles?.Show();
                }
                else
                {
                    SpriteText.Anchor = Anchor.CentreLeft;
                    SpriteText.Origin = Anchor.CentreLeft;
                    SpriteText.Font = OsuFont.GetFont(weight: FontWeight.Regular);
                    base.Height = actualHeight / 2;
                    Background.Hide();
                    Triangles?.Hide();
                }
            }, true);
        }
    }
}
