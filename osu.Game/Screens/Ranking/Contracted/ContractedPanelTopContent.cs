// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Ranking.Contracted
{
    public partial class ContractedPanelTopContent : CompositeDrawable
    {
        public readonly Bindable<int?> ScorePosition = new Bindable<int?>();

        private OsuSpriteText text = null!;

        public ContractedPanelTopContent()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = text = new OsuSpriteText
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Y = 6,
                Font = OsuFont.GetFont(size: 18, weight: FontWeight.Bold)
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ScorePosition.BindValueChanged(pos => text.Text = pos.NewValue != null ? $"#{pos.NewValue}" : string.Empty, true);
        }
    }
}
