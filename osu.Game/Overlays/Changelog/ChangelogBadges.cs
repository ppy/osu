// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Online.API.Requests.Responses;
using System;
using System.Collections.Generic;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogBadges : Container
    {
        private const float container_height = 106.5f;
        private const float padding_y = 20;
        private const float padding_x = 85;

        public delegate void SelectionHandler(string updateStream, string version, EventArgs args);

        public event SelectionHandler Selected;

        private readonly FillFlowContainer<StreamBadge> badgesContainer;

        public ChangelogBadges()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(32, 24, 35, 255),
                },
                badgesContainer = new FillFlowContainer<StreamBadge>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding
                    {
                        Top = padding_y,
                        Bottom = padding_y,
                        Left = padding_x,
                        Right = padding_x,
                    },
                },
            };
            //foreach (StreamBadge streamBadge in BadgesContainer.Children)
            //{
            //    streamBadge.OnActivation = () =>
            //    {
            //        SelectedRelease = streamBadge.ChangelogEntry;
            //        foreach (StreamBadge item in BadgesContainer.Children)
            //            if (item.ChangelogEntry.Id != streamBadge.ChangelogEntry.Id)
            //                item.Deactivate();
            //        OnSelection?.Invoke();
            //    };
            //}
        }

        public void Populate(List<APIChangelog> latestBuilds)
        {
            foreach (APIChangelog updateStream in latestBuilds)
            {
                var streamBadge = new StreamBadge(updateStream);
                streamBadge.Selected += OnBadgeSelected;
                badgesContainer.Add(streamBadge);
            }
        }

        public void SelectNone()
        {
            foreach (StreamBadge streamBadge in badgesContainer)
                streamBadge.Deactivate();
        }

        public void SelectUpdateStream(string updateStream)
        {
            foreach (StreamBadge streamBadge in badgesContainer)
                if (streamBadge.ChangelogEntry.UpdateStream.Name == updateStream)
                {
                    streamBadge.Activate();
                    return;
                }
        }

        private void OnBadgeSelected(StreamBadge source, EventArgs args)
        {
            OnSelected(source);
        }

        protected virtual void OnSelected(StreamBadge source)
        {
            if (Selected != null)
                Selected(source.ChangelogEntry.UpdateStream.Name, source.ChangelogEntry.Version, EventArgs.Empty);
        }

        //protected override bool OnHover(InputState state)
        //{
        //    foreach (StreamBadge streamBadge in BadgesContainer.Children)
        //    {
        //        if (SelectedRelease != null)
        //        {
        //            if (SelectedRelease.UpdateStream.Id != streamBadge.ChangelogEntry.UpdateStream.Id)
        //                streamBadge.Deactivate();
        //            else
        //                streamBadge.EnableDim();
        //        }
        //        else
        //            streamBadge.Deactivate();
        //    }
        //    return base.OnHover(state);
        //}

        //protected override void OnHoverLost(InputState state)
        //{
        //    foreach (StreamBadge streamBadge in BadgesContainer.Children)
        //    {
        //        if (SelectedRelease == null)
        //            streamBadge.Activate(true);
        //        else if (streamBadge.ChangelogEntry.UpdateStream.Id == SelectedRelease.UpdateStream.Id)
        //            streamBadge.DisableDim();
        //    }
        //    base.OnHoverLost(state);
        //}
    }
}
