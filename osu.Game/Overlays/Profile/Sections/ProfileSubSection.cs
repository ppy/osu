// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Profile.Sections
{
    public abstract class ProfileSubSection : FillFlowContainer
    {
        protected readonly OsuSpriteText MissingText;
        protected readonly Bindable<User> User = new Bindable<User>();

        protected override Container<Drawable> Content => content;

        private readonly Container content;

        protected ProfileSubSection(Bindable<User> user, string header, string missing)
        {
            User.BindTo(user);

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Margin = new MarginPadding { Vertical = 10 };
            Spacing = new Vector2(0, 10);

            InternalChildren = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = header,
                    Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold),
                },
                content = new Container
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                },
                MissingText = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 15),
                    Text = missing,
                    Alpha = 0,
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            User.ValueChanged += OnUserChanged;
            User.TriggerChange();
        }

        protected abstract void OnUserChanged(ValueChangedEvent<User> user);
    }
}
