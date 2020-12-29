using UnityEngine;
using UnityEditor;

public class NewRangeAttribute : MultiPropertyAttribute {
    private float min = 0.0f;
    private float max = 0.0f;

    public NewRangeAttribute(float max) {
        this.max = max;
    }

    public NewRangeAttribute(int max) {
        this.max = (float)max;
    }

    public NewRangeAttribute(float min, float max) : this(max) {
        this.min = min;
    }

    public NewRangeAttribute(int min, int max) : this(max) {
        this.min = (float)min;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        if (property.propertyType == SerializedPropertyType.Float) { 
            EditorGUI.Slider(position, property, min, max, label);
        } else if (property.propertyType == SerializedPropertyType.Integer) {
            EditorGUI.IntSlider(position, property, (int)min, (int)max, label);
        } else {
            Debug.LogError(property.propertyType + " " + property.floatValue);
            EditorGUI.LabelField(position, label.text, "Error: Use Range[] attribute with float or int.");
        }
    }
}
