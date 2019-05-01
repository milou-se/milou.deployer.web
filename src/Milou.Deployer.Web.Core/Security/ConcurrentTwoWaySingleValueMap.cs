using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Milou.Deployer.Web.Core.Security
{
    public class ConcurrentTwoWaySingleValueMap<T1, T2> where T1 : class where T2 : class
    {
        private readonly ConcurrentDictionary<T1, T2> _forward = new ConcurrentDictionary<T1, T2>();
        private readonly ConcurrentDictionary<T2, T1> _reverse = new ConcurrentDictionary<T2, T1>();

        public ImmutableArray<T1> ForwardKeys => _forward.Keys.ToImmutableArray();
        public ImmutableArray<T2> ReverseKeys => _reverse.Keys.ToImmutableArray();

        public bool TrySet([NotNull] T1 t1, [NotNull] T2 t2)
        {
            if (t1 == null)
            {
                throw new ArgumentNullException(nameof(t1));
            }

            if (t2 == null)
            {
                throw new ArgumentNullException(nameof(t2));
            }

            bool forwardSet;
            bool reverseSet;

            T2 oldValue = null;

            if (!_forward.ContainsKey(t1))
            {
                forwardSet = _forward.TryAdd(t1, t2);
            }
            else
            {
                if (_forward.TryGetValue(t1, out var ov))
                {
                    oldValue = ov;
                }

                _forward[t1] = t2;
                forwardSet = true;
            }

            if (oldValue != null && _reverse.ContainsKey(oldValue) && !t2.Equals(oldValue))
            {
                _reverse.TryRemove(oldValue, out _);
            }

            if (t2.Equals(oldValue))
            {
                reverseSet = true;
            }
            else
            {
                if (!_reverse.ContainsKey(t2))
                {
                    reverseSet = _reverse.TryAdd(t2, t1);
                }
                else
                {
                    _reverse[t2] = t1;
                    reverseSet = true;
                }
            }

            return forwardSet && reverseSet;
        }
    }
}
