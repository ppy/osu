// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public abstract class OverlayHeader : Container
    {
        private readonly Box titleBackground;
        private readonly Container background;
        protected readonly FillFlowContainer HeaderInfo;
        protected readonly OverlayRulesetSelector RulesetSelector;

        public readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

        protected Color4 TitleBackgroundColour
        {
            set => titleBackground.Colour = value;
        }

        protected float BackgroundHeight
        {
            set => background.Height = value;
        }

        protected OverlayHeader(bool hasRulesetSelector)
        {
            Container titleContainer;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Add(new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    HeaderInfo = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Depth = -float.MaxValue,
                        Children = new[]
                        {
                            background = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                Masking = true,
                                Child = CreateBackground()
                            },
                            titleContainer = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Children = new Drawable[]
                                {
                                    titleBackground = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Color4.Gray,
                                    },
                                    CreateTitle().With(title =>
                                    {
                                        title.Anchor = Anchor.CentreLeft;
                                        title.Origin = Anchor.CentreLeft;
                                        title.Margin = new MarginPadding
                                        {
                                            Vertical = 10,
                                            Left = UserProfileOverlay.CONTENT_X_MARGIN
                                        };
                                    })
                                }
                            }
                        }
                    },
                    CreateContent()
                }
            });

            if (hasRulesetSelector)
            {
                titleContainer.Add(RulesetSelector = CreateRulesetSelector().With(selector =>
                {
                    selector.Anchor = Anchor.CentreRight;
                    selector.Origin = Anchor.CentreRight;
                    selector.Margin = new MarginPadding
                    {
                        Vertical = 10,
                        Right = UserProfileOverlay.CONTENT_X_MARGIN
                    };
                }));

                Ruleset.BindTo(RulesetSelector.Current);
            }
        }

        [NotNull]
        protected virtual Drawable CreateBackground() => new Container();

        [NotNull]
        protected virtual Drawable CreateContent() => new Container();

        [NotNull]
        protected virtual OverlayRulesetSelector CreateRulesetSelector() => new OverlayRulesetSelector();

        protected abstract ScreenTitle CreateTitle();
    }
}
