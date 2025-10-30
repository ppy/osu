// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Matchmaking.Events;
using osu.Game.Online.Multiplayer;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.Match.BeatmapSelect
{
    public partial class MatchmakingCursorContainer : CompositeDrawable
    {
        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        private Vector2? latestMousePosition;

        private readonly Dictionary<int, Cursor> cursorLookup = new Dictionary<int, Cursor>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.UserJoined += onUserJoined;
            client.UserLeft += onUserLeft;
            client.MatchEvent += onMatchEvent;

            if (client.Room != null)
            {
                foreach (var user in client.Room.Users)
                    onUserJoined(user);
            }

            Scheduler.AddDelayed(updateMousePosition, 50, repeat: true);
        }

        private void onMatchEvent(MatchServerEvent evt)
        {
            if (evt is MatchmakingCursorPositionEvent cursorEvent)
                cursorLookup.GetValueOrDefault(cursorEvent.UserId)?.OnCursorPositionReceived(new Vector2(cursorEvent.X, cursorEvent.Y));
        }

        private void updateMousePosition()
        {
            if (latestMousePosition is { } pos)
            {
                client.SendMatchRequest(new MatchmakingCursorPositionRequest
                {
                    X = pos.X,
                    Y = pos.Y,
                });

                latestMousePosition = null;
            }
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            latestMousePosition = e.MousePosition;

            return false;
        }

        private void onUserJoined(MultiplayerRoomUser user)
        {
            if (user.UserID == api.LocalUser.Value?.Id)
                return;

            if (cursorLookup.ContainsKey(user.UserID))
                return;

            var cursor = new Cursor(user.User!);
            cursorLookup[user.UserID] = cursor;
            AddInternal(cursor);
        }

        private void onUserLeft(MultiplayerRoomUser user)
        {
            if (cursorLookup.Remove(user.UserID, out var cursor))
                cursor.Expire();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (client.IsNotNull())
            {
                client.UserJoined -= onUserJoined;
                client.UserLeft -= onUserLeft;
                client.MatchEvent -= onMatchEvent;
            }

            base.Dispose(isDisposing);
        }

        private partial class Cursor : CompositeDrawable
        {
            private float targetAlpha = 0;

            private readonly APIUser user;

            public Cursor(APIUser user)
            {
                this.user = user;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Alpha = 0;
                AutoSizeAxes = Axes.Both;
                AlwaysPresent = true;

                InternalChildren = new Drawable[]
                {
                    new Sprite
                    {
                        Texture = textures.Get(@"Cursor/menu-cursor"),
                        Scale = new Vector2(0.1f)
                    },
                    new CircularContainer
                    {
                        Position = new Vector2(20, -15),
                        Size = new Vector2(20),
                        Masking = true,
                        Child = new UpdateableAvatar(user)
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    }
                };
            }

            public void OnCursorPositionReceived(Vector2 position)
            {
                ClearTransforms(targetMember: nameof(targetAlpha));

                this.MoveTo(position, 100, Easing.Out);

                this.TransformTo(nameof(targetAlpha), 1f, 100)
                    .Delay(1000)
                    .TransformTo(nameof(targetAlpha), 0f, 4000);
            }

            protected override void Update()
            {
                base.Update();

                const float fade_distance = 50f;

                var parentRect = new RectangleF(Parent!.ChildOffset, Parent!.ChildSize).Shrink(fade_distance * 2);

                var position = DrawPosition;

                float distance = 0;

                if (position.X < parentRect.Left)
                    distance = Math.Max(distance, parentRect.Left - position.X);
                if (position.Y < parentRect.Top)
                    distance = Math.Max(distance, parentRect.Top - position.Y);
                if (position.X > parentRect.Right)
                    distance = Math.Max(distance, position.X - parentRect.Right);
                if (position.Y > parentRect.Bottom)
                    distance = Math.Max(distance, position.Y - parentRect.Bottom);

                Alpha = float.Clamp(1 - distance / fade_distance, 0, 1);
            }
        }
    }
}
