// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Framework.Testing.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Input.Bindings;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tests.Visual
{
    public abstract partial class OsuManualInputManagerTestScene : OsuTestScene
    {
        protected override Container<Drawable> Content => content;
        private readonly Container content;

        protected readonly ManualInputManager InputManager;

        private readonly Container takeControlOverlay;

        /// <summary>
        /// Whether to create a nested container to handle <see cref="GlobalAction"/>s that result from local (manual) test input.
        /// This should be disabled when instantiating an <see cref="OsuGame"/> instance else actions will be lost.
        /// </summary>
        protected virtual bool CreateNestedActionContainer => true;

        /// <summary>
        /// Whether a menu cursor controlled by the manual input manager should be displayed.
        /// True by default, but is disabled for <see cref="OsuGameTestScene"/>s as they provide their own global cursor.
        /// </summary>
        protected virtual bool DisplayCursorForManualInput => true;

        protected OsuManualInputManagerTestScene()
        {
            var mainContent = content = new Container { RelativeSizeAxes = Axes.Both };

            if (DisplayCursorForManualInput)
            {
                var cursorDisplay = new GlobalCursorDisplay { RelativeSizeAxes = Axes.Both };

                cursorDisplay.Add(content = new OsuTooltipContainer(cursorDisplay.MenuCursor)
                {
                    RelativeSizeAxes = Axes.Both,
                });

                mainContent.Add(cursorDisplay);
            }

            if (CreateNestedActionContainer)
            {
                var globalActionContainer = new GlobalActionContainer(null)
                {
                    Child = mainContent
                };
                mainContent = globalActionContainer;
            }

            base.Content.AddRange(new Drawable[]
            {
                InputManager = new ManualInputManager
                {
                    UseParentInput = true,
                    Child = mainContent
                },
                takeControlOverlay = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Margin = new MarginPadding(40),
                    CornerRadius = 5,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4.Black,
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.4f,
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Margin = new MarginPadding(10),
                            Spacing = new Vector2(10),
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Font = OsuFont.Default.With(weight: FontWeight.Bold),
                                    Text = "The test is currently overriding local input",
                                },
                                new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Spacing = new Vector2(5),
                                    Direction = FillDirection.Horizontal,

                                    Children = new Drawable[]
                                    {
                                        new RoundedButton
                                        {
                                            Text = "Take control back",
                                            Size = new Vector2(180, 30),
                                            Action = () => InputManager.UseParentInput = true
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

            takeControlOverlay.Alpha = InputManager.UseParentInput ? 0 : 1;
        }

        /// <summary>
        /// Wait for a button to become enabled, then click it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected void ClickButtonWhenEnabled<T>()
            where T : Drawable
        {
            if (typeof(T) == typeof(Button))
                AddUntilStep($"wait for {typeof(T).Name} enabled", () => (this.ChildrenOfType<T>().Single() as ClickableContainer)?.Enabled.Value == true);
            else
                AddUntilStep($"wait for {typeof(T).Name} enabled", () => this.ChildrenOfType<T>().Single().ChildrenOfType<ClickableContainer>().Single().Enabled.Value);

            AddStep($"click {typeof(T).Name}", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<T>().Single());
                InputManager.Click(MouseButton.Left);
            });
        }
    }
}
