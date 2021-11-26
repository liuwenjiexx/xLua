using System.Collections.Generic;
using Packages.network.Runtime.V2.Runtime.Helpers;
using UnityEditor;
using UnityEngine;
using Yoozoo.Managers.NetworkV2.Helpers;

namespace Yoozoo.Managers.NetworkV2.Editor
{
    public class DeserializeDiverterSelectWindow:EditorWindow
    {
        private List<System.Type> showTypes = new List<System.Type>();

        public DeserializeDiverterSelectWindow()
        {
            showTypes.Clear();
            showTypes.AddRange(Type.GetTypes(typeof(DeserializeDiverterBase)));
        }

        internal NetChannelInspector Inspector { get; set; }

        private Vector3 _scrollPos;
        private void OnGUI()
        {
            _scrollPos =EditorGUILayout.BeginScrollView(_scrollPos,  GUILayout.Height(100));
            for (int i = 0; i < showTypes.Count; i++)
            {
                if (GUILayout.Button(showTypes[i].Name))
                {
                    if (Inspector.entity.gameObject.GetComponent(showTypes[i]) == null)
                    {
                        var addedComponents = Inspector.entity.gameObject.GetComponents<DeserializeDiverterBase>();
                        for (int j = 0; j < addedComponents.Length; j++)
                        {
                            Object.DestroyImmediate(addedComponents[j]);
                        }

                        Inspector.entity.gameObject.AddComponent(showTypes[i]);
                    }
                }

            }
            EditorGUILayout.EndScrollView();
        }
    }
}