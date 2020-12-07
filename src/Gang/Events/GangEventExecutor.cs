using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Gang.Events
{
    /// <summary>
    /// Executor for handlers for the given data type
    /// </summary>
    /// <typeparam name="TDataImplements">Implemented by all data</typeparam>
    public sealed class GangEventExecutor<TDataImplements>
    {
        readonly IImmutableList<GangEventHandler<TDataImplements>> _allHandlers;
        readonly IImmutableDictionary<Type, ImmutableList<Func<TDataImplements, Task>>> _handlers;

        public GangEventExecutor(
            IEnumerable<GangEventHandler<TDataImplements>> handlers = null
            )
        {
            _allHandlers = handlers.ToImmutableListDefaultEmpty();

            _handlers = _allHandlers.Aggregate(
                ImmutableDictionary<Type, ImmutableList<Func<TDataImplements, Task>>>.Empty,
                (result, GangEventHandler) =>
                {
                    if (!result.ContainsKey(GangEventHandler.DataType))
                        return result.Add(
                            GangEventHandler.DataType,
                            ImmutableList<Func<TDataImplements, Task>>.Empty.Add(GangEventHandler.HandleAsync)
                            );


                    return result.SetItem(
                        GangEventHandler.DataType,
                        result[GangEventHandler.DataType].Add(GangEventHandler.HandleAsync)
                        );
                });

            DataTypes = _handlers.Keys.ToImmutableList();
        }

        /// <summary>
        /// All handled data types
        /// </summary>
        public IImmutableList<Type> DataTypes { get; }

        /// <summary>
        /// Add a delegate/method as a GangEventHandler
        /// </summary>
        /// <typeparam name="TData">Type of data</typeparam>
        /// <param name="handle">Handle delegate/method</param>
        /// <returns>A new Executor</returns>
        public GangEventExecutor<TDataImplements> AddHandler<TData>(Action<TData> handle)
            where TData : class, TDataImplements
        {
            return new GangEventExecutor<TDataImplements>(
                    _allHandlers.Add(GangEventHandler<TDataImplements>.From(handle))
                );
        }

        /// <summary>
        /// Add a delegate/method as a GangEventHandler
        /// </summary>
        /// <typeparam name="TData">Type of data</typeparam>
        /// <param name="handle">Handle delegate/method</param>
        /// <returns>A new Executor</returns>
        public GangEventExecutor<TDataImplements> AddHandler<TData>(Func<TData, Task> handle)
            where TData : class, TDataImplements
        {
            return new GangEventExecutor<TDataImplements>(
                    _allHandlers.Add(GangEventHandler<TDataImplements>.From(handle))
                );
        }

        /// <summary>
        /// Add a GangEventHandler
        /// </summary>
        /// <typeparam name="TData">Type of data</typeparam>
        /// <param name="GangEventHandler">A GangEventHandler</param>
        /// <returns>A new Executor</returns>
        public GangEventExecutor<TDataImplements> AddHandler<TData>(IGangEventHandler<TData> GangEventHandler)
            where TData : class, TDataImplements
        {
            return new GangEventExecutor<TDataImplements>(
                    _allHandlers.Add(GangEventHandler<TDataImplements>.From(GangEventHandler))
                );
        }

        /// <summary>
        /// Remove all handlers for a given type
        /// </summary>
        /// <typeparam name="TData">Type of data</typeparam>
        /// <returns>A new Executor</returns>
        public GangEventExecutor<TDataImplements> RemoveHandlers<TData>()
            where TData : TDataImplements
        {
            return new GangEventExecutor<TDataImplements>(
                    _allHandlers.RemoveAll(h => h.DataType == typeof(TData))
                );
        }

        /// <summary>
        /// Execute the GangEventHandler for the data passed
        /// </summary>
        /// <typeparam name="T">Actual type of the data, implements TData</typeparam>
        /// <param name="data">Data</param>
        public Task ExecuteAsync<T>(
            T data)
            where T : TDataImplements
        {
            var dataType = data.GetType();
            var handlers = _handlers.ContainsKey(dataType)
                ? _handlers[dataType]
                : null;

            if (handlers == null) return Task.CompletedTask;

            return Task.WhenAll(
                handlers
                    .Select(GangEventHandler => GangEventHandler(data))
                    .ToArray()
                );
        }

        /// <summary>
        /// Execute the GangEventHandler sync for the data passed
        /// </summary>
        /// <typeparam name="T">Actual type of the data, implements TData</typeparam>
        /// <param name="data">Data</param>
        public void Execute<T>(
            T data)
            where T : TDataImplements
        {
            ExecuteAsync(data).GetAwaiter().GetResult();
        }
    }
}
