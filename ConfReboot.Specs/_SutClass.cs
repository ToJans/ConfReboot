using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ConfReboot.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConfReboot.Specs
{
    public class SUTClass : Bus
    {
        MessageStore ms = new MessageStore();

        public List<string> published = new List<string>();

        public Exception e = null;

        public void Given(params Action<dynamic>[] events)
        {
            foreach (var e in events)
            {
                ms.Add(Message.FromAction(e));
            }
        }

        public void When(Action<dynamic> command)
        {
            try
            {
                e = null;
                base.HandleUntilAllConsumed(Message.FromAction(command), x => { published.Add(x.ToFriendlyString()); ms.Add(x); }, ms.Filter);
            }
            catch (TargetInvocationException e)
            {
                this.e = e.InnerException;
            }
            catch (Exception e)
            {
                this.e = e;
            }
        }

        public void Then(params Action<dynamic>[] events)
        {
            Assert.IsNull(e, "Received an exception but did not expect one:" + (e ?? new Exception()).Message);
            foreach (string evt in events.Select(x => Message.FromAction(x).ToFriendlyString()))
            {
                var match = published.Where(x => x == evt).FirstOrDefault();
                var similar = match;
                if (match == null)
                {
                    var simarr = published.Select(x => new { dist = LevenshteinDistance(evt, x), msg = x }).OrderBy(x => x.dist).Select(x => x.msg).Take(3).ToArray();
                    similar = string.Join("\n", simarr);
                }

                Assert.AreEqual(evt, match, "Event missing; 3 best match are: " + similar);
            }
        }

        internal void ThenException<T>(Predicate<T> assertion = null) where T : Exception
        {
            Assert.IsInstanceOfType(e, typeof(T), "Expected an exception, but did not receive one");
            if (assertion != null)
                Assert.IsTrue(assertion(e as T), "The found exception but it does not match the assertion");
        }

        // from: http://www.dotnetperls.com/levenshtein
        private static int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }
    }
}