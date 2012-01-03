/*
 * File Name: Recognizer.cs
 * Author   : Chi-En Wu
 * Date     : 2012/01/04
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Xml;

using Utils.DataStructure;

namespace Utils.NamedEntity
{
    /// <summary>
    /// An Recognizer that is used to find all named entities that occurs in content text.
    /// </summary>
    /// <typeparam name="T">The type of the information that describes for each named entity.</typeparam>
    public class NamedEntityRecognizer<T>
    {
        #region [Constructor(s)]

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedEntityRecognizer&lt;T&gt;"/> class.
        /// </summary>
        public NamedEntityRecognizer()
        {
            this.dictionary = new NamedEntityDictionary<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedEntityRecognizer&lt;T&gt;"/> class contains elements given by the specified IDictionary&lt;string, T&gt;.
        /// </summary>
        /// <param name="dictionary">The IDictionary&lt;string, T&gt; that used to initialize the new <see cref="NamedEntityRecognizer&lt;T&gt;"/>.</param>
        public NamedEntityRecognizer(IDictionary<string, T> dictionary)
        {
            this.dictionary = new NamedEntityDictionary<T>(dictionary);
        }

        #endregion

        #region[Field(s)]

        private NamedEntityDictionary<T> dictionary;

        #endregion

        #region[Property(ies)]

        /// <summary>
        /// An dictionary records information of named entities.
        /// </summary>
        public NamedEntityDictionary<T> Dictionary
        {
            get { return this.dictionary; }
        }

        #endregion

        #region [Public Method(s)]

        /// <summary>
        /// Recognizes named entities occurring in content text that recorded in the dictionary.
        /// </summary>
        /// <param name="content">The content string to recognize the named entities.</param>
        /// <returns>A set of <see cref="NamedEntityInfo&lt;T&gt;"/> represents named entities that is recognized from the content text.</returns>
        public ICollection<NamedEntityInfo<T>> Recognize(string content)
        {
            return this.dictionary.Recognize(content);
        }

        #endregion
    }

    /// <summary>
    /// An dictionary records information of named entities.
    /// </summary>
    /// <typeparam name="T">The type of the information that describes for each named entity.</typeparam>
    public class NamedEntityDictionary<T> : AbstractDictionary<string, T>
    {
        #region [Constructor(s)]

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedEntityDictionary&lt;T&gt;"/> class.
        /// </summary>
        public NamedEntityDictionary()
        {
            this.prefixTree = new PrefixTree<string, T>();
            this.dictionary = new Dictionary<string, T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedEntityDictionary&lt;T&gt;"/> class contains elements given by the specified IDictionary&lt;string, T&gt;.
        /// </summary>
        /// <param name="dictionary">The IDictionary&lt;string, T&gt; that used to initialize the new <see cref="NamedEntityDictionary&lt;T&gt;"/>.</param>
        public NamedEntityDictionary(IDictionary<string, T> dictionary) : this()
        {
            foreach (KeyValuePair<string, T> pair in dictionary)
            {
                this.Add(pair.Key, pair.Value);
            }
        }

        #endregion

        #region [Field(s)]

        private PrefixTree<string, T> prefixTree;
        private Dictionary<string, T> dictionary;

        #endregion

        #region [Property(ies)]

        /// <summary>
        /// Gets the number of key/value pairs contained in the dictionary.
        /// </summary>
        public override int Count
        {
            get { return this.dictionary.Count; }
        }

        /// <summary>
        /// Gets a collection containing the keys in the dictionary.
        /// </summary>
        public override ICollection<string> Keys
        {
            get { return this.dictionary.Keys; }
        }

        /// <summary>
        /// Gets a collection containing the values in the dictionary.
        /// </summary>
        public override ICollection<T> Values
        {
            get { return dictionary.Values; }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        public override T this[string key]
        {
            get { return this.dictionary[key.ToLower()]; }
            set
            {
                key = key.ToLower();
                this.prefixTree.TrySetValue(NamedEntityDictionary<T>.Tokenize(key), value);
                this.dictionary[key] = value;
            }
        }

        #endregion

        #region [Public Method(s)]

        /// <summary>
        /// Recognizes named entities occurring in content text that recorded in the dictionary.
        /// </summary>
        /// <param name="content">The content string to recognize the named entities.</param>
        /// <returns>A set of <see cref="NamedEntityInfo&lt;T&gt;"/> represents named entities that is recognized from the content text.</returns>
        public IList<NamedEntityInfo<T>> Recognize(string content)
        {
            content = content.ToLower();

            IList<string> tokens = NamedEntityDictionary<T>.Tokenize(content);
            IList<NamedEntityInfo<T>> result = new List<NamedEntityInfo<T>>();
            int contentIndex = 0;
            while (tokens.Count > 0)
            {
                PrefixMatch<string, T> match = this.prefixTree.LongestPrefixMatch(tokens);
                if (match.Length == 0)
                {
                    contentIndex += tokens[0].Length;
                    tokens.RemoveAt(0);
                    continue;
                }

                int termLength = 0;
                for (int index = 0; index < match.Length; index++)
                {
                    termLength += tokens[0].Length;
                    tokens.RemoveAt(0);
                }

                NamedEntityInfo<T> entity =
                    new NamedEntityInfo<T>(match.Value, contentIndex, termLength);
                result.Add(entity);
                contentIndex += termLength;
            }

            return result;
        }

        /// <summary>
        /// Updates the dictionary by the specified IDictionary&lt;string, T&gt;. If <paramref name="dictionary"/> contains keys including in the dictionary, the old value will be overwritten with the new one.
        /// </summary>
        /// <param name="dictionary">The IDictionary&lt;string, T&gt; that used to update the dictionary.</param>
        public void Update(IDictionary<string, T> dictionary)
        {
            foreach (KeyValuePair<string, T> pair in dictionary)
            {
                this[pair.Key] = pair.Value;
            }
        }

        /// <summary>
        /// Adds the specified key and value to the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        public override void Add(string key, T value)
        {
            key = key.ToLower();
            this.prefixTree.SetValue(NamedEntityDictionary<T>.Tokenize(key), value);
            this.dictionary.Add(key, value);
        }

        /// <summary>
        /// Removes all keys and values from the dictionary.
        /// </summary>
        public override void Clear()
        {
            this.dictionary.Clear();
            this.prefixTree.Clear();
        }

        /// <summary>
        /// Determines whether the dictionary contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the dictionary.</param>
        /// <returns>True if the dictionary contains an element with the specified key; otherwise, false.</returns>
        public override bool ContainsKey(string key)
        {
            return this.dictionary.ContainsKey(key.ToLower());
        }

        /// <summary>
        /// Removes the value with the specified key from the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>True if the element is successfully found and removed; otherwise, false. This method returns false if key is not found in the dictionary.</returns>
        public override bool Remove(string key)
        {
            key = key.ToLower();
            bool dictResult = this.dictionary.Remove(key);
            bool treeResult = this.prefixTree.Remove(NamedEntityDictionary<T>.Tokenize(key));
            Debug.Assert(dictResult == treeResult);
            return treeResult;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns>True if the dictionary contains an element with the specified key; otherwise, false.</returns>
        public override bool TryGetValue(string key, out T value)
        {
            return this.dictionary.TryGetValue(key.ToLower(), out value);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the dictionary.
        /// </summary>
        /// <returns>An <see cref="IEnumerator&lt;T&gt;"/> that can be used to iterate through the collection.</returns>
        public override IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }

        #endregion

        #region [Private Static Method(s)]

        private static IList<string> Tokenize(string content)
        {
            char[] symbols = @"_+-*/=|#.,:;!?'""()[] ".ToCharArray();

            List<string> result = new List<string>();
            int start = 0, end = 0;
            while ((end = content.IndexOfAny(symbols, start)) > 0)
            {
                if (start != end)
                {
                    result.Add(content.Substring(start, end - start));
                }

                result.Add(content[end].ToString());
                start = end + 1;
            }

            if (start != content.Length)
            {
                result.Add(content.Substring(start));
            }

            return result;
        }

        #endregion
    }

    /// <summary>
    /// Represents the result of named entity recognition.
    /// </summary>
    /// <typeparam name="T">The type of the information that describes for each named entity.</typeparam>
    public struct NamedEntityInfo<T>
    {
        #region [Constructor(s)]

        internal NamedEntityInfo(T info, int index, int length)
        {
            this.info = info;
            this.index = index;
            this.length = length;
        }

        #endregion

        #region [Field(s)]

        private T info;
        private int index;
        private int length;

        #endregion

        #region [Property(ies)]

        /// <summary>
        /// The information that describes for each named entity.
        /// </summary>
        public T Info
        {
            get { return this.info; }
        }

        /// <summary>
        /// The index of content text that the named entity occurs.
        /// </summary>
        public int Index
        {
            get { return this.index; }
        }

        /// <summary>
        /// The length of the named entity that occurs in the content text.
        /// </summary>
        public int Length
        {
            get { return this.length; }
        }

        #endregion
    }
}
