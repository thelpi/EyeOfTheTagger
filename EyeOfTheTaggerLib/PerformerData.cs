using System;
using EyeOfTheTaggerLib.Abstractions;

namespace EyeOfTheTaggerLib
{
    /// <summary>
    /// Represents a performer.
    /// </summary>
    /// <seealso cref="BaseData"/>
    public class PerformerData : BaseData
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"><see cref="Name"/></param>
        /// <param name="isDefault"><see cref="IsDefault"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>Null</c>.</exception>
        internal PerformerData(string name, bool isDefault)
            : base(name, isDefault) { }
    }
}
