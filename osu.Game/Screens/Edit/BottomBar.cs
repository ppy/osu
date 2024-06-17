// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Testing;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Components;
using osu.Game.Screens.Edit.Components.Timelines.Summary;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit
{
    internal partial class BottomBar : CompositeDrawable
    {
        public TestGameplayButton TestGameplayButton { get; private set; } = null!;

        private IBindable<bool> saveInProgress = null!;
        private Bindable<bool> composerFocusMode = null!;

        [BackgroundDependencyLoader]
        private void load(Editor editor)
        {
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;

            RelativeSizeAxes = Axes.X;

            Height = 60;

            Masking = true;
            EdgeEffect = new EdgeEffectParameters
            {
                Colour = Color4.Black.Opacity(0.2f),
                Type = EdgeEffectType.Shadow,
                Radius = 10f,
            };

            InternalChildren = new Drawable[]
            {
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 170),
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 220),
                        new Dimension(GridSizeMode.Absolute, HitObjectComposer.TOOLBOX_CONTRACTED_SIZE_RIGHT),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new TimeInfoContainer { RelativeSizeAxes = Axes.Both },
                            new SummaryTimeline { RelativeSizeAxes = Axes.Both },
                            new PlaybackControl { RelativeSizeAxes = Axes.Both },
                            TestGameplayButton = new TestGameplayButton
                            {
                                RelativeSizeAxes = Axes.Both,
                                Size = new Vector2(1),
                                Action = editor.TestGameplay,
                            }
                        },
                    }
                }
            };

            saveInProgress = editor.MutationTracker.InProgress.GetBoundCopy();
            composerFocusMode = editor.ComposerFocusMode.GetBoundCopy();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            saveInProgress.BindValueChanged(_ => TestGameplayButton.Enabled.Value = !saveInProgress.Value, true);
            composerFocusMode.BindValueChanged(_ =>
            {
                float targetAlpha = composerFocusMode.Value ? 0.5f : 1;

                foreach (var c in this.ChildrenOfType<BottomBarContainer>())
                    c.Background.FadeTo(targetAlpha, 400, Easing.OutQuint);
            }, true);
        }
    }
}
