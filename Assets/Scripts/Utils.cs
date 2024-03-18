using UnityEngine;

public static class Utils {

    public enum GetComponentSearch {
        None,
        Children,
        Parent
    }

    public static void SetIfNull<T>(this Component component, ref T value, GetComponentSearch search = GetComponentSearch.None) where T : Component {
        if (value) {
            return;
        }

        value = search switch {
            GetComponentSearch.Children => component.GetComponentInChildren<T>(),
            GetComponentSearch.Parent => component.GetComponentInParent<T>(),
            _ => component.GetComponent<T>()
        };
    }
}
