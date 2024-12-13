// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        protected override void Update()
        {
            base.Update();

            Expanded.Value = inputManager.CurrentState.Mouse.Position.X >= button.ScreenSpaceDrawQuad.TopLeft.X;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            // handle un-expanding manually because our children do weird hover blocking stuff.
        }

        public void AddAtStart(PlayerSettingsGroup drawable) => content.Insert(-1, drawable);
    }
}
