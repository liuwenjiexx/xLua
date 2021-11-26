using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Yoozoo.Managers.NetworkV2.Core;
using Yoozoo.Managers.NetworkV2.Helpers;

namespace Yoozoo.Managers.NetworkV2.Editor
{
    [CustomEditor(typeof(NetChannel))]
    internal sealed class NetChannelInspector : UnityEditor.Editor
    {
        private const string NoneOptionName = "<None>";

        private static readonly float[] GameSpeed = new float[]
            {0f, 0.01f, 0.1f, 0.25f, 0.5f, 1f, 1.5f, 2f, 4f, 8f};

        private static readonly string[] GameSpeedForDisplay = new string[]
            {"0x", "0.01x", "0.1x", "0.25x", "0.5x", "1x", "1.5x", "2x", "4x", "8x"};
        
        
        private SerializedProperty m_PacketHeaderHelperTypeKey = null;
        private SerializedProperty m_PacketBodyHelperTypeKey = null;
        private SerializedProperty m_PacketSerializerHelperTypeKey = null;
        private SerializedProperty m_HeartBeatHelperTypeKey= null;
        private SerializedProperty m_ReconnectHelperTypeKey= null;

        private string[] m_PacketHeaderHelperTypeKeys = null;
        private int m_PacketHeaderHelperTypeKeyIndex = 0;
        private string[] m_PacketBodyHelperTypeKeys = null;
        private int m_PacketBodyHelperTypeKeyIndex = 0;
        private string[] m_PacketSerializerHelperTypeKeys = null;
        private int m_PacketSerializerHelperTypeKeyIndex = 0;
        private string[] m_HeartBeatHelperTypeKeys = null;
        private int m_HeartBeatHelperTypeKeyIndex = 0;  
        private string[] m_ReconnectHelperTypeKeys = null;
        private int m_ReconnectHelperTypeKeyIndex = 0;
        
        private NetChannel _entity;
        public NetChannel entity 
        {
            get
            {
                if (_entity == null)
                    _entity = (NetChannel) target;
                return _entity;
            }
        }
        
        public override void OnInspectorGUI()
        {
            if (m_IsCompiling && !EditorApplication.isCompiling)
            {
                m_IsCompiling = false;
                OnCompileComplete();
            }
            else if (!m_IsCompiling && EditorApplication.isCompiling)
            {
                m_IsCompiling = true;
                OnCompileStart();
            }
            
            base.OnInspectorGUI();
            
            serializedObject.Update();

            NetChannel t = (NetChannel) target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Channel Helpers", EditorStyles.boldLabel);

                    int packetHeaderHelperTypeKeyIndex = EditorGUILayout.Popup("Header Deserializer Helper",
                        m_PacketHeaderHelperTypeKeyIndex,  m_PacketHeaderHelperTypeKeys);
                    if (packetHeaderHelperTypeKeyIndex != m_PacketHeaderHelperTypeKeyIndex)
                    {
                        m_PacketHeaderHelperTypeKeyIndex = packetHeaderHelperTypeKeyIndex;
                        m_PacketHeaderHelperTypeKey.stringValue = packetHeaderHelperTypeKeyIndex <= 0
                            ? null
                            : m_PacketHeaderHelperTypeKeys[packetHeaderHelperTypeKeyIndex];
                    }

                    int packetBodyHelperTypeKey = EditorGUILayout.Popup("Body Deserializer Helper", m_PacketBodyHelperTypeKeyIndex,
                        m_PacketBodyHelperTypeKeys);
                    if (packetBodyHelperTypeKey != m_PacketBodyHelperTypeKeyIndex)
                    {
                        m_PacketBodyHelperTypeKeyIndex = packetBodyHelperTypeKey;
                        m_PacketBodyHelperTypeKey.stringValue = packetBodyHelperTypeKey <= 0
                            ? null
                            : m_PacketBodyHelperTypeKeys[packetBodyHelperTypeKey];
                        ClearDiverter();

                    }

                    int zipHelperSelectedIndex = EditorGUILayout.Popup("Serializer Helper", m_PacketSerializerHelperTypeKeyIndex,
                        m_PacketSerializerHelperTypeKeys);
                    if (zipHelperSelectedIndex != m_PacketSerializerHelperTypeKeyIndex)
                    {
                        m_PacketSerializerHelperTypeKeyIndex = zipHelperSelectedIndex;
                        m_PacketSerializerHelperTypeKey.stringValue = zipHelperSelectedIndex <= 0
                            ? null
                            : m_PacketSerializerHelperTypeKeys[zipHelperSelectedIndex];
                    }
                    
                    
                    
                    if(m_PacketBodyHelperTypeKeyIndex>0 && GUILayout.Button("选择一个消息分流器")){
                        DeserializeDiverterSelectWindow wnd = EditorWindow.GetWindow<DeserializeDiverterSelectWindow>(true, "选择扩展组件");
                        wnd.Inspector = this;
                    }

                    int heartBeatSelectIndex = EditorGUILayout.Popup("Heart Beat Helper", m_HeartBeatHelperTypeKeyIndex,
                        m_HeartBeatHelperTypeKeys);
                    if (heartBeatSelectIndex != m_HeartBeatHelperTypeKeyIndex)
                    {
                        m_HeartBeatHelperTypeKeyIndex = heartBeatSelectIndex;
                        m_HeartBeatHelperTypeKey.stringValue = heartBeatSelectIndex <= 0
                            ? null
                            : m_HeartBeatHelperTypeKeys[heartBeatSelectIndex];
                    }
                    
                    
                    int reconnectSelectIndex = EditorGUILayout.Popup("Reconnection Helper", m_ReconnectHelperTypeKeyIndex,
                        m_ReconnectHelperTypeKeys);
                    if (reconnectSelectIndex != m_ReconnectHelperTypeKeyIndex)
                    {
                        m_ReconnectHelperTypeKeyIndex = reconnectSelectIndex;
                        m_ReconnectHelperTypeKey.stringValue = reconnectSelectIndex <= 0
                            ? null
                            : m_ReconnectHelperTypeKeys[reconnectSelectIndex];
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }

        private void ClearDiverter()
        {
            if (m_PacketBodyHelperTypeKeyIndex <= 0)
            {
                var addedComponents = entity.gameObject.GetComponents<DeserializeDiverterBase>();
                for (int j = 0; j < addedComponents.Length; j++)
                {
                    GameObject.DestroyImmediate(addedComponents[j]);
                }
            }
        }

        private void OnEnable()
        {
            
            m_PacketHeaderHelperTypeKey = serializedObject.FindProperty("m_PacketHeaderHelperTypeKey");
            m_PacketBodyHelperTypeKey = serializedObject.FindProperty("m_PacketBodyHelperTypeKey");
            m_PacketSerializerHelperTypeKey = serializedObject.FindProperty("m_PacketSerializerHelperTypeKey");
            m_HeartBeatHelperTypeKey  = serializedObject.FindProperty("m_HeartBeatHelperTypeKey");
            m_ReconnectHelperTypeKey  = serializedObject.FindProperty("m_ReconnectHelperTypeKey");
            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            List<string> versionHelperTypeNames = new List<string>
            {
                NoneOptionName
            };

            versionHelperTypeNames.AddRange(Type.GetTypeNames(typeof(IPacketHeaderHelper)));
            m_PacketHeaderHelperTypeKeys = versionHelperTypeNames.ToArray();
            m_PacketHeaderHelperTypeKeyIndex = 0;
            if (!string.IsNullOrEmpty(m_PacketHeaderHelperTypeKey.stringValue))
            {
                m_PacketHeaderHelperTypeKeyIndex = versionHelperTypeNames.IndexOf(m_PacketHeaderHelperTypeKey.stringValue);
                if (m_PacketHeaderHelperTypeKeyIndex <= 0)
                {
                    m_PacketHeaderHelperTypeKeyIndex = 0;
                    m_PacketHeaderHelperTypeKey.stringValue = null;
                }
            }

            List<string> logHelperTypeNames = new List<string>
            {
                NoneOptionName
            };

            logHelperTypeNames.AddRange(Type.GetTypeNames(typeof(IPacketBodyHelper)));
            m_PacketBodyHelperTypeKeys = logHelperTypeNames.ToArray();
            m_PacketBodyHelperTypeKeyIndex = 0;
            if (!string.IsNullOrEmpty(m_PacketBodyHelperTypeKey.stringValue))
            {
                m_PacketBodyHelperTypeKeyIndex = logHelperTypeNames.IndexOf(m_PacketBodyHelperTypeKey.stringValue);
                if (m_PacketBodyHelperTypeKeyIndex <= 0)
                {
                    m_PacketBodyHelperTypeKeyIndex = 0;
                    m_PacketBodyHelperTypeKey.stringValue = null;
                    ClearDiverter();
                }
            }

            List<string> serializerHelperTypeKeys = new List<string>
            {
                NoneOptionName
            };

            serializerHelperTypeKeys.AddRange(Type.GetTypeNames(typeof(IPacketSerializedHelper)));
            m_PacketSerializerHelperTypeKeys = serializerHelperTypeKeys.ToArray();
            m_PacketSerializerHelperTypeKeyIndex = 0;
            if (!string.IsNullOrEmpty(m_PacketSerializerHelperTypeKey.stringValue))
            {
                m_PacketSerializerHelperTypeKeyIndex = serializerHelperTypeKeys.IndexOf(m_PacketSerializerHelperTypeKey.stringValue);
                if (m_PacketSerializerHelperTypeKeyIndex <= 0)
                {
                    m_PacketSerializerHelperTypeKeyIndex = 0;
                    m_PacketSerializerHelperTypeKey.stringValue = null;
                }
            }
            
            
            
            List<string> heartBeatHelperTypeKeys = new List<string>
            {
                NoneOptionName
            };

            heartBeatHelperTypeKeys.AddRange(Type.GetTypeNames(typeof(IHeartBeatHelper)));
            m_HeartBeatHelperTypeKeys = heartBeatHelperTypeKeys.ToArray();
            m_HeartBeatHelperTypeKeyIndex = 0;
            if (!string.IsNullOrEmpty(m_HeartBeatHelperTypeKey.stringValue))
            {
                m_HeartBeatHelperTypeKeyIndex = heartBeatHelperTypeKeys.IndexOf(m_HeartBeatHelperTypeKey.stringValue);
                if (m_HeartBeatHelperTypeKeyIndex <= 0)
                {
                    m_HeartBeatHelperTypeKeyIndex = 0;
                    m_HeartBeatHelperTypeKey.stringValue = null;
                }
            }
            
            
            List<string> reconnectHelperTypeKeys = new List<string>
            {
                NoneOptionName
            };
            reconnectHelperTypeKeys.AddRange(Type.GetTypeNames(typeof(IReconnectHelper)));
            m_ReconnectHelperTypeKeys = reconnectHelperTypeKeys.ToArray();
            m_ReconnectHelperTypeKeyIndex = 0;
            if (!string.IsNullOrEmpty(m_ReconnectHelperTypeKey.stringValue))
            {
                m_ReconnectHelperTypeKeyIndex = reconnectHelperTypeKeys.IndexOf(m_ReconnectHelperTypeKey.stringValue);
                if (m_ReconnectHelperTypeKeyIndex <= 0)
                {
                    m_ReconnectHelperTypeKeyIndex = 0;
                    m_ReconnectHelperTypeKey.stringValue = null;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
        
        
        private bool m_IsCompiling = false;


        /// <summary>
        /// 编译开始事件。
        /// </summary>
        protected void OnCompileStart()
        {
        }
        
        protected void OnCompileComplete()
        {
            RefreshTypeNames();
        }


        protected bool IsPrefabInHierarchy(UnityEngine.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

#if UNITY_2018_3_OR_NEWER
            return PrefabUtility.GetPrefabAssetType(obj) != PrefabAssetType.Regular;
#else
            return PrefabUtility.GetPrefabType(obj) != PrefabType.Prefab;
#endif
        }
        
    }
}