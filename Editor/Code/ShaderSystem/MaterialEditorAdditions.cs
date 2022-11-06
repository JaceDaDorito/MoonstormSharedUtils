﻿using Moonstorm.EditorUtils.EditorWindows;
using Moonstorm.Experimental;
using RoR2EditorKit.Utilities;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Moonstorm.EditorUtils.ShaderSystem
{
    [InitializeOnLoad]
    public class MaterialEditorAdditions
    {
        static MaterialEditorAdditions()
        {
            Editor.finishedDefaultHeaderGUI += Draw;
        }

        private static void Draw(Editor obj)
        {
            if (!(obj is MaterialEditor materialEditor))
            {
                return;
            }

            var id = GUIUtility.GetControlID(new GUIContent("Pick shader asset"), FocusType.Passive);

            Material targetMaterial = materialEditor.target as Material;
            Shader shader = targetMaterial.shader;
            if(shader.name.StartsWith("Stubbed"))
            {
                if(GUILayout.Button("Upgrade to Real Shader"))
                {
                    MaterialShaderManager.Upgrade((Material)materialEditor.target);
                }
            }
            if(shader.name == "AddressableMaterialShader")
            {
                AddressableMaterialShaderHeader(obj);
            }

            if (GUILayout.Button("Pick shader asset"))
            {
                EditorGUIUtility.ShowObjectPicker<Shader>(null, false, null, id);
            }

            if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == id)
            {
                materialEditor.SetShader(EditorGUIUtility.GetObjectPickerObject() as Shader, true);
            }
        }

        private static void AddressableMaterialShaderHeader(Editor obj)
        {
            SerializedObject so = obj.serializedObject;
            SerializedProperty shaderKeywords = so.FindProperty("m_ShaderKeywords");
            shaderKeywords.stringValue = EditorGUILayout.TextField(new GUIContent("Address"), shaderKeywords.stringValue);
            so.ApplyModifiedProperties();

            if(GUILayout.Button("Show Preview"))
            {
                var window = EditorWindow.CreateWindow<AddressableMaterialPreview>();
                window.materialAddress = shaderKeywords.stringValue;
                window.ShowPopup();
            }
        }
    }
}