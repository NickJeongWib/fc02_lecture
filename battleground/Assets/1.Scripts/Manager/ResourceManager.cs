using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

/// <summary>
/// Resource.Load�� �����ϴ� Ŭ����
/// ���߿� ��¹���� �����
/// </summary>
public class ResourceManager : MonoBehaviour
{
    public static UnityObject Load(string path)
    {
        // ������ ���ҽ� �ε����� ���Ŀ� ��� �ε�
        return Resources.Load(path);
    }

    public static GameObject LoadAndInstntiate(string path)
    {
        UnityObject source = Load(path);

        if (source == null)
        {
            return null;
        }

        return GameObject.Instantiate(source) as GameObject;
    }


}
