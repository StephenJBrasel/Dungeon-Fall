using System;
using UnityEngine;
using UnityEditor;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
    AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class MapGenerationTypeHideAttribute : HideAttribute {
    //The name of the bool field that will be in control
    public string ConditionalSourceField = "";
    //TRUE = Hide in inspector / FALSE = Disable in inspector 
    public bool HideInInspector = false;
    //The condition on which this attribute will be enabled
    public GENERATOR_TYPE[] DesiredTypes = { GENERATOR_TYPE.CELLULAR_AUTOMATA};

    public MapGenerationTypeHideAttribute(string conditionalSourceField) {
        this.ConditionalSourceField = conditionalSourceField;
    }

    public MapGenerationTypeHideAttribute(string conditionalSourceField, bool hideInInspector)
        : this(conditionalSourceField){
        this.HideInInspector = hideInInspector;
    }

    public MapGenerationTypeHideAttribute(string conditionalSourceField, bool hideInInspector, GENERATOR_TYPE[] desiredCondition) 
        : this(conditionalSourceField, hideInInspector){
        this.DesiredTypes = desiredCondition;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.PropertyField(position, property, label, true);
    }

    public override bool isEnabled(SerializedProperty property) {
        //Look for the sourcefield within the object that the property belongs to
        //string propertyPath = property.propertyPath; //returns the property path of the property we want to apply the attribute to
        //string enabledconditionPath = property.propertyPath.Replace(property.name, ConditionalSourceField); //changes the path to the conditionalsource property path
        bool enabled = !HideInInspector;

        SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(
            property.propertyPath.Replace(
                property.name, ConditionalSourceField));

        if (sourcePropertyValue != null && DesiredTypes.Length > 0) {
			for (int j = 0; j < DesiredTypes.Length; j++) 
                if (sourcePropertyValue.enumValueIndex == (int)DesiredTypes[j]) 
                    return enabled || true;
        } else {
            Debug.LogWarning("Attempting to use a MapGenerationTypeHideAttribute but no matching SourcePropertyValue: "
                + ConditionalSourceField + " found in object: " + ConditionalSourceField);
        }

        return enabled || false;
    }

}