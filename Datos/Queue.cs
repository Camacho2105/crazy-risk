using System;

namespace CrazyRisk.Datos
{
    public class MyQueue<T>
    {
        private Node<T>? head, tail;
        private int count;

        public MyQueue() { head = tail = null; count = 0; }
        public int Count => count;

        public void Enqueue(T value)
        {
            var n = new Node<T>(value);
            if (tail == null) head = tail = n;
            else { tail.Next = n; tail = n; }
            count++;
        }

        public T Dequeue()
        {
            if (head == null) throw new InvalidOperationException("Queue is empty");
            var v = head.Value;
            head = head.Next;
            if (head == null) tail = null;
            count--;
            return v;
        }

        public T Peek()
        {
            if (head == null) throw new InvalidOperationException("Queue is empty");
            return head.Value;
        }

        public bool IsEmpty() => count == 0;
    }
}
