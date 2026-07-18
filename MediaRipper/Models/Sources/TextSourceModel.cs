using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MediaRipper.Models.Sources;

public class TextSourceModel : BaseSourceModel
{
    public TextSourceModel(string titleResourceKey)
    {
        TitleResourceKey = titleResourceKey;
    }

    /// <summary>
    /// Gets the resource key of the title.
    /// </summary>
    public string TitleResourceKey { get; }
    
    /// <summary>
    /// Gets the sub-nodes.
    /// </summary>
    public ObservableCollection<BaseSourceModel> SubNodes { get; init; } = [];
}

public class TextSourceModel<TChild> : TextSourceModel where TChild : BaseSourceModel
{
    public TextSourceModel(string titleResourceKey) : base(titleResourceKey)
    {
    }

    /// <summary>
    /// Gets the items in this node.
    /// </summary>
    public IEnumerable<TChild> Items => SubNodes.Cast<TChild>();
}