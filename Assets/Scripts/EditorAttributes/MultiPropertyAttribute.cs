using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[AttributeUsage(AttributeTargets.Field)]
public abstract class MultiPropertyAttribute : PropertyAttribute {
    public List<object> stored = new List<object>();
    public virtual GUIContent BuildLabel(GUIContent label) {
        return label;
    }
    public abstract void OnGUI(Rect position, SerializedProperty property, GUIContent label);

    public virtual float? GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return null;
    }
}
