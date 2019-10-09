using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Trie;

namespace TrieTest
{
    [TestClass]
    public class TrieTest
    {
        [TestMethod]
        public void TestInsert()
        {
            var trie = new Trie<char, string>();
            trie["foo"] = "bar";
            Assert.IsTrue(trie.Contains(new KeyValuePair<IEnumerable<char>, string>("foo", "bar")));
            Assert.IsFalse(trie.Contains(new KeyValuePair<IEnumerable<char>, string>("foo", "foo")));
            Assert.IsFalse(trie.Contains(new KeyValuePair<IEnumerable<char>, string>("bar", "foo")));
            Assert.IsTrue(trie.ContainsKey("foo"));
            Assert.IsFalse(trie.ContainsKey("bar"));
            Assert.AreEqual(1, trie.Count);
            Assert.AreEqual("bar", trie["foo"]);

            Assert.IsTrue(trie.TryGetValue("foo", out var actual));
            Assert.AreEqual("bar", actual);

            Assert.IsFalse(trie.TryGetValue("baz", out var actual2));

            Assert.AreEqual(1, trie.Keys.Count);
            Assert.AreEqual(1, trie.Values.Count);
            Assert.IsTrue(trie.Keys.Select(a => string.Concat(a)).Contains("foo"));
            Assert.IsTrue(trie.Values.Select(a => string.Concat(a)).Contains("bar"));
        }

        [TestMethod]
        public void TestRemove()
        {
            var trie = new Trie<char, string>();
            trie["foo"] = "bar";
            trie["bar"] = "foo";
            trie.Remove("baz");
            Assert.AreEqual(2, trie.Count);
            trie.Remove("foo");
            Assert.AreEqual(1, trie.Count);
            trie.Remove("bar");
            Assert.AreEqual(0, trie.Count);
        }

        [TestMethod]
        public void TestClear()
        {
            var trie = new Trie<char, string>();
            trie["foo"] = "bar";
            trie.Clear();
            Assert.AreEqual(0, trie.Count);
        }

        [TestMethod]
        public void TestIteration()
        {
            var trie = new Trie<char, string>();
            trie["foo"] = "bar";
            trie["bar"] = "foo";
            trie["a"] = "a";
            trie["ab"] = "ab";
            trie["abc"] = "abc";

            var dic = new Dictionary<string, string>
            {
                { "foo", "bar"},
                {"bar", "foo" },
                { "a", "a"},
                { "ab", "ab"},
                { "abc", "abc"},
            };

            foreach (var v in trie)
            {
                Assert.IsTrue(dic.ContainsKey(string.Concat(v.Key)));
                Assert.AreEqual(dic[string.Concat(v.Key)], v.Value);
            }
        }

        [TestMethod]
        public void TestByPrefix()
        {
            var trie = new Trie<char, string>();
            trie["ab"] = "ab";
            trie["abc"] = "abc";
            trie["abcd"] = "abcd";

            Assert.AreEqual(3, trie.ByPrefix("").Count());
            Assert.AreEqual(3, trie.ByPrefix("a").Count());
            Assert.AreEqual(3, trie.ByPrefix("ab").Count());
            Assert.AreEqual(2, trie.ByPrefix("abc").Count());
            Assert.AreEqual(1, trie.ByPrefix("abcd").Count());
            Assert.AreEqual(0, trie.ByPrefix("abcde").Count());
        }

        [TestMethod]
        public void TestLongestPrefix()
        {
            var trie = new Trie<char, string>();
            trie["ab"] = "ab";
            trie["abcd"] = "abcd";

            Assert.AreEqual("", string.Concat(trie.LongestPrefix("foo")));
            Assert.AreEqual("", string.Concat(trie.LongestPrefix("a")));
            Assert.AreEqual("ab", string.Concat(trie.LongestPrefix("abc")));
            Assert.AreEqual("abcd", string.Concat(trie.LongestPrefix("abcdefg")));
        }

        [TestMethod]
        public void TestCopyTo()
        {
            var trie = new Trie<char, string>();
            trie["ab"] = "ab";
            trie["abcd"] = "abcd";

            var arr = new KeyValuePair<IEnumerable<char>, string>[2];

            trie.CopyTo(arr, 0);

            Assert.AreEqual("ab", string.Concat(arr[0].Key));
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [TestMethod]
        public void TestCopyToFail()
        {
            var trie = new Trie<char, string>();
            trie["ab"] = "ab";
            trie["abcd"] = "abcd";

            var arr = new KeyValuePair<IEnumerable<char>, string>[0];

            trie.CopyTo(arr, 0);
        }

        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [TestMethod]
        public void TestIndexingFail()
        {
            var trie = new Trie<char, string>();

            var v = trie["foo"];
        }
    }
}
