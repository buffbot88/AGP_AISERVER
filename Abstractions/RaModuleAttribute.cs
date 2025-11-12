namespace Abstractions
{
    /// <summary>
    /// Attribute to mark a module with metadata for the module manager.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RaModuleAttribute : Attribute
    {
        /// <summary>
        /// The category this module belongs to (e.g., "core", "extensions", "plugins").
        /// </summary>
        public string Category { get; set; } = "core";
    }
}
