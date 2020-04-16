using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Utils {
    
    public unsafe struct NativeLinkedNode<T> where T: struct {
        public T value;
        internal int prev;
        internal int next;
        internal bool isNoInList;
        internal int index;

        public static NativeLinkedNode<T> Create(T value) {
            NativeLinkedNode<T> ret = new NativeLinkedNode<T>();
            ret.value = value;
            ret.prev = -1;
            ret.next = -1;
            ret.index = -1;
            ret.isNoInList = true;
            return ret;
        }

        public bool HasPrevNode {
            get {
                return (!isNoInList) && (prev >= 0);
            }
        }
        public bool HasNextNode {
            get {
                return (!isNoInList) && (next >= 0);
            }
        }
    }
    
    // 用连续空间实现的链表
    public unsafe class NativeLinkedList<T>: DisposeObject where T: struct {
        private static NativeLinkedNode<T> _cEmpty = new NativeLinkedNode<T>();
        private NativeList<NativeLinkedNode<T>> m_List = null;
        private int m_FirstIdx = -1;
        private int m_EmptyFirst = -1;


        protected override void OnFree(bool isManual) {
            if (m_List != null) {
                m_List.Dispose();
                m_List = null;
            }
        }

        public bool GetFirstNode(out NativeLinkedNode<T> node) {
            node = _cEmpty;
            if (m_List  == null || m_FirstIdx < 0)
                return false;
            node = m_List[m_FirstIdx];
            return true;
        }

        private void PushEmptyList(NativeLinkedNode<T> item) {
            if (m_List == null || item.isNoInList)
                return;
            item.prev = -1;
            item.isNoInList = true;
            item.next = m_EmptyFirst;
            if (m_EmptyFirst >= 0) {
                var firstNode = m_List[m_EmptyFirst];
                firstNode.prev = item.index;
            } else {
                m_EmptyFirst = item.index;
            }
        }

        private void InitList() {
            if (m_List == null)
                m_List = new NativeList<NativeLinkedNode<T>>();
        }

        public void Clear(bool isRemoveEmptyNode = false) {
            if (isRemoveEmptyNode) {
                m_FirstIdx = -1;
                m_EmptyFirst = -1;
                if (m_List != null)
                    m_List.Clear();

                return;
            }


        }

        public NativeLinkedNode<T> AddFirst(T item) {
            InitList();
            if (m_EmptyFirst < 0) {
                var node = NativeLinkedNode<T>.Create(item);
                int idx = m_List.Add(node);
                if (idx >= 0) {
                    // 说明在队列里
                    node.isNoInList = false;
                    node.index = idx;
                    node.next = m_FirstIdx;
                    if (m_FirstIdx >= 0) {
                        var firstNode = m_List[m_FirstIdx];
                        firstNode.prev = idx;
                        node.next = firstNode.index;
                    }
                    m_FirstIdx = idx;
                }

                return node;
            } else {
                var emptyNode = m_List[m_EmptyFirst];
                emptyNode.value = item;
                emptyNode.isNoInList = false;
                emptyNode.prev = -1;
                emptyNode.next = m_FirstIdx;
                if (m_FirstIdx >= 0) {
                    var firstNode = m_List[m_FirstIdx];
                    firstNode.prev = emptyNode.index;
                } else {
                    m_FirstIdx = emptyNode.index;
                }

                return emptyNode;
             }
        }

        public bool RemoveFirst() {
            NativeLinkedNode<T> node;
            if (GetFirstNode(out node)) {
                return Remove(node);
            }
            return false;
        }

        public bool Remove(NativeLinkedNode<T> item) {
            if (m_List == null || m_List.Count <= 0 || item.isNoInList)
                return false;
            bool isFirst = item.prev < 0;
            if (item.HasPrevNode) {
                NativeLinkedNode<T> prevNode = m_List[item.prev];
                prevNode.next = item.next;
            }

            if (item.HasNextNode) {
                NativeLinkedNode<T> nextNode = m_List[item.next];
                nextNode.prev = item.prev;
            }

            if (isFirst) {
                m_FirstIdx = item.next;
            }

            item.prev = -1;
            item.next = -1;


            PushEmptyList(item);


            return true;
        }
        
    }
}