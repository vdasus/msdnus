using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace msdnus.Shared
{
    public class MagicAttribute : Attribute { }

    public class NoMagicAttribute : Attribute { }

    [Magic]
    public class PropertyChangedBase : INotifyPropertyChanged
    {
        protected virtual void RaisePropertyChanged([CallerMemberName] string property = "")
        {
            Contract.Requires(property != null);
            var e = PropertyChanged;
            e?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}