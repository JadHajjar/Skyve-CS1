namespace KianCommons {
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public unsafe struct FastSegmentList : IEnumerable<ushort> {
        const int MAX_SIZE = 8;
        int size_;
        fixed ushort segments_[MAX_SIZE];

        public int Count => size_;

        public void Add(ushort value) {
            if (size_ >= MAX_SIZE)
                throw new Exception("List grows too big (max size is 10)");
            segments_[size_++] = value;
        }

        public ushort this[int index] {
            get {
                if (index < size_ && index >= 0)
                    return segments_[index];
                else
                    throw new IndexOutOfRangeException($"index:{index} size:{size_}");
            }
            set {
                if (index < size_)
                    segments_[index] = value;
                else
                    throw new IndexOutOfRangeException($"index:{index} size:{size_}");
            }
        }

        public void Clear() => size_ = 0;

        #region iterator
        IEnumerator<ushort> IEnumerable<ushort>.GetEnumerator() => new Iterator(this);
        IEnumerator IEnumerable.GetEnumerator() => new Iterator(this);

        public struct Iterator : IEnumerator<ushort> {
            int i_;
            FastSegmentList list_;
            ushort current_;

            public Iterator(FastSegmentList list) {
                i_ = 0;
                list_ = list;
                current_ = 0;
            }

            public bool MoveNext() {
                if (i_ < list_.Count) {
                    current_ = list_[i_++];
                    return true;
                } else {
                    return false;
                }
            }

            public void Reset() => i_ = current_ = 0;

            public ushort Current => current_;
            public Iterator GetEnumerator() => this;
            public void Dispose() => Reset();
            object IEnumerator.Current => Current;
        }
        #endregion
    }
}
