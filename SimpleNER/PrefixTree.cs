/*
 * File Name: PrefixTree.cs
 * Author   : Chi-En Wu
 * Date     : 2012/01/04
 */

using System;
using System.Collections.Generic;

namespace Utils.DataStructure
{
    /// <summary>
    /// Implements the prefix tree data structure that supports longest prefix matching.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the prefix tree.</typeparam>
    /// <typeparam name="TValue">The type of the values in the prefix tree.</typeparam>
    public class PrefixTree<TKey, TValue>
    {
        #region [Constructor(s)]

        /// <summary>
        /// Initializes a new instance of the <see cref="PrefixTree&lt;TKey, TValue&gt;"/> class.
        /// </summary>
        public PrefixTree()
        {
            this.collection = new PrefixTreeNodeCollection<TKey, TValue>();
        }

        #endregion

        #region [Field(s)]

        private PrefixTreeNodeCollection<TKey, TValue> collection;

        #endregion

        #region [Public Method(s)]

        /// <summary>
        /// Removes all nodes from the prefix tree.
        /// </summary>
        public void Clear()
        {
            this.collection.Clear();
        }

        /// <summary>
        /// Removes the prefix node with the specified path from the tree.
        /// </summary>
        /// <param name="enumerable">An enumerable object that specifies the path of the node to remove.</param>
        /// <returns>True if the node is successfully found and removed; otherwise, false. This method returns false if the specified node is not found in the tree.</returns>
        public bool Remove(IEnumerable<TKey> enumerable)
        {
            Stack<KeyValuePair<TKey, PrefixTreeNodeCollection<TKey, TValue>>> track
                = new Stack<KeyValuePair<TKey, PrefixTreeNodeCollection<TKey, TValue>>>();
            foreach (KeyValuePair<TKey, PrefixTreeNodeCollection<TKey, TValue>> pair
                in this.PrefixNodePath(enumerable))
            {
                if (!pair.Value.ContainsKey(pair.Key)) { return false; }

                track.Push(pair);
            }

            if (track.Count == 0) { return false; }

            KeyValuePair<TKey, PrefixTreeNodeCollection<TKey, TValue>> top = track.Pop();
            bool hasChild = top.Value[top.Key].Children.Count > 0;
            top.Value[top.Key].ResetValue();
            if (!hasChild)
            {
                while (track.Count > 0)
                {
                    KeyValuePair<TKey, PrefixTreeNodeCollection<TKey, TValue>> pair = track.Pop();
                    if (pair.Value.Count > 1 || pair.Value[pair.Key].HasValue) { break; }

                    pair.Value.Remove(pair.Key);
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether the tree contains the specified node.
        /// </summary>
        /// <param name="enumerable">An enumerable object that specifies the path of the node.</param>
        /// <returns>True if the tree contains the specified node; otherwise, false.</returns>
        public bool HasValue(IEnumerable<TKey> enumerable)
        {
            PrefixTreeNode<TKey, TValue> currentNode = this.GetMatchedNode(enumerable);
            return currentNode != null && currentNode.HasValue;
        }

        /// <summary>
        /// Gets the node value associated with the specified path.
        /// </summary>
        /// <param name="enumerable">An enumerable object that specifies the path of the node to get.</param>
        /// <returns>The node value associated with the specified path</returns>
        public TValue GetValue(IEnumerable<TKey> enumerable)
        {
            PrefixTreeNode<TKey, TValue> currentNode = this.GetMatchedNode(enumerable);
            return currentNode.Value;
        }

        /// <summary>
        /// Gets the node value associated with the specified path.
        /// </summary>
        /// <param name="enumerable">An enumerable object that specifies the path of the node to get.</param>
        /// <param name="value">When this method returns, contains the node value associated with the specified path, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns>True if the tree contains the specified node; otherwise, false.</returns>
        public bool TryGetValue(IEnumerable<TKey> enumerable, out TValue value)
        {
            PrefixTreeNode<TKey, TValue> currentNode = this.GetMatchedNode(enumerable);
            bool hasValue = currentNode != null && currentNode.HasValue;

            value = hasValue ? currentNode.Value : default(TValue);
            return hasValue;
        }

        /// <summary>
        /// Sets the node value associated with the specified path.
        /// </summary>
        /// <param name="enumerable">An enumerable object that specifies the path of the node to set.</param>
        /// <param name="value">The value of the node to set.</param>
        public void SetValue(IEnumerable<TKey> enumerable, TValue value)
        {
            PrefixTreeNode<TKey, TValue> currentNode = this.GetMatchedNode(enumerable, true);
            currentNode.Value = value;
        }

        /// <summary>
        /// Sets the node value associated with the specified path if that node doesn't exist.
        /// </summary>
        /// <param name="enumerable">An enumerable object that specifies the path of the node to set.</param>
        /// <param name="value">The value of the node to set.</param>
        /// <returns>True if the set operation is success; otherwise, false. This method returns false if the specified node is already existed in the tree.</returns>
        public bool TrySetValue(IEnumerable<TKey> enumerable, TValue value)
        {
            PrefixTreeNode<TKey, TValue> currentNode = this.GetMatchedNode(enumerable, true);
            if (currentNode.HasValue) { return false; }

            currentNode.Value = value;
            return true;
        }

        /// <summary>
        /// Applies the longest prefix match algorithm to the given enumerable object.
        /// </summary>
        /// <param name="enumerable">An enumerable to find a longest matching in the prefix tree.</param>
        /// <returns>A <see cref="PrefixMatch&lt;TKey, TValue&gt;"/> that represents the result of the longest prefix matching.</returns>
        public PrefixMatch<TKey, TValue> LongestPrefixMatch(IEnumerable<TKey> enumerable)
        {
            PrefixTreeNode<TKey, TValue> longestNode = null;
            int currentLength = 1, longestLength = 0;
            foreach (KeyValuePair<TKey, PrefixTreeNodeCollection<TKey, TValue>> pair
                in this.PrefixNodePath(enumerable))
            {
                if (!pair.Value.ContainsKey(pair.Key)) { break; }

                if (pair.Value[pair.Key].HasValue)
                {
                    longestNode = pair.Value[pair.Key];
                    longestLength = currentLength;
                }

                currentLength++;
            }

            return new PrefixMatch<TKey, TValue>(longestNode, longestLength);
        }

        #endregion

        #region [Private Method(s)]

        private PrefixTreeNode<TKey, TValue> GetMatchedNode(IEnumerable<TKey> enumerable)
        {
            return this.GetMatchedNode(enumerable, false);
        }

        private PrefixTreeNode<TKey, TValue> GetMatchedNode(IEnumerable<TKey> enumerable, bool createNew)
        {
            PrefixTreeNode<TKey, TValue> currentNode = null;
            foreach (KeyValuePair<TKey, PrefixTreeNodeCollection<TKey, TValue>> pair
                in this.PrefixNodePath(enumerable))
            {
                if (!pair.Value.ContainsKey(pair.Key))
                {
                    if (!createNew) { return null; }

                    pair.Value[pair.Key] = new PrefixTreeNode<TKey, TValue>();
                }

                currentNode = pair.Value[pair.Key];
            }

            return currentNode;
        }

        private IEnumerable<KeyValuePair<TKey, PrefixTreeNodeCollection<TKey, TValue>>>
            PrefixNodePath(IEnumerable<TKey> enumerable)
        {
            PrefixTreeNodeCollection<TKey, TValue> currentNodeCollection = this.collection;

            IEnumerator<TKey> enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                TKey prefix = enumerator.Current;
                yield return new KeyValuePair<TKey, PrefixTreeNodeCollection<TKey, TValue>>
                    (prefix, currentNodeCollection);
                if (!currentNodeCollection.ContainsKey(prefix))
                {
                    yield break;
                }

                currentNodeCollection = currentNodeCollection[prefix].Children;
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents the result of the longest prefix matching.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the prefix tree.</typeparam>
    /// <typeparam name="TValue">The type of the values in the prefix tree.</typeparam>
    public class PrefixMatch<TKey, TValue>
    {
        #region [Constructor(s)]

        internal PrefixMatch(PrefixTreeNode<TKey, TValue> node, int length)
        {
            this.node = node;
            this.length = length;
        }

        #endregion

        #region [Field(s)]

        private int length;
        private PrefixTreeNode<TKey, TValue> node;

        #endregion

        #region [Property(ies)]

        /// <summary>
        /// The length of the longest path that matches the prefix of content.
        /// </summary>
        public int Length
        {
            get { return this.length; }
        }

        /// <summary>
        ///  The value of node that matches the longest prefix.
        /// </summary>
        public TValue Value
        {
            get { return this.node.Value; }
            set { this.node.Value = value; }
        }

        #endregion
    }

    class PrefixTreeNode<TKey, TValue>
    {
        #region [Constructor(s)]

        public PrefixTreeNode()
            : this(default(TValue), false)
        {
            // do nothing
        }

        public PrefixTreeNode(TValue value)
            : this(value, true)
        {
            // do nothing
        }

        private PrefixTreeNode(TValue value, bool hasValue)
        {
            this.children = new PrefixTreeNodeCollection<TKey, TValue>();
            this.value = value;
            this.hasValue = hasValue;
        }

        #endregion

        #region [Field(s)]

        private bool hasValue;
        private TValue value;
        private PrefixTreeNodeCollection<TKey, TValue> children;

        #endregion

        #region [Property(ies)]

        public bool HasValue
        {
            get { return this.hasValue; }
        }

        public TValue Value
        {
            get
            {
                if (!this.hasValue)
                    throw new InvalidOperationException();
                return this.value;
            }
            set
            {
                this.hasValue = true;
                this.value = value;
            }
        }

        public PrefixTreeNodeCollection<TKey, TValue> Children
        {
            get { return this.children; }
        }

        #endregion

        #region [Public Method(s)]

        public void ResetValue()
        {
            this.hasValue = false;
        }

        #endregion
    }

    class PrefixTreeNodeCollection<TKey, TValue>
        : AbstractDictionary<TKey, PrefixTreeNode<TKey, TValue>>
    {
        #region [Constructor(s)]

        public PrefixTreeNodeCollection()
        {
            this.collection = new Dictionary<TKey, PrefixTreeNode<TKey, TValue>>();
        }

        public PrefixTreeNodeCollection(PrefixTreeNodeCollection<TKey, TValue> collection)
        {
            this.collection = collection.collection;
        }

        #endregion

        #region [Field(s)]

        private Dictionary<TKey, PrefixTreeNode<TKey, TValue>> collection;

        #endregion

        #region [Property(ies)]

        public override PrefixTreeNode<TKey, TValue> this[TKey key]
        {
            get { return this.collection[key]; }
            set { this.collection[key] = value; }
        }

        public override ICollection<TKey> Keys
        {
            get { return this.collection.Keys; }
        }

        public override ICollection<PrefixTreeNode<TKey, TValue>> Values
        {
            get { return this.collection.Values; }
        }

        public override int Count
        {
            get { return this.collection.Count; }
        }

        #endregion

        #region [Public Method(s)]

        public override void Add(TKey key, PrefixTreeNode<TKey, TValue> value)
        {
            this.collection.Add(key, value);
        }

        public override void Clear()
        {
            this.collection.Clear();
        }

        public override bool ContainsKey(TKey key)
        {
            return this.collection.ContainsKey(key);
        }

        public override bool Remove(TKey key)
        {
            return this.collection.Remove(key);
        }

        public override bool TryGetValue(TKey key, out PrefixTreeNode<TKey, TValue> value)
        {
            return this.collection.TryGetValue(key, out value);
        }

        public override IEnumerator<KeyValuePair<TKey, PrefixTreeNode<TKey, TValue>>> GetEnumerator()
        {
            return this.collection.GetEnumerator();
        }

        #endregion
    }
}
