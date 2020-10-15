namespace Fulu.WebAPI.Abstractions
{
    /// <summary>
    ///     
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// none
        /// </summary>
        //[JsonConverter(typeof(StringEnumConverter), true)]
        None = 0,
        /// <summary>
        /// info
        /// </summary>
        //[JsonConverter(typeof(StringEnumConverter), true)]
        Info = 1,
        /// <summary>
        /// warn
        /// </summary>
        //[JsonConverter(typeof(StringEnumConverter), true)]
        Warn = 2,
        /// <summary>
        /// error
        /// </summary>
        //[JsonConverter(typeof(StringEnumConverter), true)]
        Error = 3,
        /// <summary>
        /// success
        /// </summary>
        //[JsonConverter(typeof(StringEnumConverter), true)]
        Success = 4,
    }
}
