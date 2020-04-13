using EyeOfTheTaggerLib.Datas.Abstractions;

namespace EyeOfTheTagger.ItemDatas.Abstractions
{
    /// <summary>
    /// Base class for every item data.
    /// </summary>
    internal abstract class BaseItemData
    {
        /// <summary>
        /// The underlying <see cref="BaseData"/>, if any.
        /// </summary>
        public BaseData SourceData { get; private set; }
        /// <summary>
        /// <see cref="BaseData.Name"/>
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sourceData"><see cref="SourceData"/>; <c>Null</c> allowed.</param>
        protected BaseItemData(BaseData sourceData)
        {
            SourceData = sourceData;
            Name = sourceData?.Name;
        }

        /// <summary>
        /// Checks if the instance has an empty or unknown name.
        /// </summary>
        /// <returns><c>True</c> if empty name; <c>False</c> otherwise.</returns>
        public bool HasEmptyName()
        {
            return string.IsNullOrWhiteSpace(Name) || SourceData.IsDefault;
        }
    }
}
