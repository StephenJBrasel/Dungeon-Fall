using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ColorAttribute : MultiPropertyAttribute {
    Color Color;
    public ColorAttribute(float R, float G, float B) {
        Color = new Color(R, G, B);
    }
    // Draw the property inside the given rect
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        GUI.color = Color;
    }
}