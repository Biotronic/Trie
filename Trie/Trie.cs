using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Trie
{
    public class Trie<TKey, TValue> : IDictionary<IEnumerable<TKey>, TValue>
    {
        [DebuggerDisplay("{DebuggerDisplay()}")]
        private class Node : IComparable<Node>
        {
            private string DebuggerDisplay()
            {
                if (HasValue)
                    return $"{Key}: {Value}";
                return $"{Key} : (null)";
            }

            public Node(TKey key)
            {
                Key = key;
                Value = default;
                HasValue = false;
                Children = new List<Node>();
            }

            private TKey Key { get; }
            public bool HasValue { get; private set; }
            public TValue Value { get; private set; }

            public List<Node> Children { get; }

            public int Count => Children.Sum(c => c.Count) + (HasValue ? 1 : 0);

            private Node GetChild(TKey key)
            {
                return Children.FirstOrDefault(c => EqualityComparer<TKey>.Default.Equals(c.Key, key));
            }

            public Node Get(IEnumerator<TKey> key)
            {
                if (!key.MoveNext())
                {
                    return this;
                }

                return GetChild(key.Current)?.Get(key);
            }

            public Node Add(IEnumerator<TKey> key, TValue value)
            {
                if (!key.MoveNext())
                {
                    HasValue = true;
                    Value = value;
                    return this;
                }

                var child = GetChild(key.Current);
                if (child == null)
                {
                    child = new Node(key.Current);
                    Children.Add(child);
                    Children.Sort();
                }

                return child.Add(key, value);
            }

            public bool Remove(IEnumerator<TKey> key)
            {
                if (!key.MoveNext())
                {
                    HasValue = false;
                    return true;
                }

                var child = GetChild(key.Current);
                if (child != null)
                {
                    child.Remove(key);
                    if (!child.HasValue && !child.Children.Any())
                        return Children.Remove(child);
                }

                return false;
            }

            [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
            public IEnumerable<TKey> GetPrefix(IEnumerable<TKey> key, IEnumerator<TKey> remaining)
            {
                if (!remaining.MoveNext())
                {
                    return HasValue ? key : null;
                }

                var child = GetChild(remaining.Current);
                var result = child?.GetPrefix(key.Append(child.Key), remaining);

                return result ?? (HasValue ? key : null);
            }

            public IEnumerable<KeyValuePair<IEnumerable<TKey>, TValue>> Enumerate(IEnumerable<TKey> key)
            {
                var childValues = Children.SelectMany(c => c.Enumerate(key.Append(c.Key)));
                return HasValue
                    ? childValues.Prepend(new KeyValuePair<IEnumerable<TKey>, TValue>(key, Value))
                    : childValues;
            }

            public int CompareTo(Node other)
            {
                if (ReferenceEquals(this, other)) return 0;
                if (ReferenceEquals(null, other)) return 1;

                return (Key as IComparable)?.CompareTo(other.Key as IComparable) ?? 0;
            }
        }

        private readonly Node _root = new Node(default);

        public IEnumerator<KeyValuePair<IEnumerable<TKey>, TValue>> GetEnumerator()
        {
            return _root.Enumerate(new TKey[0]).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<IEnumerable<TKey>, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _root.Children.Clear();
        }

        public bool Contains(KeyValuePair<IEnumerable<TKey>, TValue> item)
        {
            return TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);
        }

        public void CopyTo(KeyValuePair<IEnumerable<TKey>, TValue>[] array, int arrayIndex)
        {
            foreach (var pair in this)
            {
                if (arrayIndex >= array.Length) throw new ArgumentOutOfRangeException($"Index {arrayIndex} is out of range for array being copied to.");
                array[arrayIndex++] = pair;
            }
        }

        public bool Remove(KeyValuePair<IEnumerable<TKey>, TValue> item)
        {
            return Remove(item.Key);
        }

        public int Count => _root.Count;
        public bool IsReadOnly => false;

        public void Add(IEnumerable<TKey> key, TValue value)
        {
            var enumerator = key.GetEnumerator();
            _root.Add(enumerator, value);
        }

        public bool ContainsKey(IEnumerable<TKey> key)
        {
            using (var enumerator = key.GetEnumerator())
                return _root.Get(enumerator) != null;
        }

        public bool Remove(IEnumerable<TKey> key)
        {
            using (var enumerator = key.GetEnumerator())
                return _root.Remove(enumerator);
        }

        public bool TryGetValue(IEnumerable<TKey> key, out TValue value)
        {
            using (var enumerator = key.GetEnumerator())
            {
                var node = _root.Get(enumerator);

                value = node?.HasValue == true ? node.Value : default;
                return node?.HasValue == true;
            }
        }

        public TValue this[IEnumerable<TKey> key]
        {
            get => TryGetValue(key, out var value) ? value : throw new ArgumentOutOfRangeException($"Key {key} not found in collection");
            set => Add(key, value);
        }

        public ICollection<IEnumerable<TKey>> Keys => _root.Enumerate(new TKey[0]).Select(a => a.Key).ToList();
        public ICollection<TValue> Values => _root.Enumerate(new TKey[0]).Select(a => a.Value).ToList();

        public IEnumerable<KeyValuePair<IEnumerable<TKey>, TValue>> ByPrefix(IEnumerable<TKey> prefix)
        {
            using (var enumerator = prefix.GetEnumerator())
            {
                var node = _root.Get(enumerator);
                return node?.Enumerate(new TKey[0]) ?? new KeyValuePair<IEnumerable<TKey>, TValue>[0];
            }
        }

        public IEnumerable<TKey> LongestPrefix(IEnumerable<TKey> key)
        {
            using (var enumerator = key.GetEnumerator())
            {
                return _root.GetPrefix(new TKey[0], enumerator) ?? new TKey[0];
            }
        }
    }
}
