using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace msdnus.Shared
{
    public class MagicAttribute : Attribute { }

    public class NoMagicAttribute : Attribute { }

    // Legacy alias for compatibility - inherits from modern ObservableObject
    [Magic]
    public class PropertyChangedBase : ObservableObject
    {
        // ObservableObject already provides INotifyPropertyChanged implementation
        // This class now serves as a compatibility wrapper
    }
}