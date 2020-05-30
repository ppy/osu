// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osuTK;
using osu.Game.Graphics.Sprites;
using osu.Framework.Threading;
using osu.Framework.Extensions;

namespace osu.Game.Overlays.MfMenu
{
    public class MfMenuTexts : MfMenuContent
    {
        private MfMenuIntroduceSection IntroduceSection;
        private MfMenuFaqSection FaqSection;
        private FillFlowContainer baseContainer;
        private OsuSpriteText subTitle;

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = baseContainer = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Spacing = new Vector2(0, 20),
                Margin = new MarginPadding{ Top = 20, Bottom = 50 },
                Children = new Drawable[]
                {
                    subTitle = new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Font = OsuFont.GetFont(size: 30),
                        Alpha = 1
                    },
                    IntroduceSection = new MfMenuIntroduceSection(),
                    FaqSection = new MfMenuFaqSection(),
                }
            };
        }

        #region 功能函数

        private ScheduledDelegate scheduledChangeContent;
        public void UpdateContent(SelectedTabType tabType)
        {
            scheduledChangeContent?.Cancel();
            scheduledChangeContent = null;

            foreach (var i in baseContainer)
            {
                i.FadeOut(300, Easing.OutQuint);
            }

            scheduledChangeContent = Scheduler.AddDelayed( () =>
            {
                subTitle.Text = tabType.GetDescription() ?? tabType.ToString();

                switch (tabType)
                {
                    case SelectedTabType.Introduce:
                        IntroduceSection.FadeIn(400, Easing.OutQuint);
                        break;

                    case SelectedTabType.Faq:
                        FaqSection.FadeIn(400, Easing.OutQuint);
                        break;
                }

                subTitle.FadeIn(400, Easing.OutQuint);
            } , 300);
        }

        #endregion 功能函数
    }
}
