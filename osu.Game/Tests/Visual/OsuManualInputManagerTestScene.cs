// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing.Input;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public abstract class OsuManualInputManagerTestScene : OsuTestScene
    {
        protected override Container<Drawable> Content => content;
        private readonly Container content;

        protected readonly ManualInputManager InputManager;

        private readonly TriangleButton buttonTest;
        private readonly TriangleButton buttonLocal;

        /// <summary>
        /// Whether to create a nested container to handle <see cref="GlobalAction"/>s that result from local (manual) test input.
        /// This should be disabled when instantiating an <see cref="OsuGame"/> instance else actions will be lost.
        /// </summary>
        protected virtual bool CreateNestedActionContainer => true;

        protected OsuManualInputManagerTestScene()
        {
            MenuCursorContainer cursorContainer;

            CompositeDrawable mainContent = cursorContainer = new MenuCursorContainer { RelativeSizeAxes = Axes.Both };

            cursorContainer.Child = content = new OsuTooltipContainer(cursorContainer.Cursor)
            {
                RelativeSizeAxes = Axes.Both
            };

            if (CreateNestedActionContainer)
            {
                mainContent = new GlobalActionContainer(null).WithChild(mainContent);
            }

            base.Content.AddRange(new Drawable[]
            {
                InputManager = new ManualInputManager
                {
                    UseParentInput = true,
                    Child = mainContent
                },
                new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Margin = new MarginPadding(5),
                    CornerRadius = 5,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4.Black,
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.5f,
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Margin = new MarginPadding(5),
                            Spacing = new Vector2(5),
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Text = "Input Priority"
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Margin = new MarginPadding(5),
                                    Spacing = new Vector2(5),
                                    Direction = FillDirection.Horizontal,

                                    Children = new Drawable[]
                                    {
                                        buttonLocal = new TriangleButton
                                        {
                                            Text = "local",
                                            Size = new Vector2(50, 30),
                                            Action = returnUserInput
                                        },
                                        buttonTest = new TriangleButton
                                        {
                                            Text = "test",
                                            Size = new Vector2(50, 30),
                                            Action = returnTestInput
                                        },
                                    }
                                },
                            }
                        },
                    }
                },
            });
        }

        protected override void Update()
        {
            base.Update();

            buttonTest.Enabled.Value = InputManager.UseParentInput;
            buttonLocal.Enabled.Value = !InputManager.UseParentInput;
        }

        private void returnUserInput() =>
            InputManager.UseParentInput = true;

        private void returnTestInput() =>
            InputManager.UseParentInput = false;
    }
}
