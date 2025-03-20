using System;
using System.Collections.Concurrent;

namespace Nito.AsyncEx.AsyncDiagnostics.Internal
{
    internal static class CallContext
    {
        static ConcurrentDictionary<string, AsyncLocal<object>> state = new ConcurrentDictionary<string, AsyncLocal<object>>();

        public static void LogicalSetData(string name, object data)
        {
            state.GetOrAdd(name, _ => new AsyncLocal<object>(data));
        }

        public static object LogicalGetData(string name)
        {
            return state.TryGetValue(name, out AsyncLocal<object> data) ? data.Value : null;
        }

        public static void FreeNamedDataSlot(string name)
        {
            state.TryRemove(name, out var value);
        }
    }

    /// <summary>
    /// Data that is "local" to the current async method. This is the async equivalent of <c>ThreadLocal&lt;T&gt;</c>.
    /// </summary>
    /// <typeparam name="TImmutableType">The type of the data. This must be an immutable type.</typeparam>
    internal class AsyncLocal<TImmutableType> : IDisposable
    {
        /// <summary>
        /// Our unique slot name.
        /// </summary>
        private readonly string _slotName = Guid.NewGuid().ToString("N");

        /// <summary>
        /// The value representing "none".
        /// </summary>
        private readonly TImmutableType _empty;

        /// <summary>
        /// Creates a new async-local variable with "empty" defined as default value of <typeparamref name="TImmutableType"/>.
        /// </summary>
        public AsyncLocal()
            : this(default(TImmutableType))
        {
        }

        /// <summary>
        /// Creates a new async-local variable with "empty" defined as the specified value of <typeparamref name="TImmutableType"/>.
        /// </summary>
        /// <param name="empty">The value to return when there is no data yet.</param>
        public AsyncLocal(TImmutableType empty)
        {
            _empty = empty;
        }

        /// <summary>
        /// Gets or sets the value of this async-local variable.
        /// </summary>
        public TImmutableType Value
        {
            get
            {
                var ret = CallContext.LogicalGetData(_slotName);
                if (ret is TImmutableType)
                    return (TImmutableType)ret;
                return _empty;
            }

            set
            {
                CallContext.LogicalSetData(_slotName, value);
            }
        }

        /// <summary>
        /// Deletes this async-local variable.
        /// </summary>
        public void Dispose()
        {
            CallContext.FreeNamedDataSlot(_slotName);
        }
    }
}