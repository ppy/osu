// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game;
using osu.Game.Graphics;
using osu.Game.Overlays.Notifications;
using osuTK;
using osuTK.Graphics;

namespace osu.Desktop.Updater
{
    public abstract class AbstractUpdateManager : Game.Updater.UpdateManager
    {
        public abstract Task PrepareUpdateAsync();

        protected class UpdateCompleteNotification : ProgressCompletionNotification
        {
            [Resolved]
            private OsuGame game { get; set; } = null!;

            public UpdateCompleteNotification(AbstractUpdateManager updateManager)
            {
                Text = @"Update ready to install. Click to restart!";

                Activated = () =>
                {
                    updateManager.PrepareUpdateAsync()
                                 .ContinueWith(_ => updateManager.Schedule(() => game.AttemptExit()));
                    return true;
                };
            }
        }

        protected class UpdateProgressNotification : ProgressNotification
        {
            private readonly AbstractUpdateManager updateManager;

            public UpdateProgressNotification(AbstractUpdateManager updateManager)
            {
                this.updateManager = updateManager;
            }

            protected override Notification CreateCompletionNotification()
            {
                return new UpdateCompleteNotification(updateManager);
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                IconContent.AddRange(new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = ColourInfo.GradientVertical(colours.YellowDark, colours.Yellow)
                    },
                    new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Icon = FontAwesome.Solid.Upload,
                        Colour = Color4.White,
                        Size = new Vector2(20),
                    }
                });
            }

            public override void Close()
            {
                // cancelling updates is not currently supported by the underlying updater.
                // only allow dismissing for now.

                switch (State)
                {
                    case ProgressNotificationState.Cancelled:
                        base.Close();
                        break;
                }
            }
        }
    }
}
