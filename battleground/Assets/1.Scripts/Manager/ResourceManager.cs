using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

/// <summary>
/// Resource.Load를 래핑하는 클래스
/// 나중에 어셋번들로 변경됨
/// </summary>
public class ResourceManager : MonoBehaviour
{
    public static UnityObject Load(string path)
    {
        // 지금은 리소스 로드지만 추후엔 어셋 로드
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
