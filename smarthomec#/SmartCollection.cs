using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SmartHomeSystem
{
    // ШАБЛОННЫЙ КЛАСС: Универсальная коллекция для хранения объектов
    // T должен реализовывать IDeviceDiagnostic
    public class SmartCollection<T> : IEnumerable<T> where T : IDeviceDiagnostic
    {
        // КОНТЕЙНЕР: List<T> (аналог std::vector)
        private List<T> items;

        // КОНТЕЙНЕР: Dictionary для быстрого поиска по ID (аналог std::map)
        private Dictionary<string, T> itemDictionary;

        public int Count => items.Count;
        public bool IsEmpty => items.Count == 0;

        public SmartCollection()
        {
            items = new List<T>();
            itemDictionary = new Dictionary<string, T>();
        }

        public SmartCollection(IEnumerable<T> collection)
        {
            items = new List<T>(collection);
            itemDictionary = new Dictionary<string, T>();

            // Заполняем словарь для устройств
            foreach (var item in items)
            {
                if (item is Device device)
                {
                    itemDictionary[device.DeviceId] = (T)(object)device;
                }
            }
        }

        // НЕШАБЛОННЫЙ МЕТОД: Добавление элемента
        public void Add(T item)
        {
            items.Add(item);

            // Обновляем словарь для устройств
            if (item is Device device)
            {
                itemDictionary[device.DeviceId] = item;
            }
        }

        // НЕШАБЛОННЫЙ МЕТОД: Удаление элемента
        public bool Remove(T item)
        {
            bool removed = items.Remove(item);

            if (removed && item is Device device)
            {
                itemDictionary.Remove(device.DeviceId);
            }

            return removed;
        }

        // ШАБЛОННЫЙ МЕТОД: Поиск элемента по предикату (аналог std::find_if)
        public T Find(Predicate<T> predicate)
        {
            return items.Find(predicate);
        }

        // ШАБЛОННЫЙ МЕТОД: Фильтрация элементов (аналог std::copy_if)
        public SmartCollection<T> Filter(Predicate<T> predicate)
        {
            return new SmartCollection<T>(items.Where(x => predicate(x)));
        }

        // АЛГОРИТМ: Сортировка (аналог std::sort)
        public void Sort(Comparison<T> comparison)
        {
            items.Sort(comparison);
        }

        // АЛГОРИТМ: Сортировка по ключу
        public void SortBy<TKey>(Func<T, TKey> keySelector) where TKey : IComparable<TKey>
        {
            items = items.OrderBy(keySelector).ToList();
        }

        // АЛГОРИТМ: Поиск минимального элемента (аналог std::min_element)
        public T MinBy<TKey>(Func<T, TKey> keySelector) where TKey : IComparable<TKey>
        {
            return items.OrderBy(keySelector).FirstOrDefault();
        }

        // АЛГОРИТМ: Поиск максимального элемента (аналог std::max_element)
        public T MaxBy<TKey>(Func<T, TKey> keySelector) where TKey : IComparable<TKey>
        {
            return items.OrderByDescending(keySelector).FirstOrDefault();
        }

        // АЛГОРИТМ: Проверка условия (аналог std::any_of)
        public bool Any(Predicate<T> predicate)
        {
            return items.Any(x => predicate(x));
        }

        // АЛГОРИТМ: Все элементы удовлетворяют условию
        public bool All(Predicate<T> predicate)
        {
            return items.All(x => predicate(x));
        }

        // ШАБЛОННЫЙ МЕТОД: Преобразование элементов (аналог std::transform)
        public SmartCollection<TResult> Transform<TResult>(Func<T, TResult> transformer) where TResult : IDeviceDiagnostic
        {
            var result = new SmartCollection<TResult>();
            foreach (var item in items)
            {
                var transformed = transformer(item);
                if (transformed != null)
                {
                    result.Add(transformed);
                }
            }
            return result;
        }

        // ШАБЛОННЫЙ МЕТОД: Выполнение действия для каждого элемента
        public void ForEach(Action<T> action)
        {
            items.ForEach(action);
        }

        // Быстрый поиск устройства по ID (использует Dictionary)
        public T FindDeviceById(string deviceId)
        {
            if (itemDictionary.TryGetValue(deviceId, out T device))
            {
                return device;
            }
            return default;
        }

        // Копирование элементов (аналог std::copy)
        public List<T> Copy()
        {
            return new List<T>(items);
        }

        // Копирование с условием (аналог std::copy_if)
        public List<T> CopyIf(Predicate<T> predicate)
        {
            return items.Where(x => predicate(x)).ToList();
        }

        // Удаление элементов по условию (аналог std::remove_if)
        public int RemoveIf(Predicate<T> predicate)
        {
            int removedCount = 0;
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (predicate(items[i]))
                {
                    var item = items[i];
                    items.RemoveAt(i);

                    if (item is Device device)
                    {
                        itemDictionary.Remove(device.DeviceId);
                    }

                    removedCount++;
                }
            }
            return removedCount;
        }

        // Очистка коллекции
        public void Clear()
        {
            items.Clear();
            itemDictionary.Clear();
        }

        // Итераторы
        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // Индексатор
        public T this[int index]
        {
            get => items[index];
            set => items[index] = value;
        }

        // Конвертация в массив (аналог std::array)
        public T[] ToArray()
        {
            return items.ToArray();
        }

        // Получение среза (аналог std::span)
        public List<T> Slice(int start, int count)
        {
            if (start < 0 || start >= items.Count || count <= 0)
                return new List<T>();

            int end = Math.Min(start + count, items.Count);
            return items.GetRange(start, end - start);
        }

        // Диагностика всех элементов
        public void RunAllDiagnostics()
        {
            Console.WriteLine($"Запуск диагностики для {items.Count} элементов:");
            foreach (var item in items)
            {
                Console.WriteLine(item.RunDiagnostics());
            }
        }
    }
}