using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ������ ���������� �� ��������� ������ �����.
/// </summary>
[System.Serializable]
public class CellData : MonoBehaviour // CellData ������� ���������, ����� ����� ���� �������� ��� ���� ������ �� �������
{
   
    public GameObject instance;         // ��������� GameObject ������
    public Vector2Int gridCoordinates;  // ���������� � ����� (x, y)
    public bool isWhiteSquare;          // ��� ���������� ������� (true - "�����", false - "������")
    
    public enum CellType // ��� ������, ���� ����� ��������� ����������
    {
        None,
        Grass,
        Water,
        Sand,
        Rock,
        Forest,
        Land,
        Desert,
        Lava,
    }
    public Vector3 worldCoordinates;    // ������� ���������� ������ ������
    public GameObject cellPrefabUsed;   // ����� ������ ��� ����������� ��� ���� ������
    public CellType cellType; // ��� ������, ���� ����� ��������� ����������

    public enum CellAreaType
    {
        None = 0,
        Water = 1,
        Land = 2,
        Forest = 3,
        Mountain = 4,
        Desert = 5,
        Swamp = 6,
        Urban = 7,
        Snow = 8,
        Beach = 9,
        Plains = 10,
    }
    public List<CellAreaType> cellAreaTypes = new List<CellAreaType>();// ��� ������� ������

    public void Init(GameObject instance, Vector3 worldCoordinates, GameObject cellPrefabUsed)
    {
        this.instance = instance;
        this.worldCoordinates = worldCoordinates;
        this.cellPrefabUsed = cellPrefabUsed;
    }

    public void SetCellData(Vector2Int gridCoordinates, bool isWhiteSquare, CellType cellType)
    {
        this.gridCoordinates = gridCoordinates;
        this.isWhiteSquare = isWhiteSquare;
        this.cellType = cellType;
    }

    public void SetCellType(CellData.CellType newCellType)
    {
        cellType = newCellType;
    }

    public void SetCellAreaType(CellAreaType newCellAreaType)
    {
        cellAreaTypes.Add(newCellAreaType);
    }
}

// �������� ����� ����, ������ ���� ������ �� ��������� ����������_ ���������� ���������(������� ����� ������ � ����������� �� ������������)
// , ��� �� ������ ���� ������(GameObject MEsh). 2 ���������� ��������. ��������� ���������� ������ Gameobject.

/// <summary>
/// ������ ���������� � ����� �� ����������� (���������� � ���/������).
/// </summary>