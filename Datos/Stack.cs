using System;

namespace CrazyRisk.Datos
{
    public class MyStack<T>
    {
        private Node<T>? head;
        private int count;

        public MyStack() { head = null; count = 0; }
        public int Count => count;

        public void Push(T value)
        {
            var n = new Node<T>(value);
            n.Next = head;
            head = n;
            count++;
        }

        public T Pop()
        {
            if (head == null) throw new InvalidOperationException("Stack empty");
            var v = head.Value;
            head = head.Next;
            count--;
            return v;
        }

        public T Peek()
        {
            if (head == null) throw new InvalidOperationException("Stack empty");
            return head.Value;
        }

        public bool IsEmpty() => count == 0;
    }
}
