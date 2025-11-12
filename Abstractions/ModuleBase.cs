namespace Abstractions
{
    /// <summary>
    /// Base class for all modules in the system.
    /// Provides common functionality like logging and lifecycle management.
    /// </summary>
    public abstract class ModuleBase : IDisposable
    {
        /// <summary>
        /// The name of the module.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Initialize the module with a reference to the module manager.
        /// </summary>
        public virtual void Initialize(object? manager)
        {
            // Base implementation - can be overridden by derived classes
        }

        /// <summary>
        /// Process a text command for this module.
        /// </summary>
        public abstract string Process(string input);

        /// <summary>
        /// Log an informational message.
        /// </summary>
        protected void LogInfo(string message)
        {
            Console.WriteLine($"[{Name}] INFO: {message}");
        }

        /// <summary>
        /// Log a warning message.
        /// </summary>
        protected void LogWarning(string message)
        {
            Console.WriteLine($"[{Name}] WARN: {message}");
        }

        /// <summary>
        /// Log an error message.
        /// </summary>
        protected void LogError(string message)
        {
            Console.WriteLine($"[{Name}] ERROR: {message}");
        }

        /// <summary>
        /// Dispose of module resources.
        /// </summary>
        public virtual void Dispose()
        {
            // Base implementation - can be overridden by derived classes
        }
    }
}
