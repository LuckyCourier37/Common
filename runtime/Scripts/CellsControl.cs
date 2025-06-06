using System.Collections.Generic;
using UnityEngine;

public class CellsControl : Controller
{
    [Header("Scriptable Input")]
    public InputData inputData;

    [Header("Grid Dimensions")]
    public int gridWidth = 10;
    public int gridHeight = 10;
    public Vector3 GridOffsetDebug;

    [Header("Generator Controls")]
    [Tooltip("Генерировать сетку при старте сцены.")]
    public bool generateOnStart = true;

    [Header("Cell Prefabs & Container")]
    [Tooltip("Список префабов ячеек. Первый используется по умолчанию или для 'белых' клеток в шахматном порядке. Второй - для 'черных' клеток, если шахматный порядок включен.")]
    public List<GameObject> cellPrefabs;
    [Tooltip("Контейнер (пустой GameObject), куда будут помещаться созданные ячейки.")]
    public Transform cellContainer;

    public enum SpacingTypeOptions { Automatic, Manual }
    [Tooltip("Automatic: ячейки располагаются вплотную на основе их размера. Manual: задается вручную.")]
    public SpacingTypeOptions spacingMode = SpacingTypeOptions.Automatic;
    [Tooltip("Отступ (промежуток) между ячейками при ручной настройке.")]
    public Vector2 manualSpacing = new Vector2(0.1f, 0.1f);

    [Header("Pattern Settings")]
    [Tooltip("Включить шахматный порядок размещения ячеек (используются первые два префаба из списка).")]
    public bool useChessPattern = false;
    public CellData.CellType defaultCellType = CellData.CellType.None;

    [Header("Input Material Lists")]
    public List<CellTypeAndTypeMaterial> WaterMaterials;
    public List<CellTypeAndTypeMaterial> LandMaterials;
    public List<CellTypeAndTypeMaterial> ForestMaterials;
    public List<CellTypeAndTypeMaterial> LavaMaterials;
    // --- Внутренние данные ---
    private CellData[,] gridCells;
    private Vector2 calculatedCellDimensions = Vector2.one;
    
    private List<CellTypeAndCoordinates> inputPointsList;
    private Dictionary<Vector2Int, CellData.CellType> inputPointsDict;
    Vector2 currentSpacing;
    Vector2 stepDistance;
    Vector3 gridOffset;
    private WallsControl wallsControl;

    // --- Структуры для типов и материалов ---
    [System.Serializable]
    public struct CellTypeAndCoordinates
    {
        public Vector2Int ChessOrderposition;
        public CellData.CellType cellType;
        public CellTypeAndCoordinates(Vector2Int position, CellData.CellType type)
        {
            ChessOrderposition = position;
            cellType = type;
        }
    }

    [System.Serializable]
    public struct CellTypeAndTypeMaterial
    {
        public CellData.CellType cellType;
        public Material cellMaterial;
        public CellTypeAndTypeMaterial(CellData.CellType type, Material material)
        {
            cellType = type;
            cellMaterial = material;
        }
    }

    

    // --- Основной цикл ---
    
    public override void Init()
    {
        wallsControl = Main.Instance.GetController<WallsControl>();

        InitCells();
        CreateCells();
        // Написать фунцкию переопределения логики ячеек, которая будет вызываться после CreateCells() и будет отвечать за:
        SetCellsAreaType(); // Установка типа области ячейки, если нужно
        DrawCells();

        
    }

    /// <summary>
    /// 1. Инициализация данных, контейнеров, словарей.
    /// </summary>
    public void InitCells()
    {
        if (!IsCellPrefabValid(0) || gridWidth <= 0 || gridHeight <= 0)
        {
            Debug.LogError("CellsControl: Некорректные параметры!");
            return;
        }

        inputPointsList = inputData?.GetInputPointsList();
        BuildInputPointsDictionary();
        GenerateCellContainer();

        CreateCellDataMassif();
       calculatedCellDimensions = CalculateCellDimensions();

        currentSpacing = GetCurrentSpacing();
        stepDistance = GetStepDistance(currentSpacing);
        gridOffset = CalculateGridOffset(stepDistance);
        

        // 
        //CalculateCells: Написать Аналитический блок, который будет вызываться в CreateCells() и будет отвечать за:
        // 1. Определение логики ячеек (например, шахматный порядок), задание типа ячейки, CellArea.
        //drawCells
        //createWalls
    }
    /// <summary>
    /// 2. Расчёт и создание сетки ячеек, заполнение массива gridCells.
    /// </summary>
    public void CreateCells() // исключить  DrawCells(); и GenerateWalls(); из этого метода, чтобы не вызывать их при каждом пересчете
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                GameObject prefabToInstantiate;
                GameObject cellInstance = CreateCell(x, y, stepDistance, gridOffset, out prefabToInstantiate);
                if (cellInstance == null || prefabToInstantiate == null)
                {
                    Debug.LogError($"CellsControl: Префаб для ячейки ({x}, {y}) не валиден, пропускаем создание ячейки.");
                    continue;
                }// Если префаб не валиден, пропускаем создание ячейки
                ApplyCellsData(cellInstance, x, y, (x + y) % 2 == 0, prefabToInstantiate);
            }
        }        
    }

    /// <summary>
    /// 3. Визуальное оформление ячеек (материалы и т.п.).
    /// </summary>
    public void DrawCells()
    {
        
        if (gridCells == null) return;
        foreach (CellData cell in gridCells)
        {
            if (cell != null && cell.instance != null)
            {
                Renderer cellRenderer = cell.instance.GetComponentInChildren<Renderer>();
                if (cell.cellType == CellData.CellType.Water && IsMaterialListValid(WaterMaterials))
                {
                    int idx = Random.Range(0, WaterMaterials.Count);
                    cellRenderer.material = WaterMaterials[idx].cellMaterial;
                }
                else if (cell.cellType == CellData.CellType.Land && IsMaterialListValid(LandMaterials))
                {
                    int idx = Random.Range(0, LandMaterials.Count);
                    cellRenderer.material = LandMaterials[idx].cellMaterial;
                }
                else if (cell.cellType == CellData.CellType.Forest && IsMaterialListValid(ForestMaterials))
                {
                    int idx = Random.Range(0, ForestMaterials.Count);
                    cellRenderer.material = ForestMaterials[idx].cellMaterial;
                }
                else if (cell.cellType == CellData.CellType.Lava && IsMaterialListValid(LavaMaterials))
                {
                    int idx = Random.Range(0, LavaMaterials.Count);
                    cellRenderer.material = LavaMaterials[idx].cellMaterial;
                }
            }
        }
    }

    public void SetCellsAreaType()
    {
        if (gridCells == null) return;

        // Создаём массив для отслеживания, какие ячейки уже обработаны
        bool[,] visited = new bool[gridWidth, gridHeight];

        // Вспомогательная функция для распространения типа области по смежным ячейкам (BFS)
        void SpreadAreaType(int startX, int startY, CellData.CellAreaType areaType)
        {
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(new Vector2Int(startX, startY));
            visited[startX, startY] = true;

            while (queue.Count > 0)
            {
                Vector2Int pos = queue.Dequeue();
                CellData cell = gridCells[pos.x, pos.y];
                if (cell != null && !cell.cellAreaTypes.Contains(areaType))
                {
                    cell.SetCellAreaType(areaType);
                }

                // Проверяем 4 соседей (по сетке)
                Vector2Int[] directions = new Vector2Int[]
                {
                    new Vector2Int(0, 1),   // вверх
                    new Vector2Int(1, 0),   // вправо
                    new Vector2Int(0, -1),  // вниз
                    new Vector2Int(-1, 0),  // влево
                };

                foreach (var dir in directions)
                {
                    int nx = pos.x + dir.x;
                    int ny = pos.y + dir.y;
                    if (nx >= 0 && nx < gridWidth && ny >= 0 && ny < gridHeight && !visited[nx, ny])
                    {
                        CellData neighbor = gridCells[nx, ny];
                        if (neighbor != null && neighbor.cellAreaTypes.Contains(areaType))
                        {
                            visited[nx, ny] = true;
                            queue.Enqueue(new Vector2Int(nx, ny));
                        }
                    }
                }
            }
        }

        // Для каждого типа области ищем стартовые ячейки и распространяем тип
        foreach (CellData.CellAreaType areaType in System.Enum.GetValues(typeof(CellData.CellAreaType)))
        {
            // Пропускаем None
            if (areaType == CellData.CellAreaType.None)
                continue;

            // Сброс посещённых для каждого типа
            System.Array.Clear(visited, 0, visited.Length);

            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (!visited[x, y] && gridCells[x, y] != null && gridCells[x, y].cellAreaTypes.Contains(areaType))
                    {
                        SpreadAreaType(x, y, areaType);
                    }
                }
            }
        }
    }

    // --- Вспомогательные методы ---

    private void BuildInputPointsDictionary()
    {
        inputPointsDict = null;
        if (inputPointsList != null && inputPointsList.Count > 0)
        {
            inputPointsDict = new Dictionary<Vector2Int, CellData.CellType>();
            foreach (var point in inputPointsList)
                inputPointsDict[point.ChessOrderposition] = point.cellType;
        }
    }

    private void GenerateCellContainer()
    {
        if (cellContainer == null)
        {
            GameObject cellContainerGO = new GameObject("CellsContainer_AutoGenerated");
            cellContainer = cellContainerGO.transform;
            cellContainer.SetParent(transform);
            cellContainerGO.transform.localPosition = Vector3.zero;
        }
    }

    

    private void CreateCellDataMassif()
    {
        gridCells = new CellData[gridWidth, gridHeight];
    }

    private void ClearGrid()
    {
        if (cellContainer != null)
        {
            for (int i = cellContainer.childCount - 1; i >= 0; i--)
                Destroy(cellContainer.GetChild(i).gameObject);
        }
        gridCells = null;
    }

    

    private bool IsCellPrefabValid(int index = 0)
    {
        return cellPrefabs != null && cellPrefabs.Count > index && cellPrefabs[index] != null;
    }

    private bool IsMaterialListValid(List<CellTypeAndTypeMaterial> list)
    {
        return list != null && list.Count > 0 && list[0].cellMaterial != null;
    }

    private Vector2 CalculateCellDimensions()
    {

        Vector2 calculatedCellDimensions = Vector2.zero;
        if (!IsCellPrefabValid(0))
        {
            calculatedCellDimensions = Vector2.one;
            return calculatedCellDimensions;
        }
        Renderer prefabRenderer = cellPrefabs[0].GetComponentInChildren<Renderer>();
        if (prefabRenderer != null)
        {
            calculatedCellDimensions = new Vector2(prefabRenderer.bounds.size.x, prefabRenderer.bounds.size.z);
        }
        else
        {
            calculatedCellDimensions = new Vector2(cellPrefabs[0].transform.localScale.x, cellPrefabs[0].transform.localScale.z);
        }
        if (calculatedCellDimensions.x <= 0 || calculatedCellDimensions.y <= 0)
        {
            calculatedCellDimensions = Vector2.one;
            return calculatedCellDimensions;
        }
        return calculatedCellDimensions;

    }

    public Vector2 GetCurrentSpacing()
    {
        return spacingMode == SpacingTypeOptions.Manual ? manualSpacing : Vector2.zero;
    }

    public Vector2 GetStepDistance(Vector2 currentSpacing)
    {
        return new Vector2(
            calculatedCellDimensions.x + currentSpacing.x,
            calculatedCellDimensions.y + currentSpacing.y
        );
    }

    public Vector3 CalculateGridOffset(Vector2 stepDistance)
    {
        float totalGridVisualWidth = (gridWidth > 1) ? (gridWidth - 1) * stepDistance.x : 0;
        float totalGridVisualHeight = (gridHeight > 1) ? (gridHeight - 1) * stepDistance.y : 0;
        return new Vector3(totalGridVisualWidth / 2.0f, 0, totalGridVisualHeight / 2.0f);
    }

    private GameObject CreateCell(int x, int y, Vector2 stepDistance, Vector3 gridOffset, out GameObject PrefabToInstantiate)
    {
        PrefabToInstantiate = null;

        GameObject prefabToInstantiate = SelectCellPrefab(x, y);
        if (prefabToInstantiate == null)
            return null;
        
        PrefabToInstantiate = prefabToInstantiate;
        Vector3 cellPosition = CalculateCellPosition(x, y, stepDistance, gridOffset);
        GameObject cellInstance = Instantiate(prefabToInstantiate, cellContainer);
        cellInstance.transform.localPosition = cellPosition;
        cellInstance.transform.localRotation = Quaternion.identity;

        return cellInstance;
        //InitializeCellData(cellInstance, x, y, (x + y) % 2 == 0, prefabToInstantiate);
    }

    private GameObject SelectCellPrefab(int x, int y)
    {
        bool isWhiteSquare = (x + y) % 2 == 0;
        if (useChessPattern && cellPrefabs.Count > 1 && cellPrefabs[1] != null)
            return isWhiteSquare ? cellPrefabs[0] : cellPrefabs[1];
        return IsCellPrefabValid(0) ? cellPrefabs[0] : null;
    }

    public Vector3 CalculateCellPosition(int x, int y, Vector2 stepDistance, Vector3 gridOffset)
    {
        return new Vector3(
            x * stepDistance.x - gridOffset.x,
            0,
            y * stepDistance.y - gridOffset.z
        );
    }

    private void ApplyCellsData(GameObject cellInstance, int x, int y, bool isWhiteSquare, GameObject prefabToInstantiate)
    {
        if (gridCells == null || x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
            return;
        if (cellInstance == null)
        {
            Debug.LogError($"CellsControl: Не удалось создать ячейку ({x}, {y}), cellInstance пустой.");
            return;
        }

        cellInstance.name = $"Cell_{x}_{y}";
        CellData cellData = cellInstance.GetComponent<CellData>();
        if (cellData == null)
            cellData = cellInstance.AddComponent<CellData>();
        gridCells[x, y] = cellData;
        cellData.Init(cellInstance, cellInstance.transform.position, prefabToInstantiate);

        CellData.CellType cellType = defaultCellType;
        if (inputPointsDict != null && inputPointsDict.TryGetValue(new Vector2Int(x, y), out var foundType))
            cellType = foundType;

        cellData.SetCellData(new Vector2Int(x, y), isWhiteSquare, cellType);
    }

    // --- Генерация стен (сохраняется) ---
    

    

   

    // --- Гизмо (сохраняется) ---
    private void OnDrawGizmosSelected()
    {
        if (!IsCellPrefabValid(0))
            return;

        Vector2 cellDims = calculatedCellDimensions;
        if (cellDims == Vector2.zero || cellDims == Vector2.one)
        {
            Renderer prefabRenderer = cellPrefabs[0].GetComponentInChildren<Renderer>();
            if (prefabRenderer != null)
                cellDims = new Vector2(prefabRenderer.bounds.size.x, prefabRenderer.bounds.size.z);
            else
                cellDims = new Vector2(cellPrefabs[0].transform.localScale.x, cellPrefabs[0].transform.localScale.z);
            if (cellDims.x <= 0) cellDims.x = 1f;
            if (cellDims.y <= 0) cellDims.y = 1f;
        }

        Vector2 currentSpacing = spacingMode == SpacingTypeOptions.Manual ? manualSpacing : Vector2.zero;
        Vector2 stepDistance = new Vector2(cellDims.x + currentSpacing.x, cellDims.y + currentSpacing.y);

        float totalGridVisualWidth = (gridWidth > 1) ? (gridWidth - 1) * stepDistance.x : 0;
        float totalGridVisualHeight = (gridHeight > 1) ? (gridHeight - 1) * stepDistance.y : 0;
        Vector3 gridOffset = new Vector3(totalGridVisualWidth / 2.0f, 0, totalGridVisualHeight / 2.0f);

        Transform container = cellContainer != null ? cellContainer : this.transform;
        Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.5f);

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector3 cellCenterLocalPosition = new Vector3(
                    x * stepDistance.x - gridOffset.x,
                    0,
                    y * stepDistance.y - gridOffset.z
                );
                Vector3 worldCellCenter = container.TransformPoint(cellCenterLocalPosition);
                Gizmos.matrix = Matrix4x4.TRS(worldCellCenter, container.rotation, Vector3.one);
                Gizmos.DrawCube(Vector3.zero, new Vector3(cellDims.x, Mathf.Min(cellDims.x, cellDims.y) * 0.1f, cellDims.y));
            }
        }
        Gizmos.matrix = Matrix4x4.identity;
    }

    // --- Контекстные меню для инспектора ---
    [ContextMenu("Init Cells")]
    public void ContextMenuInitCells() => InitCells();

    [ContextMenu("Create Cells")]
    public void ContextMenuCreateCells() => CreateCells();

    [ContextMenu("Draw Cells")]
    public void ContextMenuDrawCells() => DrawCells();

    
    [ContextMenu("Clear Grid")]
    public void ClearMap()
    {
        ClearGrid();
        
    }
}