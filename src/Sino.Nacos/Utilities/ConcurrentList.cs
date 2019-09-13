using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;

namespace Sino.Nacos.Utilities
{
    public class ConcurrentList<T> : IList<T>
    {
        private readonly List<T> m_TList;
        private readonly ReaderWriterLockSlim LockList = new ReaderWriterLockSlim();

        private int m_Disposed;
        private bool Disposed
        {
            get { return Thread.VolatileRead(ref m_Disposed) == 1; }
            set { Thread.VolatileWrite(ref m_Disposed, value ? 1 : 0); }
        }

        public ConcurrentList()
        {
            m_TList = new List<T>();
        }

        public ConcurrentList(int capacity)
        {
            m_TList = new List<T>(capacity);
        }

        public ConcurrentList(IEnumerable<T> collection)
        {
            m_TList = new List<T>(collection);
        }

        /// <summary>
        /// Converts List to TList
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator ConcurrentList<T>(List<T> value)
        {
            return new ConcurrentList<T>(value);
        }

        /// <summary>
        /// Returns an enumerator to iterate through the collection
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            List<T> localList;

            LockList.EnterReadLock();
            try
            {
                // create a copy of m_TList
                localList = new List<T>(m_TList);
            }
            finally
            {
                LockList.ExitReadLock();
            }

            // get the enumerator
            foreach (T item in localList)
                yield return item;
        }

        /// <summary>
        /// Returns an enumerator to iterate through the collection
        /// </summary>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            List<T> localList;
            LockList.EnterReadLock();
            try
            {
                localList = new List<T>(m_TList);
            }
            finally
            {
                LockList.ExitReadLock();
            }
            foreach (T item in localList)
                yield return item;
        }

        /// <summary>
        /// Dispose of the object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The disposer
        /// </summary>
        /// <param name="disposing">true if disposed</param>
        private void Dispose(bool disposing)
        {
            if (this.Disposed) return;
            Disposed = true;
        }

        ~ConcurrentList()
        {
            Dispose(false);
        }

        /// <summary>
        /// Adds an item to the threaded list
        /// </summary>
        /// <param name="item">the item to add to the end of the collection</param>
        public void Add(T item)
        {
            LockList.EnterWriteLock();
            try
            {
                m_TList.Add(item);
            }

            finally
            {
                LockList.ExitWriteLock();
            }
        }

        /// <summary>
        /// Adds the elements of collection to the end of the threaded list
        /// </summary>
        /// <param name="collection">the collection to add to the end of the list</param>
        public void AddRange(IEnumerable<T> collection)
        {
            LockList.EnterWriteLock();
            try
            {
                m_TList.AddRange(collection);
            }

            finally
            {
                LockList.ExitWriteLock();
            }
        }

        /// <summary>
        /// Adds an item to the threaded list if it is not already in the list.
        /// Returns true if added to the list, false if the item already existed
        /// in the list
        /// </summary>
        /// <param name="item">the item to add to the end of the collection</param>
        public bool AddIfNotExist(T item)
        {
            LockList.EnterWriteLock();
            try
            {
                // check if it exists already
                if (m_TList.Contains(item))
                    return false;

                // add the item and return true
                m_TList.Add(item);
                return true;
            }

            finally
            {
                LockList.ExitWriteLock();
            }
        }

        /// <summary>
        /// Returns a read-only collection of the current threaded list
        /// </summary>
        public ReadOnlyCollection<T> AsReadOnly()
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.AsReadOnly();
            }

            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Searches the collection using the default comparator and returns the zero-based index of the item found
        /// </summary>
        /// <param name="item">the item to search for</param>
        public int BinarySearch(T item)
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.BinarySearch(item);
            }
            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Searches the collection using the default comparator and returns the zero-based index of the item found
        /// </summary>
        /// <param name="item">the item to search for</param>
        /// <param name="comparer">the IComparer to use when searching, or null to use the default</param>
        public int BinarySearch(T item, IComparer<T> comparer)
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.BinarySearch(item, comparer);
            }
            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Searches the collection using the default comparator and returns the zero-based index of the item found
        /// </summary>
        /// <param name="index">the zero-based index to start searching from</param>
        /// <param name="count">the number of records to search</param>
        /// <param name="item">the item to search for</param>
        /// <param name="comparer">the IComparer to use when searching, or null to use the default</param>
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.BinarySearch(index, count, item, comparer);
            }
            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets or sets the initial capacity of the list
        /// </summary>
        public int Capacity
        {
            get
            {
                LockList.EnterReadLock();
                try
                {
                    return m_TList.Capacity;
                }

                finally
                {
                    LockList.ExitReadLock();
                }
            }
            set
            {
                LockList.EnterWriteLock();
                try
                {
                    m_TList.Capacity = value;
                }

                finally
                {
                    LockList.ExitWriteLock();
                }
            }
        }

        /// <summary>
        /// Removes all items from the threaded list
        /// </summary>
        public void Clear()
        {
            LockList.EnterReadLock();
            try
            {
                m_TList.Clear();
            }

            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Returns true if the collection contains this item
        /// </summary>
        /// <param name="item">the item to find in the collection</param>
        public bool Contains(T item)
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.Contains(item);
            }

            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Converts the elements of the threaded list to another type, and returns a list of the new type
        /// </summary>
        /// <typeparam name="TOutput">the destination type</typeparam>
        /// <param name="converter">delegate to convert the items to a new type</param>
        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.ConvertAll(converter);
            }

            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Copies the elements of this threaded list to a one-dimension array of the same type
        /// </summary>
        /// <param name="array">the destination array</param>
        /// <param name="arrayIndex">index at which copying begins</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            LockList.EnterReadLock();
            try
            {
                m_TList.CopyTo(array, arrayIndex);
            }

            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Returns a count of the number of elements in this collection
        /// </summary>
        public int Count
        {
            get
            {
                LockList.EnterReadLock();
                try
                {
                    return m_TList.Count;
                }

                finally
                {
                    LockList.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Determines whether an item exists which meets the match criteria
        /// </summary>
        /// <param name="match">delegate that defines the conditions to search for</param>
        public bool Exists(Predicate<T> match)
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.Exists(match);
            }
            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Searches for an element that matches the criteria
        /// </summary>
        /// <param name="match">delegate that defines the conditions to search for</param>
        public T Find(Predicate<T> match)
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.Find(match);
            }
            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Searches for elements that match the criteria
        /// </summary>
        /// <param name="match">delegate that defines the conditions to search for</param>
        public List<T> FindAll(Predicate<T> match)
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.FindAll(match);
            }
            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Returns the index of the element which matches the criteria
        /// </summary>
        /// <param name="match">delegate that defines the conditions to search for</param>
        public int FindIndex(Predicate<T> match)
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.FindIndex(match);
            }
            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Returns the index of the element which matches the criteria
        /// </summary>
        /// <param name="startIndex">the zero-based index starting the search</param>
        /// <param name="match">delegate that defines the conditions to search for</param>
        public int FindIndex(int startIndex, Predicate<T> match)
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.FindIndex(startIndex, match);
            }
            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Returns the index of the element which matches the criteria
        /// </summary>
        /// <param name="startIndex">the zero-based index starting the search</param>
        /// <param name="count">the number of elements to search</param>
        /// <param name="match">delegate that defines the conditions to search for</param>
        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.FindIndex(startIndex, count, match);
            }
            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Searches for the last element in the collection that matches the criteria
        /// </summary>
        /// <param name="match">delegate that defines the conditions to search for</param>
        public T FindLast(Predicate<T> match)
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.FindLast(match);
            }
            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Returns the last index of the element which matches the criteria
        /// </summary>
        /// <param name="match">delegate that defines the conditions to search for</param>
        public int FindLastIndex(Predicate<T> match)
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.FindLastIndex(match);
            }
            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Returns the last index of the element which matches the criteria
        /// </summary>
        /// <param name="startIndex">the zero-based index starting the search</param>
        /// <param name="match">delegate that defines the conditions to search for</param>
        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.FindLastIndex(startIndex, match);
            }
            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Returns the last index of the element which matches the criteria
        /// </summary>
        /// <param name="startIndex">the zero-based index starting the search</param>
        /// <param name="count">the number of elements to search</param>
        /// <param name="match">delegate that defines the conditions to search for</param>
        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.FindLastIndex(startIndex, count, match);
            }
            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Peforms the action on each element of the list
        /// </summary>
        /// <param name="action">the action to perfom</param>
        public void ForEach(Action<T> action)
        {
            LockList.EnterWriteLock();
            try
            {
                m_TList.ForEach(action);
            }
            finally
            {
                LockList.ExitWriteLock();
            }
        }

        /// <summary>
        /// Creates a shallow copy of the range of elements in the source
        /// </summary>
        /// <param name="index">index to start from</param>
        /// <param name="count">number of elements to return</param>
        /// <returns></returns>
        public List<T> GetRange(int index, int count)
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.GetRange(index, count);
            }
            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Searches the list and returns the index of the item found in the list
        /// </summary>
        /// <param name="item">the item to find</param>
        public int IndexOf(T item)
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.IndexOf(item);
            }
            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Searches the list and returns the index of the item found in the list
        /// </summary>
        /// <param name="item">the item to find</param>
        /// <param name="index">the zero-based index to begin searching from</param>
        public int IndexOf(T item, int index)
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.IndexOf(item, index);
            }
            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Searches the list and returns the index of the item found in the list
        /// </summary>
        /// <param name="item">the item to find</param>
        /// <param name="index">the zero-based index to begin searching from</param>
        /// <param name="count">the number of elements to search</param>
        public int IndexOf(T item, int index, int count)
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.IndexOf(item, index, count);
            }
            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Inserts the item into the list
        /// </summary>
        /// <param name="index">the index at which to insert the item</param>
        /// <param name="item">the item to insert</param>
        public void Insert(int index, T item)
        {
            LockList.ExitWriteLock();
            try
            {
                m_TList.Insert(index, item);
            }
            finally
            {
                LockList.ExitWriteLock();
            }
        }

        /// <summary>
        /// Insert a range of objects into the list
        /// </summary>
        /// <param name="index">index to insert at</param>
        /// <param name="range">range of values to insert</param>
        public void InsertRange(int index, IEnumerable<T> range)
        {
            LockList.EnterWriteLock();
            try
            {
                m_TList.InsertRange(index, range);
            }
            finally
            {
                LockList.ExitWriteLock();
            }
        }

        /// <summary>
        /// Always false
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Returns the last index of the item in the list
        /// </summary>
        /// <param name="item">the item to find</param>
        public int LastIndexOf(T item)
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.LastIndexOf(item);
            }
            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Returns the last index of the item in the list
        /// </summary>
        /// <param name="item">the item to find</param>
        /// <param name="index">the index at which to start searching</param>
        public int LastIndexOf(T item, int index)
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.LastIndexOf(item, index);
            }
            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Returns the last index of the item in the list
        /// </summary>
        /// <param name="item">the item to find</param>
        /// <param name="index">the index at which to start searching</param>
        /// <param name="count">number of elements to search</param>
        public int LastIndexOf(T item, int index, int count)
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.LastIndexOf(item, index, count);
            }
            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Removes this item from the list
        /// </summary>
        /// <param name="item">the item to remove</param>
        public bool Remove(T item)
        {
            LockList.EnterWriteLock();
            try
            {
                return m_TList.Remove(item);
            }
            finally
            {
                LockList.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes all the matching items from the list
        /// </summary>
        /// <param name="match">the pattern to search on</param>
        public int RemoveAll(Predicate<T> match)
        {
            LockList.EnterWriteLock();
            try
            {
                return m_TList.RemoveAll(match);
            }
            finally
            {
                LockList.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes the item at the specified index
        /// </summary>
        /// <param name="index">the index of the item to remove</param>
        public void RemoveAt(int index)
        {
            LockList.EnterWriteLock();
            try
            {
                m_TList.RemoveAt(index);
            }
            finally
            {
                LockList.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes the items from the list
        /// </summary>
        /// <param name="index">the index of the item to begin removing</param>
        /// <param name="count">the number of items to remove</param>
        public void RemoveRange(int index, int count)
        {
            LockList.EnterWriteLock();
            try
            {
                m_TList.RemoveRange(index, count);
            }
            finally
            {
                LockList.ExitWriteLock();
            }
        }

        /// <summary>
        /// Reverses the order of elements in the list
        /// </summary>
        public void Reverse()
        {
            LockList.EnterWriteLock();
            try
            {
                m_TList.Reverse();
            }
            finally
            {
                LockList.ExitWriteLock();
            }
        }

        /// <summary>
        /// Reverses the order of elements in the list
        /// </summary>
        /// <param name="index">the index to begin reversing at</param>
        /// <param name="count">the number of elements to reverse</param>
        public void Reverse(int index, int count)
        {
            LockList.EnterWriteLock();
            try
            {
                m_TList.Reverse(index, count);
            }
            finally
            {
                LockList.ExitWriteLock();
            }
        }

        /// <summary>
        /// Sorts the items in the list
        /// </summary>
        public void Sort()
        {
            LockList.EnterWriteLock();
            try
            {
                m_TList.Sort();
            }
            finally
            {
                LockList.ExitWriteLock();
            }
        }

        /// <summary>
        /// Sorts the items in the list
        /// </summary>
        /// <param name="comparison">the comparison to use when comparing elements</param>
        public void Sort(Comparison<T> comparison)
        {
            LockList.EnterWriteLock();
            try
            {
                m_TList.Sort(comparison);
            }
            finally
            {
                LockList.ExitWriteLock();
            }
        }

        /// <summary>
        /// Sorts the items in the list
        /// </summary>
        /// <param name="comparer">the comparer to use when comparing elements</param>
        public void Sort(IComparer<T> comparer)
        {
            LockList.EnterWriteLock();
            try
            {
                m_TList.Sort(comparer);
            }
            finally
            {
                LockList.ExitWriteLock();
            }
        }

        /// <summary>
        /// Sorts the items in the list
        /// </summary>
        /// <param name="index">the index to begin sorting at</param>
        /// <param name="count">the number of elements to sort</param>
        /// <param name="comparer">the comparer to use when sorting</param>
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            LockList.EnterWriteLock();
            try
            {
                m_TList.Sort(index, count, comparer);
            }
            finally
            {
                LockList.ExitWriteLock();
            }
        }

        /// <summary>
        /// Copies the elements of the list to an array
        /// </summary>
        public T[] ToArray()
        {
            LockList.EnterReadLock();
            try
            {
                return m_TList.ToArray();
            }
            finally
            {
                LockList.ExitReadLock();
            }
        }

        /// <summary>
        /// Sets the capacity to the actual number of elements in the list, if that 
        /// number is less than the threshold
        /// </summary>
        public void TrimExcess()
        {
            LockList.EnterWriteLock();
            try
            {
                m_TList.TrimExcess();
            }
            finally
            {
                LockList.ExitWriteLock();
            }
        }

        /// <summary>
        /// Determines whether all members of the list matches the conditions in the predicate
        /// </summary>
        /// <param name="match">the delegate which defines the conditions for the search</param>
        public bool TrueForAll(Predicate<T> match)
        {
            LockList.EnterWriteLock();
            try
            {
                return m_TList.TrueForAll(match);
            }
            finally
            {
                LockList.ExitWriteLock();
            }
        }

        /// <summary>
        /// An item in the list
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get
            {
                LockList.EnterReadLock();
                try
                {
                    return m_TList[index];
                }

                finally
                {
                    LockList.ExitReadLock();
                }
            }
            set
            {
                LockList.EnterWriteLock();
                try
                {
                    m_TList[index] = value;
                }

                finally
                {
                    LockList.ExitWriteLock();
                }
            }
        }
    }
}
