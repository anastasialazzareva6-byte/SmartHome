using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartHomeSystem
{
    // Класс с шаблонными функциями (не в классе)
    public static class AlgorithmUtils
    {
        // ШАБЛОННАЯ ФУНКЦИЯ 1: Вычисление статистики для коллекции чисел
        // Ограничение: T должен быть значимым типом и реализовывать IComparable
        public static Statistics<T> CalculateStatistics<T>(IEnumerable<T> collection)
            where T : struct, IComparable<T>
        {
            if (collection == null || !collection.Any())
                return new Statistics<T>();

            var list = collection.ToList();

            // АЛГОРИТМ: Сортировка (аналог std::sort)
            list.Sort();

            T min = list[0];
            T max = list[list.Count - 1];

            // Вычисление среднего
            dynamic sum = default(T);
            foreach (var item in list)
            {
                sum += (dynamic)item;
            }
            T average = (T)(sum / list.Count);

            return new Statistics<T>
            {
                Count = list.Count,
                Min = min,
                Max = max,
                Average = average
            };
        }

        // ШАБЛОННАЯ ФУНКЦИЯ 2: Поиск элемента в коллекции (аналог std::find)
        public static T FindItem<T>(IEnumerable<T> collection, Predicate<T> predicate)
            where T : class
        {
            if (collection == null)
                return null;

            // АЛГОРИТМ: Поиск (аналог std::find_if)
            foreach (var item in collection)
            {
                if (predicate(item))
                    return item;
            }
            return null;
        }

        // ШАБЛОННАЯ ФУНКЦИЯ 3: Фильтрация коллекции (аналог std::filter_view)
        public static IEnumerable<T> Filter<T>(IEnumerable<T> collection, Predicate<T> predicate)
            where T : class
        {
            if (collection == null)
                yield break;

            foreach (var item in collection)
            {
                if (predicate(item))
                    yield return item;
            }
        }

        // ШАБЛОННАЯ ФУНКЦИЯ 4: Преобразование коллекции (аналог std::transform)
        public static IEnumerable<TResult> Transform<T, TResult>(
            IEnumerable<T> collection,
            Func<T, TResult> transformer)
            where T : class
            where TResult : class
        {
            if (collection == null)
                yield break;

            foreach (var item in collection)
            {
                yield return transformer(item);
            }
        }

        // ШАБЛОННАЯ ФУНКЦИЯ 5: Копирование коллекции с условием (аналог std::copy_if)
        public static List<T> CopyIf<T>(IEnumerable<T> collection, Predicate<T> predicate)
            where T : class
        {
            var result = new List<T>();
            if (collection == null)
                return result;

            foreach (var item in collection)
            {
                if (predicate(item))
                    result.Add(item);
            }
            return result;
        }

        // ШАБЛОННАЯ ФУНКЦИЯ 6: Удаление элементов из коллекции (аналог std::remove_if)
        public static List<T> RemoveIf<T>(List<T> collection, Predicate<T> predicate)
            where T : class
        {
            if (collection == null)
                return new List<T>();

            var result = new List<T>(collection);
            result.RemoveAll(predicate);
            return result;
        }

        // ШАБЛОННАЯ ФУНКЦИЯ 7: Проверка условия для любого элемента (аналог std::any_of)
        public static bool AnyOf<T>(IEnumerable<T> collection, Predicate<T> predicate)
            where T : class
        {
            if (collection == null)
                return false;

            foreach (var item in collection)
            {
                if (predicate(item))
                    return true;
            }
            return false;
        }

        // ШАБЛОННАЯ ФУНКЦИЯ 8: Поиск минимального элемента (аналог std::min_element)
        public static T MinElement<T, TKey>(IEnumerable<T> collection, Func<T, TKey> keySelector)
            where T : class
            where TKey : IComparable<TKey>
        {
            if (collection == null || !collection.Any())
                return null;

            return collection.OrderBy(keySelector).First();
        }

        // ШАБЛОННАЯ ФУНКЦИЯ 9: Поиск максимального элемента (аналог std::max_element)
        public static T MaxElement<T, TKey>(IEnumerable<T> collection, Func<T, TKey> keySelector)
            where T : class
            where TKey : IComparable<TKey>
        {
            if (collection == null || !collection.Any())
                return null;

            return collection.OrderByDescending(keySelector).First();
        }

        // ШАБЛОННАЯ ФУНКЦИЯ 10: Использование варианта (аналог std::variant)
        // Обертка для хранения разных типов данных
        public static object GetVariantValue<T1, T2>(bool useFirst, T1 firstValue, T2 secondValue)
            where T1 : class
            where T2 : class
        {
            return useFirst ? firstValue : secondValue;
        }

        // ШАБЛОННАЯ ФУНКЦИЯ 11: Вычисление суммарной мощности устройств
        public static double CalculateTotalPower<T>(IEnumerable<T> devices)
            where T : Device
        {
            if (devices == null)
                return 0;

            double total = 0;
            foreach (var device in devices)
            {
                if (device.IsOn)
                {
                    total += device.PowerConsumption;
                }
            }
            return total;
        }

        // ШАБЛОННАЯ ФУНКЦИЯ 12: Группировка устройств по типу
        public static Dictionary<DeviceType, List<T>> GroupByDeviceType<T>(IEnumerable<T> devices)
            where T : Device
        {
            var result = new Dictionary<DeviceType, List<T>>();

            if (devices == null)
                return result;

            foreach (var device in devices)
            {
                if (!result.ContainsKey(device.DeviceType))
                {
                    result[device.DeviceType] = new List<T>();
                }
                result[device.DeviceType].Add(device);
            }

            return result;
        }
    }

    // Вспомогательный класс для статистики
    public struct Statistics<T> where T : struct
    {
        public int Count { get; set; }
        public T Min { get; set; }
        public T Max { get; set; }
        public T Average { get; set; }

        public override string ToString()
        {
            return $"Количество: {Count}, Минимум: {Min}, Максимум: {Max}, Среднее: {Average}";
        }
    }
}