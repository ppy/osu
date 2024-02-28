// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public partial class SettingsTeamDropdown : SettingsDropdown<TournamentTeam?>
    {
        public SettingsTeamDropdown(BindableList<TournamentTeam> teams)
        {
            foreach (var t in teams.Prepend(new TournamentTeam()))
                add(t);

            teams.CollectionChanged += (_, args) =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        Debug.Assert(args.NewItems != null);

                        args.NewItems.Cast<TournamentTeam>().ForEach(add);
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        Debug.Assert(args.OldItems != null);

                        args.OldItems.Cast<TournamentTeam>().ForEach(i => Control.RemoveDropdownItem(i));
                        break;
                }
            };
        }

        private readonly List<IUnbindable> refBindables = new List<IUnbindable>();

        private T boundReference<T>(T obj)
            where T : IBindable
        {
            obj = (T)obj.GetBoundCopy();
            refBindables.Add(obj);
            return obj;
        }

        private void add(TournamentTeam team)
        {
            Control.AddDropdownItem(team);
            boundReference(team.FullName).BindValueChanged(_ =>
            {
                Control.RemoveDropdownItem(team);
                Control.AddDropdownItem(team);
            });
        }
    }
}
