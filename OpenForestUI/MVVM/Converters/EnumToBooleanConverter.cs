using System;
using System.Globalization;
using System.Windows.Data;

namespace OpenForestUI.MVVM.Converters
{
    /// <summary>
    /// Maps an enum value to bool by comparing against a ConverterParameter, and back. Used to bind a
    /// sidebar RadioButton's IsChecked to <c>INavigationService.CurrentRoute</c>:
    /// <c>IsChecked="{Binding Nav.CurrentRoute, Converter={StaticResource EnumToBool},
    /// ConverterParameter={x:Static core:AppRoute.Ingame}}"</c>. ConvertBack returns the parameter only
    /// when the button becomes checked, so unchecking doesn't push a stale route.
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || !(value is bool b) || !b)
                return Binding.DoNothing;

            return parameter;
        }
    }
}
