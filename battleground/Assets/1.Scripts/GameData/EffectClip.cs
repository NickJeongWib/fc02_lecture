using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Effect Prefab�� ���, Ÿ�� ���� �Ӽ� �����͸� ������ �ְ� �Ǹ�
/// Prefab �����ε� ����� ���� �ְ� - Ǯ���� ���� ����̱⵵ �ϴ�.
/// ����Ʈ �ν��Ͻ� ��ɵ� ���� ������ - Ǯ���� �����ؼ� ����ϱ⵵ �Ѵ�.
/// </summary>
public class EffectClip
{
    // ���� �Ӽ��� ������ �ٸ� ����Ʈ Ŭ���� ���� �� �־� �к���.
    public int realId = 0;
    public EffectType effectType = EffectType.NORMAL;
    public GameObject effectPrefab = null;

    public string effectName = string.Empty;
    public string effectPath = string.Empty;
    public string effectFullPath = string.Empty;

    public EffectClip() { }

    public void PreLoad()
    {
        this.effectFullPath = effectPath + effectName;

        // ��� �����Ͱ� �־���ϰ� ����Ʈ �����ε��� ���� �ʾ��� ��   
        if (this.effectFullPath != string.Empty && this.effectPrefab == null)
        {
            effectPrefab = ResourceManager.Load(effectFullPath) as GameObject;
        }
    }

    public void ReleaseEffect()
    {
        if (this.effectPrefab != null)
        {
            this.effectPrefab = null;
        }
    }
    /// <summary>
    /// ���ϴ� ��ġ�� ���� ���ϴ� ����Ʈ�� �ν��Ͻ��Ѵ�.
    /// </summary>
    public GameObject Instantiate(Vector3 Pos)
    {
        if (this.effectPrefab == null)
        {
            PreLoad();
        }

        if (effectPrefab != null)
        {
            GameObject effect = GameObject.Instantiate(effectPrefab, Pos, Quaternion.identity);
            return effect;
        }

        return null;
    }
}