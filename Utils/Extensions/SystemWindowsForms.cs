#if USE_SYSTEMWINDOWSFORMS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Bluscream;

public static partial class Extensions
{
    #region TreeNodeCollection
    public static IEnumerable<TreeNode> GetAllChilds(this TreeNodeCollection nodes)
    {
        foreach (TreeNode node in nodes)
        {
            yield return node;
            foreach (var child in GetAllChilds(node.Nodes))
                yield return child;
        }
    }
    #endregion

    #region DataGridView
    public static void StretchLastColumn(this DataGridView dataGridView)
    {
        dataGridView.AutoResizeColumns();
        var lastColIndex = dataGridView.Columns.Count - 1;
        var lastCol = dataGridView.Columns[lastColIndex];
        lastCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
    }
    #endregion

    #region DialogResult
    public static bool isFalse(this DialogResult result)
    {
        return result == DialogResult.No
            || result == DialogResult.Cancel
            || result == DialogResult.Abort
            || result == DialogResult.Ignore
            || result == DialogResult.None;
    }
    #endregion
}
#endif 