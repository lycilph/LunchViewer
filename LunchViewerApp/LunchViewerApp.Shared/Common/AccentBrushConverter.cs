using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace LunchViewerApp.Common
{
    public sealed class AccentBrushConverter : IValueConverter
    {
        private static SolidColorBrush transparent_brush = new SolidColorBrush(Colors.Transparent);

        public object Convert(object value, Type target_type, object parameter, string language)
        {
            return (bool)value ? App.Current.Resources["PhoneAccentBrush"] : transparent_brush;
        }

        public object ConvertBack(object value, Type target_type, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
