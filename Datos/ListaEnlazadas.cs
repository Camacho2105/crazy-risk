using System;
using System.Collections;
using System.Collections.Generic;

namespace CrazyRisk.Datos
{
    public class MyLinkedList<T> : IEnumerable<T>
    {
        private Node<T>? head;
        private Node<T>? tail;
        private int count;

        public MyLinkedList() { head = tail = null; count = 0; }
        public int Count => count;

        public void AddLast(T value)
        {
            var node = new Node<T>(value);
            if (head == null) head = tail = node;
            else { tail.Next = node; tail = node; }
            count++;
        }

        public bool Remove(T value)
        {
            Node<T>? prev = null;
            var cur = head;
            while (cur != null)
            {
                if (Equals(cur.Value, value))
                {
                    if (prev == null) head = cur.Next;
                    else prev.Next = cur.Next;
                    if (cur == tail) tail = prev;
                    count--;
                    return true;
                }
                prev = cur;
                cur = cur.Next;
            }
            return false;
        }

        public bool Contains(T value)
        {
            var cur = head;
            while (cur != null)
            {
                if (Equals(cur.Value, value)) return true;
                cur = cur.Next;
            }
            return false;
        }

        public T[] ToArray()
        {
            T[] arr = new T[count];
            var cur = head;
            int i = 0;
            while (cur != null)
            {
                arr[i++] = cur.Value;
                cur = cur.Next;
            }
            return arr;
        }

        public void ForEach(Action<T> action)
        {
            var cur = head;
            while (cur != null)
            {
                action(cur.Value);
                cur = cur.Next;
            }
        }

        // ✅ Implementación de IEnumerable<T>
        public IEnumerator<T> GetEnumerator()
        {
            var cur = head;
            while (cur != null)
            {
                yield return cur.Value;
                cur = cur.Next;
            }
        }

        // ✅ Implementación de IEnumerable (no genérico)
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
