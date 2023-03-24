namespace KianCommons {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal static class EnumerationExtensions {
        /// <summary>
        /// returns a new List of cloned items.
        /// </summary>
        internal static List<T> Clone1<T>(this IEnumerable<T> orig) where T : ICloneable =>
            orig.Select(item => (T)item.Clone()).ToList();

        internal static Dictionary<TKey, TValue> ShallowClone<TKey, TValue>(this IDictionary<TKey, TValue> dict) {
            if (dict == null) return null;
            return new Dictionary<TKey, TValue>(dict);
        }

        /// <summary>
        /// fast way of determining if collection is null or empty
        /// </summary>
        internal static bool IsNullorEmpty<T>(this ICollection<T> a)
            => a == null || a.Count == 0;

        /// <summary>
        /// generic way of determining if IEnumerable is null or empty
        /// </summary>
        internal static bool IsNullorEmpty<T>(this IEnumerable<T> a) {
            return a == null || !a.Any();
        }

        internal static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> a) where T : class =>
            a ?? Enumerable.Empty<T>();
        internal static IEnumerable<T?> EmptyIfNull<T>(this IEnumerable<T?> a) where T: struct=>
            a ?? Enumerable.Empty<T?>();

        internal static bool AllEqual<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            comparer = comparer ?? EqualityComparer<T>.Default;

            using (var enumerator = source.GetEnumerator()) {
                if (enumerator.MoveNext()) {
                    var value = enumerator.Current;

                    while (enumerator.MoveNext()) {
                        if (!comparer.Equals(enumerator.Current, value))
                            return false;
                    }
                }

                return true;
            }
        }

        internal static bool AllEqual<T,T2>(this IEnumerable<T> source, Func<T,T2> selector, IEqualityComparer<T2> comparer = null) {
            return source.Select(selector).AllEqual();
        }

        internal static int FindIndex<T>(this IEnumerable<T> e, Func<T,bool> predicate) {
            int i = 0;
            foreach(var item in e) {
                if (predicate(item))
                    return i;
                i++;
            }
            return -1;
        }

        internal static void DropElement<T>(ref T[] array, int i) {
            int n1 = array.Length;
            T[] ret = new T[n1 - 1];
            int i1 = 0, i2 = 0;

            while (i1 < n1) {
                if (i1 != i) {
                    ret[i2] = array[i1];
                    i2++;
                }
                i1++;
            }
            array = ret;
        }

        internal static T[] RemoveAt<T>(this T[] array, int index) {
            var list = new List<T>(array);
            list.RemoveAt(index);
            return list.ToArray();
        }


        internal static bool ContainsRef<T>(this IEnumerable<T> list, T element) where T : class {
            foreach (T item in list) {
                if (object.ReferenceEquals(item, element))
                    return true;
            }
            return false;
        }

        internal static void AppendElement<T>(ref T[] array, T element) {
            Array.Resize(ref array, array.Length + 1);
            array.Last() = element;
        }

        internal static T[] AppendOrCreate<T>(this T[] array, T element) {
            if (array == null) {
                array = new T[] { element };
            } else {
                AppendElement(ref array, element);
            }
            return array;
        }

        internal static void ReplaceElement<T>(this T[] array, T oldVal, T newVal) {
            int index = (array as IList).IndexOf(oldVal);
            if(index>=0)
                array[index] = newVal;
        }

        internal static void ReplaceElement(this Array array, object oldVal, object newVal) {
            int index = (array as IList).IndexOf(oldVal);
            if(index >= 0)
                array.SetValue(newVal, index);
        }

        internal static ref T Last<T>(this T[] array) => ref array[array.Length - 1];

        internal static void Swap<T>(this IList<T> list, int i1, int i2) {
            var temp = list[i1];
            list[i1] = list[i2];
            list[i2] = temp;
        }

        internal static TItem MinBy<TItem, TBy>(this IEnumerable<TItem> items, Func<TItem, TBy> selector)
            where TBy : IComparable {
            if (items == null) return default;
            TItem ret = default;
            TBy val = default;
            bool first = true;
            foreach (TItem item in items) {
                TBy val2 = selector(item);
                if (first || val2.CompareTo(val) < 0) {
                    first = false;
                    ret = item;
                    val = val2;
                }
            }
            return ret;
        }
        internal static TItem MaxBy<TItem, TBy>(this IEnumerable<TItem> items, Func<TItem, TBy> selector)
            where TBy : IComparable {
            if (items == null) return default;
            TItem ret = default;
            TBy val = default;
            bool first = true;
            foreach (TItem item in items) {
                TBy val2 = selector(item);
                if (first || val2.CompareTo(val) > 0) {
                    first = false;
                    ret = item;
                    val = val2;
                }
            }
            return ret;
        }

        internal static TValue GetorDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key) {
            return dict.TryGetValue(key, out TValue value) ? value : default(TValue);
        }

    }
}
