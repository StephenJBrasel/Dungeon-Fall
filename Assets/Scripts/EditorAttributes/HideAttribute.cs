using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class HideAttribute : MultiPropertyAttribute {
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		throw new System.NotImplementedException();
	}
	
    public abstract bool isEnabled(SerializedProperty property);
}