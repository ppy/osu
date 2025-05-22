// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Play.PlayerSettings;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class PlayerSettingsOverlay : ExpandingContainer
    {
        public VisualSettings VisualSettings { get; private set; }

        private const float padding = 10;

        public const float EXPANDED_WIDTH = player_settings_width + padding * 2;

        private const float player_settings_width = 270;

        private const int fade_duration = 200;

        public override void Show() => this.FadeIn(fade_duration);
        public override void Hide() => this.FadeOut(fade_duration);

        // we'll handle this ourselves because we have slightly custom logic.
        protected override bool ExpandOnHover => false;

        protected override Container<Drawable> Content => content;

        private readonly FillFlowContainer content;

        private readonly IconButton button;

        private InputManager inputManager = null!;

        [Resolved]
        private HUDOverlay? hudOverlay { get; set; }

        public PlayerSettingsOverlay()
            : base(0, EXPANDED_WIDTH)
        {
            Origin = Anchor.TopRight;
            Anchor = Anchor.TopRight;

            base.Content.Add(content = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 20),
                Margin = new MarginPadding(padding),
                Children = new PlayerSettingsGroup[]
                {
                    VisualSettings = new VisualSettings { Expanded = { Value = false } },
                    new AudioSettings { Expanded = { Value = false } }
                }
            });

            // For future consideration, this icon should probably not exist.
            //
            // If we remove it, the following needs attention:
            // - Mobile support (swipe from side of screen?)
            // - Consolidating this overlay with the one at player loader (to have the animation hint at its presence)
            AddInternal(button = new IconButton
            {
                Icon = FontAwesome.Solid.Cog,
                Origin = Anchor.TopRight,
                Anchor = Anchor.TopLeft,
                Margin = new MarginPadding(5),
                Action = () => Expanded.Toggle()
            });

            AddInternal(new Box
            {
                Colour = ColourInfo.GradientHorizontal(Color4.Black.Opacity(0), Color4.Black.Opacity(0.8f)),
                Depth = float.MaxValue,
                RelativeSizeAxes = Axes.Both,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager()!;
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            screenSpacePos.X > button.ScreenSpaceDrawQuad.TopLeft.X;

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            checkExpanded();
            return base.OnMouseMove(e);
        }

        protected override void Update()
        {
            base.Update();

            if (hudOverlay != null)
                button.Y = ToLocalSpace(hudOverlay.TopRightElements.ScreenSpaceDrawQuad.BottomRight).Y;

            // Only check expanded if already expanded.
            // This is because if we are always checking, it would bypass blocking overlays.
            // Case in point: the skin editor overlay blocks input from reaching the player, but checking raw coordinates would make settings pop out.
            if (Expanded.Value)
                checkExpanded();
        }

        private void checkExpanded()
        {
            float screenMouseX = inputManager.CurrentState.Mouse.Position.X;

            Expanded.Value =
                (screenMouseX >= button.ScreenSpaceDrawQuad.TopLeft.X && screenMouseX <= ToScreenSpace(new Vector2(DrawWidth + EXPANDED_WIDTH, 0)).X)
                // Stay expanded if the user is dragging a slider.
                || inputManager.DraggedDrawable != null;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            // handle un-expanding manually because our children do weird hover blocking stuff.
        }

        public void AddAtStart(PlayerSettingsGroup drawable) => content.Insert(-1, drawable);
    }
}
