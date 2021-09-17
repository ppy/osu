using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Tmds.DBus;

namespace M.DBus.Services.Canonical
{
    [Dictionary]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    [SuppressMessage("ReSharper", "ConvertToConstant.Local")]
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    public class DBusMenuProperties
    {
        public uint Version
        {
            get => _Version;

            set => _Version = value;
        }

        public string TextDirection
        {
            get => _TextDirection;

            set => _TextDirection = value;
        }

        public string Status
        {
            get => _Status;

            set => _Status = value;
        }

        public string[] IconThemePath
        {
            get => _IconThemePath;

            set => _IconThemePath = value;
        }

        private uint _Version = 4;

        private string _TextDirection = "ltr";

        //normal / notice
        private string _Status = "normal";

        private string[] _IconThemePath = Array.Empty<string>();

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
