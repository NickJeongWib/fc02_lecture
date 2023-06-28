using UnityEditor;
using UnityEngine;
using System.Text;
using UnityObject = UnityEngine.Object;

public class EffectTool : EditorWindow
{
    // UI �׸��µ� �ʿ��� ������
    public int uiWidthLarge = 300;
    public int uiWidthMiddle = 200;
    private int selection = 0;
    private Vector2 SP1 = Vector2.zero;
    private Vector2 SP2 = Vector2.zero;

    // ����Ʈ Ŭ��
    private GameObject effectSource = null;
    // ����Ʈ������
    private static EffectData effectData;

    [MenuItem("Tools/Effect Tool")]

    static void Init()
    {
        effectData = ScriptableObject.CreateInstance<EffectData>();

        effectData.LoadData();

        EffectTool window = GetWindow<EffectTool>(false, "Effect Tool");
        window.Show();
    }

    private void OnGUI()
    {
        if (effectData == null)
        {
            return;
        }

        EditorGUILayout.BeginVertical();
        {
            // ��� add, remove, copy ����
            UnityObject source = effectSource;
            EditorHelper.EditorToolTopLayer(effectData, ref selection, ref source,
                this.uiWidthMiddle);
            effectSource = (GameObject)source;

            EditorGUILayout.BeginHorizontal();
            {
                // �߰�, ������ ���
                EditorHelper.EditorToolListLayer(ref SP1, effectData, ref selection, ref source, uiWidthLarge);
                effectSource = (GameObject)source;

                // ���� �κ�
                EditorGUILayout.BeginVertical();
                {
                    SP2 = EditorGUILayout.BeginScrollView(SP2);
                    {
                        if (effectData.GetDataCount() > 0)
                        {
                            EditorGUILayout.BeginVertical();
                            {
                                EditorGUILayout.Separator();

                                EditorGUILayout.LabelField("ID.", selection.ToString(),
                                    GUILayout.Width(uiWidthLarge));

                                effectData.names[selection] = EditorGUILayout.TextField("�̸�.", effectData.names[selection],
                                    GUILayout.Width(uiWidthLarge * 1.5f));

                                effectData.effectClips[selection].effectType =
                                    (EffectType)EditorGUILayout.EnumPopup("����Ʈ Ÿ��.",
                                    effectData.effectClips[selection].effectType,
                                    GUILayout.Width(uiWidthLarge));

                                EditorGUILayout.Separator();

                                if (effectSource == null && effectData.effectClips[selection].effectName != string.Empty)
                                {
                                    effectData.effectClips[selection].PreLoad();
                                    effectSource = Resources.Load(effectData.effectClips[selection].effectPath +
                                        effectData.effectClips[selection].effectName) as GameObject;
                                }

                                effectSource = (GameObject)EditorGUILayout.ObjectField("����Ʈ.",
                                    effectSource, typeof(GameObject), false, GUILayout.Width(uiWidthLarge * 1.5f));

                                if (effectSource != null)
                                {
                                    effectData.effectClips[selection].effectPath =
                                        EditorHelper.GetPath(effectSource);
                                    effectData.effectClips[selection].effectName = effectSource.name;
                                }
                                else
                                {
                                    effectData.effectClips[selection].effectPath = string.Empty;
                                    effectData.effectClips[selection].effectName = string.Empty;
                                    effectSource = null;
                                }
                                EditorGUILayout.Separator();
                            }
                            EditorGUILayout.EndVertical();
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        // �ϴ�
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Reload Setting"))
            {
                effectData = CreateInstance<EffectData>();
                effectData.LoadData();
                selection = 0;
                effectSource = null;
            }

            if (GUILayout.Button("Save"))
            {
                EffectTool.effectData.SaveData();
                CreateEnumStructure();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    public void CreateEnumStructure()
    {
        string enumName = "EffecList";
        StringBuilder builder = new StringBuilder();
        builder.AppendLine();

        for (int i = 0; i < effectData.names.Length; i++)
        {
            if (effectData.names[i] != string.Empty)
            {
                builder.AppendLine("    " + effectData.names[i] + " = " + i + ",");
            }
        }

        EditorHelper.CreateEnumStructure(enumName, builder);
    }
}
