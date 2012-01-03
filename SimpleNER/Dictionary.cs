/*
 * File Name: Dictionary.cs
 * Author   : Chi-En Wu
 * Date     : 2012/01/04
 */

using System;
using System.Collections.Generic;

namespace Utils.DataStructure
{
    /// <summary>
    /// Provides basic implement of <see cref="IDictionary&lt;TKey, TValue&gt;"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    public abstract class AbstractDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        #region [Abstract Property(ies)]

        /// <summary>
        /// Gets the number of key/value pairs contained in the dictionary.
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Gets a collection containing the keys in the dictionary.
        /// </summary>
        public abstract ICollection<TKey> Keys { get;}

        /// <summary>
        /// Gets a collection containing the values in the dictionary.
        /// </summary>
        public abstract ICollection<TValue> Values { get; }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        public abstract TValue this[TKey key] { get; set; }

        #endregion

        #region [Public Abstract Method(s)]

        /// <summary>
        /// Adds the specified key and value to the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        public abstract void Add(TKey key, TValue value);

        /// <summary>
        /// Removes all keys and values from the dictionary.
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Determines whether the dictionary contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the dictionary.</param>
        /// <returns>True if the dictionary contains an element with the specified key; otherwise, false.</returns>
        public abstract bool ContainsKey(TKey key);

        /// <summary>
        /// Removes the value with the specified key from the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>True if the element is successfully found and removed; otherwise, false. This method returns false if key is not found in the dictionary.</returns>
        public abstract bool Remove(TKey key);

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns>True if the dictionary contains an element with the specified key; otherwise, false.</returns>
        public abstract bool TryGetValue(TKey key, out TValue value);

        /// <summary>
        /// Returns an enumerator that iterates through the dictionary.
        /// </summary>
        /// <returns>An <see cref="IEnumerator&lt;T&gt;"/> that can be used to iterate through the collection.</returns>
        public abstract IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();

        #endregion

        #region [Private Method(s)]

        private bool Contains(KeyValuePair<TKey, TValue> item)
        {
            TValue value;
            if (!this.TryGetValue(item.Key, out value))
                return false;
            return EqualityComparer<TValue>.Default.Equals(value, item.Value);
        }

        #endregion

        #region [IDictionary<TKey, TValue> Member(s)]

        /// <summary>
        /// Gets an <see cref="ICollection&lt;T&gt;"/> containing the keys of the <see cref="IDictionary&lt;TKey, TValue&gt;"/>.
        /// </summary>
        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get { return this.Keys; }
        }

        /// <summary>
        /// Gets an <see cref="ICollection&lt;T&gt;"/> containing the values of the <see cref="IDictionary&lt;TKey, TValue&gt;"/>.
        /// </summary>
        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get { return this.Values; }
        }

        #endregion

        #region [ICollection<KeyValuePair<TKey, TValue>> Member(s)]

        /// <summary>
        /// Gets a value indicating whether the dictionary is read-only.
        /// </summary>
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Adds the specified value to the <see cref="ICollection&lt;T&gt;"/> with the specified key.
        /// </summary>
        /// <param name="item">The <see cref="KeyValuePair&lt;TKey, TValue&gt;"/> structure representing the key and value to add to the dictionary.</param>
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Determines whether the <see cref="ICollection&lt;T&gt;"/> contains a specific key and value.
        /// </summary>
        /// <param name="item">The <see cref="KeyValuePair&lt;TKey, TValue&gt;"/> structure to locate in the <see cref="ICollection&lt;T&gt;"/>.</param>
        /// <returns>True if <paramref name="item"/> is found in the <see cref="ICollection&lt;T&gt;"/>; otherwise, false.</returns>
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return this.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="ICollection&lt;T&gt;"/> to an array of type <see cref="KeyValuePair&lt;TKey, TValue&gt;"/>, starting at the specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional array of type <see cref="KeyValuePair&lt;TKey, TValue&gt;"/> that is the destination of the <see cref="KeyValuePair&lt;TKey, TValue&gt;"/> elements copied from the <see cref="ICollection&lt;T&gt;"/>. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException("arrayIndex");

            if ((array.Length - arrayIndex) < this.Count)
                throw new ArgumentException("Destination array is not large enough. Check array.Length and arrayIndex.");

            foreach (KeyValuePair<TKey, TValue> item in this)
                array[arrayIndex++] = item;
        }

        /// <summary>
        /// Removes a key and value from the dictionary.
        /// </summary>
        /// <param name="item">The <see cref="KeyValuePair&lt;TKey, TValue&gt;"/> structure representing the key and value to remove from the dictionary.</param>
        /// <returns>True if the key and value represented by <paramref name="item"/> is successfully found and removed; otherwise, false. This method returns false if <paramref name="item"/> is not found in the <see cref="ICollection&lt;T&gt;"/>.</returns>
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!this.Contains(item))
                return false;
            return this.Remove(item.Key);
        }

        #endregion

        #region [IEnumerable Member(s)]

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An <see cref="System.Collections.IEnumerator"/> that can be used to iterate through the collection.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((AbstractDictionary<TKey, TValue>)this).GetEnumerator();
        }

        #endregion
    }
}
