using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace VMix
{
    public static class ExtensionMethods
    {
        public static double GetLength(this Point point)
        {
            return Math.Sqrt(point.X * point.X + point.Y * point.Y);
        }

        //Extension method for IEnumerable
        public static bool IsSameValue<T, U>(this ICollection<T> list, Func<T, U> selector)
        {
            return list.Select(selector).Distinct().Count() == 1;
        }

        public static bool IsSameValue<T, U, G>(this ICollection<T> list, Func<T, U> selector) where U : ICollection<G>
        {
            return list.Select(selector).Select(x => (x).GetContentsHashCode()).Distinct().Count() == 1;
        }

        // Extension method for IEnumerable
        public static int GetContentsHashCode<T>(this ICollection<T> list)
        {
            return list.Aggregate(17, (total, next) => total * 31 + next.GetHashCode());
        }

        //This is beyond dodgy
        public static List<PropertyInfo> GetPropertiesBindableRecursive(this Type t)
        {
            List<PropertyInfo> props = t.GetProperties().Where(x => typeof(INotifyPropertyChanged).IsAssignableFrom(x.PropertyType)).ToList();
            props.AddRange(RecursiveGetProperties(props));
            return props;
        }

        private static List<PropertyInfo> RecursiveGetProperties(List<PropertyInfo> props)
        {
            List<PropertyInfo> nprops = new List<PropertyInfo>(props);
            foreach (PropertyInfo p in props)
                if (typeof(INotifyPropertyChanged).IsAssignableFrom(p.PropertyType))
                    nprops.AddRange(RecursiveGetProperties(p.PropertyType.GetProperties().ToList()));

            return nprops;
        }
    }
}
