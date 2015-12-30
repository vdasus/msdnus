using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace msdnus.Shared
{
    public class InverseBoolToVisibleConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType != typeof(Visibility))
                throw new InvalidOperationException("The target must be a Visibility");

            return (bool) value ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
        #endregion
    }
}