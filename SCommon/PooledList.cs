namespace SCommon
{
    /*
        PooledList provides Thread-Safe non-circular doubly linked list
        Created by Shine for SilkroadProject
    */
    using System;
    using System.Threading;
    using System.Collections.Generic;
    using System.Collections.Concurrent;

    public class node<T> where T : class
    {
        public T value_;
        public node<T> next_;
        public node<T> prev_;
        public PooledList<T> list_;
        public bool queueable_;
        public object locker_;

        public node()
        {
            locker_ = new object();
        }
    }

    /// <summary>
    /// Thread-Safe non-circular doubly linked list
    /// </summary>
    /// <typeparam name="T">Item type which will be stored in the list</typeparam>
    public class PooledList<T> : IEnumerable<T> where T : class
    {
        #region Public Properties and Fields

        /// <summary>
        /// The queue stores the freed nodes
        /// </summary>
        private ConcurrentQueue<node<T>> m_queue;

        /// <summary>
        /// The head, always value = null its a start point for iterations
        /// </summary>
        private node<T> m_head;

        /// <summary>
        /// The tail, last node of the cycle
        /// </summary>
        private node<T> m_tail;

        /// <summary>
        /// The count of active nodes (which count of nodes that node.value != null)
        /// </summary>
        private long m_count;

        /// <summary>
        /// The count of allocated nodes (which count of nodes that both node.value != null & node.value == null)
        /// </summary>
        private long m_nodes;

        /// <summary>
        /// Gets the head
        /// </summary>
        public node<T> Head => m_head;

        /// <summary>
        /// Gets the tail
        /// </summary>
        public node<T> Tail => m_tail;

        /// <summary>
        /// Gets the if list is empty
        /// </summary>
        public bool IsEmpty => Interlocked.Read(ref m_count) == 0;

        /// <summary>
        /// GEts the element count in list
        /// </summary>
        public long Count => Interlocked.Read(ref m_count);

        #endregion

        #region Constructors and Destructors

        public PooledList()
        {
            m_queue = new ConcurrentQueue<node<T>>();
            m_head = new node<T>() { list_ = this };
            m_tail = new node<T>() { list_ = this };
            m_head.next_ = m_tail;
            m_count = 0;
            m_nodes = 2; //head + tail
        }

        #endregion

        #region Public Methods

        public node<T> push(T item)
        {
            node<T> node;
            if (m_queue.TryDequeue(out node)) //if we have freed node in the queue
            {
                //change value of freed node
                //we dont need change next/prev because this node is part of chain already
                lock (node.locker_)
                    node.value_ = item;

                //increase the count
                Interlocked.Increment(ref m_count);

                return node;
            }
            else
            {
                //lock the tail
                lock (m_tail.locker_)
                {
                    if (m_tail.value_ == null) //if tail freed while we waiting or already free
                    {
                        //only change the value of tail
                        m_tail.value_ = item;

                        //increase the count
                        Interlocked.Increment(ref m_count);

                        return m_tail;
                    }
                    else
                    {
                        //if the tail is okay allocate the new node
                        //as the tail (next = null)
                        node = new node<T>() { list_ = this, queueable_ = true };
                        node.value_ = item;
                        node.next_ = null;

                        //push allocated node to back so we must
                        //set node->prev = current_tail, current_tail = node
                        //also we must set current_tail->next = node before setting current_tail = node
                        //to ensure the cycle
                        node.prev_ = m_tail;
                        m_tail.next_ = node;
                        m_tail = node;

                        //increase the count
                        Interlocked.Increment(ref m_count);
                        //increse the node count
                        Interlocked.Increment(ref m_nodes);

                        return m_tail;
                    }
                }
            }
        }

        public bool pop(node<T> node)
        {
            ValidateNode(node);

            //lock the node and free node with
            //setting value as null
            lock (node.locker_)
            {
                if (node.value_ == null) //already freed
                    return false;

                node.value_ = null;
            }

            //if node is queueable (if it isnt head or tail)
            //queue the freed node so we can dequeue next time for use
            if (node.queueable_)
                m_queue.Enqueue(node);

            Interlocked.Decrement(ref m_count);

            return true;
        }

        public node<T> first()
        {
            return this.Head.next_;
        }

        public void ForEach(Action<T> action)
        {
            node<T> node = m_head.next_;
            while (node != null)
            {
                lock (node.locker_)
                {
                    if (node.value_ != null)
                        action(node.value_);

                    node = node.next_;
                }
            }
        }

        public Enumarator GetEnumerator()
        {
            return new Enumarator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Private Methods

        private void ValidateNode(node<T> node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (node.list_ != this)
                throw new InvalidOperationException("invalid node for list");
        }


        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Enumerator class for Thread-Safe looping the list<T>

        public struct Enumarator : IEnumerator<T>, System.Collections.IEnumerator
        {
            private PooledList<T> list;
            private node<T> node;
            private T current;
            private object lastlock;

            internal Enumarator(PooledList<T> list)
            {
                this.list = list;
                this.node = list.first();
                this.current = null;
                this.lastlock = null;
            }

            public T Current => current;

            object System.Collections.IEnumerator.Current => current;

            public bool MoveNext()
            {
                if (lastlock != null)
                    Monitor.Exit(lastlock);

                if (node == null)
                {
                    lastlock = null;
                    return false;
                }

                Monitor.Enter(node.locker_);
                lastlock = node.locker_;

                current = node.value_;
                node = node.next_;

                if (current == null)
                    return MoveNext();

                return true;
            }

            void System.Collections.IEnumerator.Reset()
            {
                if (lastlock != null)
                    Monitor.Exit(lastlock);

                this.node = this.list.Head;
                this.current = null;
                lastlock = null;
            }

            public void Dispose()
            {
                if (lastlock != null)
                    Monitor.Exit(lastlock);
            }
        }

        #endregion
    }
}
