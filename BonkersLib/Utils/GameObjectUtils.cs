using UnityEngine;

namespace BonkersLib.Utils;

public class GameObjectUtils
{
    public static GameObject FindMinimapIcon(Transform objTransform)
    {
        if (objTransform == null) return null;

        var rootTransform = objTransform.root;

        var allChildren = rootTransform.GetComponentsInChildren<Transform>(true);

        foreach (var childTransform in allChildren)
        {
            if (childTransform.name.StartsWith("MinimapIcon"))
            {
                return childTransform.gameObject;
            }
        }

        return null;
    }
}