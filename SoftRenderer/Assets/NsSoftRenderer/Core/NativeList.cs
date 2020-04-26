using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Utils {

    // 原生List
    //[BurstCompile]
    public unsafe class NativeList<T> : DisposeObject where T : struct {
        private NativeArray<T> m_Arr;
        private int m_Count = 0;
        private Allocator m_Allocator = Allocator.Persistent;

        public NativeArray<T> OriginArray {
            get {
                return m_Arr;
            }
        }

        public void* GetPtr(int index) {
            if (IsVaild && index >= 0 && index < m_Count) {
                IntPtr bb = (IntPtr)m_Arr.GetUnsafePtr<T>();
                int elemSize = this.ElemSize;
                void* ret = (void*)IntPtr.Add(bb, elemSize * index);
                return ret;
            }
            return null;
        }

        public IntPtr GetIntPtr(int index) {
            void* ret = GetPtr(index);
            return (IntPtr)ret;
        }

        public int ElemSize {
            get {
                return UnsafeUtility.SizeOf<T>();
            }
        }

        public NativeList(int cap = 0, Allocator alloc = Allocator.Persistent) {
            m_Allocator = alloc;
            if (cap > 0) {
                m_Arr = new NativeArray<T>(cap, m_Allocator, NativeArrayOptions.UninitializedMemory);
            }
        }

        public void Clear(bool isClearArray = true) {
            if (isClearArray)
                FreeArray();
            m_Count = 0;
        }

        public bool IsVaild {
            get {
                return m_Arr.IsCreated;
            }
        }

        protected virtual void Grow() {
            int delta;
            int cap = this.Capacity;
            if (cap > 64)
                delta = cap / 4;
            else if (cap > 8)
                delta = 16;
            else
                delta = 4;
            this.Capacity = cap + delta;
        }

        // 快排
        static protected void QuickSort<V>(NativeArray<V> arr, int L, int R, IComparer<V> onCompare) where V : struct {
            int I;
            do {
                I = L;
                int J = R;
                V P = arr[(L + R) >> 1];
                do {
                    while (onCompare.Compare(arr[I], P) < 0)
                        ++I;
                    while (onCompare.Compare(arr[J], P) > 0)
                        --J;
                    if (I <= J) {
                        V t = arr[I];
                        arr[I] = arr[J];
                        arr[J] = t;
                        ++I;
                        --J;
                    }
                } while (I <= J);
            } while (I < R);
        }

        // 快速排序
        public bool Sort(IComparer<T> onCompare) {
            if (IsVaild && m_Count > 0 && onCompare != null) {
                QuickSort(m_Arr, 0, m_Count - 1, onCompare);
                return true;
            }
            return false;
        }

        public int LastIndexOf(T item, IComparer<T> onCompare = null) {
            if (IsVaild) {
                for (int i = m_Count - 1; i >= 0; --i) {
                    if (onCompare != null) {
                        if (onCompare.Compare(item, m_Arr[i]) == 0)
                            return i;
                    } else if (item.Equals(m_Arr[i]))
                        return i;
                }
            }
            return -1;
        }

        public bool Delete(int index) {
            if (IsVaild && index >= 0 && index < m_Count) {
                // 减少数量
                --m_Count;

                if (index < m_Count) {
                    // 移动数组
                    int elemSize = UnsafeUtility.SizeOf<T>();
                    IntPtr bb = (IntPtr)m_Arr.GetUnsafePtr();
                    void* src = (void*)IntPtr.Add(bb, elemSize * (index + 1));
                    void* dst = (void*)IntPtr.Add(bb, elemSize * index);
                    // 拷贝
                    UnsafeUtility.MemCpy(dst, src, elemSize * (m_Count - index));
                }

                return true;
            }
            return false;
        }

        public int Remove(T item, IComparer<T> onCompare = null) {
            int ret = IndexOf(item, onCompare);
            if (ret >= 0)
                Delete(ret);
            return ret;
        }

        public bool Insert(int index, T item) {
            if (!IsVaild || index < 0 || index > m_Count)
                return false;
            if (m_Count == this.Capacity)
                Grow();


            if (index < m_Count) {
                int elemSize = UnsafeUtility.SizeOf<T>();
                IntPtr bb = (IntPtr)m_Arr.GetUnsafePtr();
                void* src = (void*)IntPtr.Add(bb, index * elemSize);
                void* dst = (void*)IntPtr.Add(bb, (index + 1) * elemSize);
                UnsafeUtility.MemCpy(dst, src, (m_Count - index) * elemSize);
            }

            m_Arr[index] = item;
            ++m_Count;

            return true;
        }

        // 交换
        public bool Exchange(int idx1, int idx2) {
            if (IsVaild && idx1 >= 0 && idx2 >= 0 && idx1 < m_Count && idx2 < m_Count) {
                T temp = m_Arr[idx1];
                m_Arr[idx1] = m_Arr[idx2];
                m_Arr[idx2] = temp;
                return true;
            }
            return false;
        }

        public int Add(T item) {
            int ret = m_Count;
            if (ret == this.Capacity)
                Grow();
            m_Arr[ret] = item;
            ++m_Count;

            return ret;
        }

        public int IndexOf(T item, IComparer<T> onCompare = null) {
            if (IsVaild) {
                for (int i = 0; i < m_Count; ++i) {
                    if (onCompare != null) {
                        if (onCompare.Compare(item, m_Arr[i]) == 0)
                            return i;
                    } else if (item.Equals(m_Arr[i]))
                        return i;
                }
            }
            return -1;
        }

        public T First {
            get {
                if (IsVaild && m_Count > 0)
                    return m_Arr[0];
                return default(T);
            }
        }

        public T Last {
            get {
                if (IsVaild && m_Count > 0)
                    return m_Arr[m_Count - 1];
                return default(T);
            }
        }

        public bool Contains(T item, IComparer<T> onCompare = null) {
            return IndexOf(item, onCompare) >= 0;
        }

        protected override void OnFree(bool isManual) {
            Clear();
        }

        private void FreeArray() {
            if (IsVaild) {
                m_Arr.Dispose();
            }
        }

        public static readonly int MaxCount = int.MaxValue / UnsafeUtility.SizeOf<T>();

        public int Count {
            get {
                return m_Count;
            }

            set {
                if (value < 0 || value > MaxCount)
                    return;
                if (value > this.Capacity)
                    this.Capacity = value;
                if (value > m_Count) {
                    int elemSize = UnsafeUtility.SizeOf<T>();
                    void* dst = (void*)IntPtr.Add((IntPtr)m_Arr.GetUnsafePtr<T>(), m_Count * elemSize);
                    UnsafeUtility.MemClear(dst, (value - m_Count) * elemSize);
                } else {
                    for (int i = m_Count - 1; i >= 0; --i) {
                        Delete(i);
                    }
                }
                m_Count = value;
            }
        }

        public T this[int idx] {
            get {
                if (IsVaild && m_Count > 0) {
                    return m_Arr[idx];
                }
                return default(T);
            }

            set {
                if (IsVaild && m_Count > 0) {
                    m_Arr[idx] = value;
                }
            }
        }
          

        public int Capacity {
            get {
                return m_Arr.Length;
            }

            set {
                if (value < 0 || value > MaxCount)
                    return;

                if (value < Count)
                    m_Count = value;

                // 创建新的数组

                if (value > 0) {

                    
                    var newArr = new NativeArray<T>(value, m_Allocator, NativeArrayOptions.UninitializedMemory);

                    if (IsVaild) {
                        if (m_Count > 0) {
                            int elemSize = UnsafeUtility.SizeOf<T>();
                            void* dst = newArr.GetUnsafePtr<T>();
                            void* src = m_Arr.GetUnsafePtr<T>();
                            UnsafeUtility.MemCpy(dst, src, m_Count * elemSize);
                        }
                        // 释放老数组
                        FreeArray();
                    }
                    // 赋值数组
                    m_Arr = newArr;

                } else {
                    Clear();
                }
            }
        }


        // 紧缩
        public void Pack() {
            if (!IsVaild || m_Count == 0)
                return;
            int packCount = 0;
            int startIndex = 0;
            T defaultValue = default(T);
            do {
                while ((m_Arr[startIndex].Equals(defaultValue)) && (startIndex < m_Count))
                    ++startIndex;
                if (startIndex < m_Count) {
                    int endIndex = startIndex;
                    while (!m_Arr[endIndex].Equals(defaultValue) && (endIndex < m_Count))
                        ++endIndex;
                    --endIndex;
                    if (startIndex > packCount) {
                        int elemSize = UnsafeUtility.SizeOf<T>();
                        IntPtr bb = (IntPtr)m_Arr.GetUnsafePtr<T>();
                        void* src = (void*)IntPtr.Add(bb, startIndex * elemSize);
                        void* dst = (void*)IntPtr.Add(bb, packCount * elemSize);

                        UnsafeUtility.MemCpy(dst, src, (endIndex - startIndex + 1) * elemSize);
                    }
                    packCount += endIndex - startIndex + 1;
                    startIndex = endIndex + 1;
                }

            } while (startIndex < m_Count);
        }


    }
}
