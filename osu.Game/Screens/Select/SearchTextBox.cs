using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics;

namespace osu.Game.Screens.Select
{
    public class SearchTextBox : TextBox
    {
        protected override Color4 BackgroundUnfocused => new Color4(10, 10, 10, 255);
        protected override Color4 BackgroundFocused => new Color4(10, 10, 10, 255);
        public bool GrabFocus = false;
        
        private SpriteText placeholder;

        protected override string InternalText
        {
            get { return base.InternalText; }
            set
            {
                base.InternalText = value;
                if (placeholder != null)
                {
                    if (string.IsNullOrEmpty(value))
                        placeholder.Text = "type to search";
                    else
                        placeholder.Text = string.Empty;
                }
            }
        }
    
        public SearchTextBox()
        {
            Height = 35;
            TextContainer.Padding = new MarginPadding(5);
            Add(new Drawable[]
            {
                placeholder = new SpriteText
                {
                    Font = @"Exo2.0-MediumItalic",
                    Text = "type to search",
                    Colour = new Color4(180, 180, 180, 255),
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Margin = new MarginPadding { Left = 10 },
                },
                new TextAwesome
                {
                    Icon = FontAwesome.fa_search,
                    Origin = Anchor.CentreRight,
                    Anchor = Anchor.CentreRight,
                    Margin = new MarginPadding { Right = 10 },
                }
            });
        }
        
        protected override void LoadComplete()
        {
            base.LoadComplete();
        }
        
        protected override void Update()
        {
            if (GrabFocus && !HasFocus && IsVisible)
            {
                var inputManager = Parent;
                while (inputManager != null && !(inputManager is InputManager))
                    inputManager = inputManager.Parent;
                if (inputManager != null && (inputManager as InputManager)?.FocusedDrawable == null)
                    TriggerFocus();
            }
            base.Update();
        }
        
        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Key == Key.Left || args.Key == Key.Right
                    || args.Key == Key.Enter || args.Key == Key.Escape)
                return false;
            return base.OnKeyDown(state, args);
        }
    }
}