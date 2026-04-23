using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TuneFlow.UI
{
    /// <summary>
    /// Преобразует наличие ID трека в списке лайков в цвет фона строки.
    /// </summary>
    public class PlaylistColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is int trackId && values[1] is List<int> likedIds)
            {
                // Если трек в плейлисте — подсвечиваем мягким зеленым цветом
                return likedIds.Contains(trackId)
                    ? new SolidColorBrush(Color.FromArgb(40, 52, 199, 89))
                    : Brushes.Transparent;
            }
            return Brushes.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}