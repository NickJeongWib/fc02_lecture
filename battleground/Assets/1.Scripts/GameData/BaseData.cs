using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Data�� �⺻ Ŭ����
/// �������� �����͸� ������ �ְ� �Ǵµ�, �̸��� ���� ���� �ִ�.
/// �������� ������ �̸��� ��� ����Ʈ�� ���� �� �ִ�.
/// </summary>
public class BaseData : ScriptableObject
{
    // C:\Users\HOME\Desktop\GIt\fc02_lecture\battleground\Assets\9.ResourcesData\Resources
    public const string dataDirectory = "/9.ResourcesData/Resources/Data/";

    public string[] names = null;

    /** ������ */
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
    /// ���� ����ϱ� ���� �̸� ����� ������ִ� �Լ� 
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
            // filterWord�� ������ �ƴ϶��
            if (filterWord != "")
            {
                // �̸��� �ҹ��ڷ� �ٲٰ� �� �ܾ �����ϰ� ���������� ���
                if (this.names[i].ToLower().Contains(filterWord.ToLower()) == false)
                {
                    continue;
                }
            }

            // ���̵� �����ش�
            if (showID == true)
            {
                // 1 : �̸�
                retList[i] = i.ToString() + " : " + this.names[i];
            }
            else
            {
                // �̸�
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
