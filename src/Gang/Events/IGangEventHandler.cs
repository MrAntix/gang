using System.Threading.Tasks;

namespace Gang.Events
{
    /// <summary>
    /// A GangEventHandler for the given data
    /// </summary>
    /// <typeparam name="TData">Data Type</typeparam>
    public interface IGangEventHandler<TData>
        where TData : class
    {
        /// <summary>
        /// Handle the data, called by an executor
        /// </summary>
        /// <param name="data">Data</param>
        /// <returns>Task</returns>
        Task HandleAsync(TData data);
    }
}
