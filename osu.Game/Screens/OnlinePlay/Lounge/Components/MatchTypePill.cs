// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public class MatchTypePill : OnlinePlayComposite
    {
        private OsuTextFlowContainer textFlow;

        public MatchTypePill()
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
                    AutoSizeAxes = Axes.Both,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Type.BindValueChanged(onMatchTypeChanged, true);
        }

        private void onMatchTypeChanged(ValueChangedEvent<MatchType> type)
        {
            textFlow.Clear();
            textFlow.AddText(type.NewValue.GetLocalisableDescription());
        }
    }
}
