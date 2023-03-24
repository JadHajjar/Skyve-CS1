using System;

namespace KianCommons.UI.Table {
    static class ArrayExtension {
        public static T[] Expand<T>(this T[] array, int size, Func<int,T> createNewElement) {
            if (array.Length < size) {
                var newArray = new T[size];
                array.CopyTo(newArray, 0);
                for (int i = array.Length; i < size; i++) {
                    newArray[i] = createNewElement(i);
                }
                return newArray;
            } else {
                return array;
            }
        }
        public static T[] Shrink<T>(this T[] array, int size, Action<T,int> destroyElement) {
            if(array.Length > size) {
                var newArray = new T[size];
                Array.Copy(array, newArray, size);
                for(int i = size; i<array.Length; i++) {
                    destroyElement(array[i], i);
                }
                return newArray;
            } else {
                return array;
            }
        }
    }
}
