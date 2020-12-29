using System;
using UnityEngine;
using UnityEditor;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
    AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class ConditionalHideAttribute : HideAttribute {
    //The name of the bool field that will be in control
    public string ConditionalSourceField = "";
    //TRUE = Hide in inspector / FALSE = Disable in inspector 
    public bool HideInInspector = false;
    //The condition on which this attribute will be enabled
    public bool DesiredCondition = true;

    public ConditionalHideAttribute(string conditionalSourceField) {
        this.ConditionalSourceField = conditionalSourceField;
    }

    public ConditionalHideAttribute(string conditionalSourceField, bool hideInInspector)
        : this(conditionalSourceField){
        this.HideInInspector = hideInInspector;
    }

    public ConditionalHideAttribute(string conditionalSourceField, bool hideInInspector, bool desiredCondition) 
        : this(conditionalSourceField, hideInInspector){
        this.DesiredCondition = desiredCondition;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.PropertyField(position, property, label, true);
        ////check if the propery we want to draw should be enabled
        //bool enabled = isEnabled(property);

        ////Enable/disable the property
        //bool wasEnabled = GUI.enabled;
        //GUI.enabled = enabled;

        ////Check if we should draw the property
        //if (enabled) {
        //    EditorGUI.PropertyField(position, property, label, true);
        //}

        ////Ensure that the next property that is being drawn uses the correct settings
        //GUI.enabled = wasEnabled;
    }

    public override bool isEnabled(SerializedProperty property) {
        bool enabled = !HideInInspector;
        //Look for the sourcefield within the object that the property belongs to
        string propertyPath = property.propertyPath; //returns the property path of the property we want to apply the attribute to
        string enabledconditionPath = propertyPath.Replace(property.name, ConditionalSourceField); //changes the path to the conditionalsource property path
        SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(enabledconditionPath);

        if (sourcePropertyValue != null) {
            return enabled || (sourcePropertyValue.boolValue == DesiredCondition);
        } else {
            Debug.LogWarning("Attempting to use a ConditionalHideAttribute but no matching SourcePropertyValue: "
                + ConditionalSourceField + " found in object: " + ConditionalSourceField);
        }

        return enabled;
    }
}