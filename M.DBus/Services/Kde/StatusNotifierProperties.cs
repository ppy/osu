using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using M.DBus.Tray;
using Tmds.DBus;

namespace M.DBus.Services.Kde
{
    [Dictionary]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    [SuppressMessage("ReSharper", "ConvertToConstant.Local")]
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    public class StatusNotifierProperties
    {
        public string Category
        {
            get => _Category;

            set => _Category = value;
        }

        public string Id
        {
            get => _Id;

            set => _Id = value;
        }

        public string Title
        {
            get => _Title;

            set => _Title = value;
        }

        public string Status
        {
            get => _Status;

            set => _Status = value;
        }

        public int WindowId
        {
            get => _WindowId;

            set => _WindowId = value;
        }

        public string IconThemePath
        {
            get => _IconThemePath;

            set => _IconThemePath = value;
        }

        public ObjectPath Menu
        {
            get => _Menu;

            set => _Menu = value;
        }

        public bool ItemIsMenu
        {
            get => _ItemIsMenu;

            set => _ItemIsMenu = value;
        }

        public string IconName
        {
            get => _IconName;

            set => _IconName = value;
        }

        public (int, int, byte[])[] IconPixmap
        {
            get => _IconPixmap;

            set => _IconPixmap = value;
        }

        public string OverlayIconName
        {
            get => _OverlayIconName;

            set => _OverlayIconName = value;
        }

        public (int, int, byte[])[] OverlayIconPixmap
        {
            get => _OverlayIconPixmap;

            set => _OverlayIconPixmap = value;
        }

        public string AttentionIconName
        {
            get => _AttentionIconName;

            set => _AttentionIconName = value;
        }

        public (int, int, byte[])[] AttentionIconPixmap
        {
            get => _AttentionIconPixmap;

            set => _AttentionIconPixmap = value;
        }

        public string AttentionMovieName
        {
            get => _AttentionMovieName;

            set => _AttentionMovieName = value;
        }

        public (string, (int, int, byte[])[], string, string) ToolTip
        {
            get => _ToolTip;

            set => _ToolTip = value;
        }

        private string _Category = "ApplicationStatus";

        private string _Id = "mfosu";

        private string _Title = "mfosu";

        private string _Status = "osu";

        private int _WindowId;

        private string _IconThemePath = string.Empty;

        private ObjectPath _Menu;

        private bool _ItemIsMenu;

        private string _IconName = string.Empty;

        private (int, int, byte[])[] _IconPixmap =
        {
            (1, 1, SimpleEntry.EmptyPngBytes)
        };

        private string _OverlayIconName = string.Empty;

        private (int, int, byte[])[] _OverlayIconPixmap = Array.Empty<(int, int, byte[])>();

        private string _AttentionIconName = string.Empty;

        private (int, int, byte[])[] _AttentionIconPixmap = Array.Empty<(int, int, byte[])>();

        private string _AttentionMovieName = string.Empty;

        private (string, (int, int, byte[])[], string, string) _ToolTip
            = (string.Empty, Array.Empty<(int, int, byte[])>(), "mfosu", string.Empty);

        private IDictionary<string, object> members;

        public object Get(string prop)
        {
            ServiceUtils.CheckIfDirectoryNotReady(this, members, out members);
            return ServiceUtils.GetValueFor(this, prop, members);
        }

        public bool Set(string name, object newValue)
        {
            ServiceUtils.CheckIfDirectoryNotReady(this, members, out members);
            return ServiceUtils.SetValueFor(this, name, newValue, members);
        }

        public bool Contains(string prop)
        {
            ServiceUtils.CheckIfDirectoryNotReady(this, members, out members);
            return ServiceUtils.CheckifContained(this, prop, members);
        }
    }
}
