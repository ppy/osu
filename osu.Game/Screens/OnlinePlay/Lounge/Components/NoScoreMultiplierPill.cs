// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public partial class NoScoreMultiplierPill : OnlinePlayComposite
    {
        private OsuTextFlowContainer textFlow;

        public NoScoreMultiplierPill()
        {
            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new PillContainer
            {
                Child = textFlow = new OsuTextFlowContainer(s => s.Font = OsuFont.GetFont(size: 12))
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Both
                }
            };
            textFlow.AddText("No mod multipliers");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            NoScoreMultiplier.BindValueChanged(onNoScoreMultiplierChanged, true);
        }

        private void onNoScoreMultiplierChanged(ValueChangedEvent<bool> mode)
        {
            if (mode.NewValue)
                Show();
            else
                Hide();
        }
    }
}
