using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Хранит информацию об отдельной ячейке сетки.
/// </summary>
[System.Serializable]
public class CellData : MonoBehaviour // CellData сделать МОнобехом, чтобы можно было получить всю инфу наведя на элемент
{
   
    public GameObject instance;         // Экземпляр GameObject ячейки
    public Vector2Int gridCoordinates;  // Координаты в сетке (x, y)
    public bool isWhiteSquare;          // Для шахматного порядка (true - "белая", false - "черная")
    
    public enum CellType // Тип ячейки, если нужно расширить функционал
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
    public Vector3 worldCoordinates;    // Мировые координаты центра ячейки
    public GameObject cellPrefabUsed;   // Какой префаб был использован для этой ячейки
    public CellType cellType; // Тип ячейки, если нужно расширить функционал

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
    public List<CellAreaType> cellAreaTypes = new List<CellAreaType>();// Тип области ячейки

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

// Написать класс Селл, внутри есть ссылка на плоскость триггерную_ триггерный коллайдер(которая может менять в зависимости от деятельности)
// , так же должен быть визуал(GameObject MEsh). 2 раздельные сущности. Создавать изначально пустой Gameobject.

/// <summary>
/// Хранит информацию о точке на поверхности (координаты и тип/префаб).
/// </summary>