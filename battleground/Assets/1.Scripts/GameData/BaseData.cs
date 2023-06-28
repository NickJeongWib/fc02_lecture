using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Data의 기본 클래스
/// 공통적인 데이터를 가지고 있게 되는데, 이름만 현재 갖고 있다.
/// 데이터의 갯수와 이름의 목록 리스트를 얻을 수 있다.
/// </summary>
public class BaseData : ScriptableObject
{
    // C:\Users\HOME\Desktop\GIt\fc02_lecture\battleground\Assets\9.ResourcesData\Resources
    public const string dataDirectory = "/9.ResourcesData/Resources/Data/";

    public string[] names = null;

    /** 생성자 */
    public BaseData() { }

    public int GetDataCount()
    {
        int retValue = 0;

        if (this.names != null)
        {
            retValue = this.names.Length;
        }

        return retValue;
    }

    /// <summary>
    /// 툴에 출력하기 위한 이름 목록을 만들어주는 함수 
    /// </summary>

    public string[] GetNameList(bool showID, string filterWord = "")
    {
        string[] retList = new string[0];

        if (this.names == null)
        {
            return retList;
        }

        retList = new string[this.names.Length];

        for (int i = 0; i < retList.Length; i++)
        {
            // filterWord가 공백이 아니라면
            if (filterWord != "")
            {
                // 이름을 소문자로 바꾸고 이 단어를 포함하고 있지않으면 계속
                if (this.names[i].ToLower().Contains(filterWord.ToLower()) == false)
                {
                    continue;
                }
            }

            // 아이디를 보여준다
            if (showID == true)
            {
                // 1 : 이름
                retList[i] = i.ToString() + " : " + this.names[i];
            }
            else
            {
                // 이름
                retList[i] = this.names[i];
            }
        }

        return retList;
    } // public string[] GetNameList(bool showID, string filterWord = "")


    public virtual int AddData(string newName)
    {
        return GetDataCount();
    }

    public virtual void RemoveData(int index)
    {

    }

    public virtual void Copy(int index)
    {

    }
}
