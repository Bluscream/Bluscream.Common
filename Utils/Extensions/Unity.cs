#if USE_UNITY
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ABI_RC.Core.Player;

namespace Bluscream;

public static partial class Extensions
{
    #region Vector3
    public static List<float> ToList(this Vector3 vec)
    {
        return vec != null ? new List<float>() { vec.x, vec.y, vec.z } : new List<float>();
    }

    public static string ToString(this Vector3 v)
    {
        return $"X:{v.x} Y:{v.y} Z:{v.z}";
    }

    public static Vector3 SetX(this Vector3 vector, float x)
    {
        return new Vector3(x, vector.y, vector.z);
    }

    public static Vector3 SetY(this Vector3 vector, float y)
    {
        return new Vector3(vector.x, y, vector.z);
    }

    public static Vector3 SetZ(this Vector3 vector, float z)
    {
        return new Vector3(vector.x, vector.y, z);
    }
    #endregion

    #region Vector2
    public static Vector2 RoundAmount(this Vector2 i, float nearestFactor)
    {
        return new Vector2(i.x.RoundAmount(nearestFactor), i.y.RoundAmount(nearestFactor));
    }
    #endregion

    #region GameObject
    public static T GetOrAddComponent<T>(this GameObject obj)
        where T : Behaviour
    {
        T comp;
        try
        {
            comp = obj.GetComponent<T>();
            if (comp == null)
            {
                comp = obj.AddComponent<T>();
            }
        }
        catch
        {
            comp = obj.AddComponent<T>();
        }
        return comp;
    }

    public static GameObject FindObject(this GameObject parent, string name)
    {
        Transform[] array = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform transform in array)
        {
            if (transform.name == name)
            {
                return transform.gameObject;
            }
        }
        return null;
    }

    public static string GetPath(this GameObject gameObject)
    {
        string text = "/" + gameObject.name;
        while (gameObject.transform.parent != null)
        {
            gameObject = gameObject.transform.parent.gameObject;
            text = "/" + gameObject.name + text;
        }
        return text;
    }
    #endregion

    #region Transform
    public static T GetOrAddComponent<T>(this Transform obj)
        where T : Behaviour
    {
        T comp;
        try
        {
            comp = obj.gameObject.GetComponent<T>();
            if (comp == null)
            {
                comp = obj.gameObject.AddComponent<T>();
            }
        }
        catch
        {
            comp = obj.gameObject.AddComponent<T>();
        }
        return comp;
    }

    public static void DestroyChildren(
        this Transform transform,
        Func<Transform, bool> exclude,
        bool DirectChildrenOnly = false
    )
    {
        List<Transform> children = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            children.Add(transform.GetChild(i));
        }
        foreach (Transform child in children)
        {
            if (exclude(child)) continue;
            if (DirectChildrenOnly)
            {
                if (child.parent == transform)
                {
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }
    }

    public static void DestroyChildren(this Transform transform, bool DirectChildrenOnly = false)
    {
        transform.DestroyChildren((Transform t) => false, DirectChildrenOnly);
    }

    public static Quaternion LookAtThisWithoutRef(this Transform transform, Vector3 FromThisPosition)
    {
        Vector3 direction = transform.position - FromThisPosition;
        return Quaternion.LookRotation(direction);
    }

    public static Transform[] GetChildren(this Transform transform)
    {
        Transform[] children = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            children[i] = transform.GetChild(i);
        }
        return children;
    }

    public static Transform[] GetAllChildren(this Transform transform)
    {
        List<Transform> children = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            children.Add(child);
            children.AddRange(child.GetAllChildren());
        }
        return children.ToArray();
    }

    public static string GetPath(this Transform transform)
    {
        string text = "/" + transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            text = "/" + transform.name + text;
        }
        return text;
    }
    #endregion

    #region Quaternion
    public static Quaternion FlipX(this Quaternion rot)
    {
        return new Quaternion(-rot.x, rot.y, rot.z, -rot.w);
    }

    public static Quaternion FlipY(this Quaternion rot)
    {
        return new Quaternion(rot.x, -rot.y, -rot.z, rot.w);
    }

    public static Quaternion FlipZ(this Quaternion rot)
    {
        return new Quaternion(rot.x, rot.y, -rot.z, -rot.w);
    }

    public static Quaternion Combine(this Quaternion rot1, Quaternion rot2)
    {
        return rot1 * rot2;
    }
    #endregion

    #region Scene
    public static T[] GetAllInstancesOfCurrentScene<T>(bool includeInactive = false, Func<T, bool> Filter = null)
        where T : Behaviour
    {
        T[] instances = UnityEngine.Object.FindObjectsOfType<T>(includeInactive);
        if (Filter != null)
        {
            instances = instances.Where(Filter).ToArray();
        }
        return instances;
    }

    [Obsolete]
    public static T[] GetAllInstancesOfAllScenes<T>(bool includeInactive = false, Func<T, bool> Filter = null)
        where T : Behaviour
    {
        List<T> instances = new List<T>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded)
            {
                instances.AddRange(scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<T>(includeInactive)));
            }
        }
        if (Filter != null)
        {
            instances = instances.Where(Filter).ToList();
        }
        return instances.ToArray();
    }
    #endregion

    #region Parsing
    public static UnityEngine.Vector3? ParseVector3(this string source)
    {
        if (string.IsNullOrEmpty(source)) return null;
        try
        {
            string[] parts = source.Split(',');
            if (parts.Length == 3)
            {
                float x = float.Parse(parts[0].Trim());
                float y = float.Parse(parts[1].Trim());
                float z = float.Parse(parts[2].Trim());
                return new UnityEngine.Vector3(x, y, z);
            }
        }
        catch
        {
            // Return null if parsing fails
        }
        return null;
    }

    public static UnityEngine.Quaternion? ParseQuaternion(this string source)
    {
        if (string.IsNullOrEmpty(source)) return null;
        try
        {
            string[] parts = source.Split(',');
            if (parts.Length == 4)
            {
                float x = float.Parse(parts[0].Trim());
                float y = float.Parse(parts[1].Trim());
                float z = float.Parse(parts[2].Trim());
                float w = float.Parse(parts[3].Trim());
                return new UnityEngine.Quaternion(x, y, z, w);
            }
        }
        catch
        {
            // Return null if parsing fails
        }
        return null;
    }
    #endregion

    #region Conversion
    public static Vector3? ToVector3(this IList<float> floats)
    {
        if (floats == null || floats.Count < 3) return null;
        return new Vector3(floats[0], floats[1], floats[2]);
    }

    public static Quaternion? ToQuaternion(this IList<float> floats)
    {
        if (floats == null || floats.Count < 4) return null;
        return new Quaternion(floats[0], floats[1], floats[2], floats[3]);
    }
    #endregion

    #region Math
    public static Vector3 RoundAmount(this Vector3 i, float nearestFactor)
    {
        return new Vector3(i.x.RoundAmount(nearestFactor), i.y.RoundAmount(nearestFactor), i.z.RoundAmount(nearestFactor));
    }
    #endregion

    #region UI
    public static ColorBlock SetColor(this ColorBlock block, Color color)
    {
        block.normalColor = color;
        block.highlightedColor = color;
        block.pressedColor = color;
        block.selectedColor = color;
        block.disabledColor = color;
        return block;
    }
    #endregion

    #region VRChat
    public static string getName(this Player player) => player.photonView.owner.NickName;
    #endregion
}
#endif