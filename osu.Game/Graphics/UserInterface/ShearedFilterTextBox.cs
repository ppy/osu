// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public partial class ShearedFilterTextBox : ShearedSearchTextBox
    {
        private const float filter_text_size = 12;

        public LocalisableString FilterText
        {
            get => ((InnerFilterTextBox)TextBox).FilterText.Text;
            set => Schedule(() => ((InnerFilterTextBox)TextBox).FilterText.Text = value);
        }

        public ShearedFilterTextBox()
        {
            Height += filter_text_size;
        }

        protected override InnerSearchTextBox CreateInnerTextBox() => new InnerFilterTextBox();

        protected partial class InnerFilterTextBox : InnerSearchTextBox
        {
            public OsuSpriteText FilterText { get; private set; } = null!;

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                TextContainer.Add(FilterText = new OsuSpriteText
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.TopLeft,
                    Font = OsuFont.Torus.With(size: filter_text_size, weight: FontWeight.SemiBold),
                    Margin = new MarginPadding { Top = 2, Left = -1 },
                    Colour = colours.Yellow
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                TextContainer.Height *= (DrawHeight - filter_text_size) / DrawHeight;
                TextContainer.Margin = new MarginPadding { Bottom = filter_text_size };
            }
        }
    }
}
