using System.Collections.Generic;
using UnityEngine;

public class Stage : MonoBehaviour
{
    public static Stage Instance { get; private set; }

    [Header("マス設定")]
    [SerializeField] private float cellSize = 1.0f;
    [SerializeField] private float floorThickness = 0.2f;
    [SerializeField] private float wallHeight = 2.0f;

    [Header("Prefab")]
    [SerializeField] private GameObject pelletPrefab;

    [Header("レイヤー")]
    [SerializeField] private string wallLayerName = "Wall";
    private const int CELL_EMPTY = 0;
    private const int CELL_WALL = 1;
    private const int CELL_PELLET = 2;
    private const int CELL_PLAYER = 3;
    private const int CELL_ENEMY_RED = 4;
    private const int CELL_ENEMY_PINK = 5;
    private const int CELL_ENEMY_BLUE = 6;
    private const int CELL_ENEMY_ORANGE = 7;
    private const int CELL_WARP = 10;
    [Header("マップ")]
    [TextArea(10, 30)]

    private int[,] mapData =
    {
    {2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2},
    {2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2},
    {2, 1, 2, 1, 1, 2, 1, 1, 1, 2, 1, 2, 1, 1, 1, 2, 1, 1, 2, 1, 2},
    {2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2},
    {2, 1, 2, 1, 1, 2, 1, 2, 1, 1, 1, 1, 1, 2, 1, 2, 1, 1, 2, 1, 2},
    {2, 1, 2, 2, 2, 2, 1, 2, 2, 2, 1, 2, 2, 2, 1, 2, 2, 2, 2, 1, 2},
    {2, 1, 1, 1, 1, 2, 1, 1, 1, 2, 1, 2, 1, 1, 1, 2, 1, 1, 1, 1, 2},
    {2, 2, 2, 2, 1, 2, 1, 2, 2, 2, 2, 2, 2, 2, 1, 2, 1, 2, 2, 2, 2},
    {2, 1, 1, 1, 1, 2, 1, 2, 1, 1, 4, 1, 1, 2, 1, 2, 1, 1, 1, 1, 2},
    {2, 2, 2, 2, 1, 2, 2, 2, 1, 5, 2, 2, 1, 2, 2, 2, 1, 2, 2, 2, 2},
    {2, 1, 1, 1, 1, 2, 1, 2, 1, 1, 1, 1, 1, 2, 1, 2, 1, 1, 1, 1, 2},
    {2, 2, 2, 2, 1, 2, 1, 2, 2, 2, 2, 2, 2, 2, 1, 2, 1, 2, 2, 2, 2},
    {2, 1, 1, 1, 1, 2, 1, 2, 1, 1, 1, 1, 1, 2, 1, 2, 1, 1, 1, 1, 2},
    {2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2},
    {2, 1, 2, 1, 1, 2, 1, 1, 1, 2, 1, 2, 1, 1, 1, 2, 1, 1, 2, 1, 2},
    {2, 1, 2, 2, 1, 2, 2, 2, 2, 2, 3, 2, 2, 2, 2, 2, 1, 2, 2, 1, 2},
    {2, 1, 1, 2, 1, 2, 1, 2, 1, 1, 1, 1, 1, 2, 1, 2, 1, 2, 1, 1, 2},
    {2, 1, 2, 2, 2, 2, 1, 2, 2, 2, 1, 2, 2, 2, 1, 2, 2, 2, 2, 1, 2},
    {2, 1, 2, 1, 1, 1, 1, 1, 1, 2, 1, 2, 1, 1, 1, 1, 1, 1, 2, 1, 2},
    {2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2},
    {2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2},
};


    private bool[,] wallMap;

    private int width;
    private int height;

    public int Width => width;
    public int Height => height;

    private CharacterAgent playerAgent;

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyRedPrefab;
    [SerializeField] private GameObject enemyPinkPrefab;
    [SerializeField] private GameObject enemyBluePrefab;
    [SerializeField] private GameObject enemyOrangePrefab;


    private bool[,] pelletMap;

    public bool HasPellet(Vector2Int cell)
    {
        if (!IsInside(cell))
            return false;

        return pelletMap[cell.x, cell.y];
    }

    public bool ConsumePellet(Vector2Int cell)
    {
        if (!IsInside(cell))
            return false;

        if (!pelletMap[cell.x, cell.y])
            return false;

        pelletMap[cell.x, cell.y] = false;
        return true;
    }

    public void SetPellet(Vector2Int cell, bool exists)
    {
        if (!IsInside(cell))
            return;

        pelletMap[cell.x, cell.y] = exists;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Generate();
    }
    public enum EnemySpawnType
    {
        Red,
        Pink,
        Blue,
        Orange,
    }

    private struct EnemySpawnRequest
    {
        public Vector2Int Cell;
        public EnemySpawnType Type;

        public EnemySpawnRequest(Vector2Int cell, EnemySpawnType type)
        {
            Cell = cell;
            Type = type;
        }
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        ClearGeneratedRoots();

        height = mapData.GetLength(0);
        width = mapData.GetLength(1);

        wallMap = new bool[width, height];
        pelletMap = new bool[width, height];
        playerAgent = null;

        Transform floorRoot = CreateRoot("Floor");
        Transform wallRoot = CreateRoot("Walls");
        Transform pelletRoot = CreateRoot("Pellets");
        Transform actorRoot = CreateRoot("Actors");
        CreateFloor(floorRoot);
        // 敵は後でまとめて生成する
        List<EnemySpawnRequest> enemySpawnRequests = new List<EnemySpawnRequest>();

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                int cellValue = mapData[row, col];

                // 上の行がワールド上では上側に来るように反転
                Vector2Int cell = new Vector2Int(col, height - 1 - row);

                switch (cellValue)
                {
                    case CELL_WALL:
                        wallMap[cell.x, cell.y] = true;
                        CreateWall(wallRoot, cell);
                        break;

                    case CELL_PELLET:
                        SetPellet(cell, true);
                        CreatePellet(pelletRoot, cell);
                        break;

                    case CELL_PLAYER:
                        CreatePlayer(actorRoot, cell);
                        break;

                    case CELL_ENEMY_RED:
                        enemySpawnRequests.Add(new EnemySpawnRequest(cell, EnemySpawnType.Red));
                        break;

                    case CELL_ENEMY_PINK:
                        enemySpawnRequests.Add(new EnemySpawnRequest(cell, EnemySpawnType.Pink));
                        break;

                    case CELL_ENEMY_BLUE:
                        // enemySpawnRequests.Add(new EnemySpawnRequest(cell, EnemySpawnType.Blue));
                        break;

                    case CELL_ENEMY_ORANGE:
                        //  enemySpawnRequests.Add(new EnemySpawnRequest(cell, EnemySpawnType.Orange));
                        break;
                }
            }
        }

        // プレイヤー生成後に敵をまとめて生成する
        for (int i = 0; i < enemySpawnRequests.Count; i++)
        {
            EnemySpawnRequest request = enemySpawnRequests[i];
            //     CreateEnemy(actorRoot, request.Cell, request.Type);
            CreateEnemy_Goal(actorRoot, request.Cell, request.Type);
        }



    }

    private void CreatePlayer(Transform parent, Vector2Int cell)
    {
        if (playerPrefab == null)
        {
            Debug.LogWarning("Stage: playerPrefab が未設定です。");
            return;
        }

        Vector3 pos = CellToWorld(cell);

        GameObject playerObject = Instantiate(playerPrefab, pos, Quaternion.identity, parent);

        CharacterAgent agent = playerObject.GetComponent<CharacterAgent>();

        agent.SetUp(this, cell);
        playerAgent = agent;
    }

    private void CreateEnemy(Transform parent, Vector2Int cell, EnemySpawnType type)
    {
        GameObject enemyPrefab = GetEnemyPrefab(type);
        if (enemyPrefab == null)
        {
            Debug.LogWarning($"Stage: {type} の enemyPrefab が未設定です。");
            return;
        }

        Vector3 pos = CellToWorld(cell);

        GameObject enemyObject = Instantiate(enemyPrefab, pos, Quaternion.identity, parent);
        enemyObject.name = $"Enemy_{type}";

        CharacterAgent agent = enemyObject.GetComponent<CharacterAgent>();
        if (agent == null)
        {
            agent = enemyObject.AddComponent<CharacterAgent>();
        }

        EnemyBase enemy = enemyObject.GetComponent<EnemyBase>();
        if (enemy == null)
        {
            Debug.LogWarning($"{enemyObject.name} に EnemyBase 継承コンポーネントが付いていません。");
            return;
        }

        Collider col = enemyObject.GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphere = enemyObject.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
        }

        agent.SetUp(this, cell);

        if (playerAgent != null)
        {
            Debug.Log("設定");
            enemy.SetPlayerAgent(playerAgent);
        }
    }

    private void CreateEnemy_Goal(Transform parent, Vector2Int cell, EnemySpawnType type)
    {
        GameObject enemyPrefab = GetEnemyPrefab(type);
        if (enemyPrefab == null)
        {
            Debug.LogWarning($"Stage: {type} の enemyPrefab が未設定です。");
            return;
        }

        Vector3 pos = CellToWorld(cell);

        GameObject enemyObject = Instantiate(enemyPrefab, pos, Quaternion.identity, parent);
        enemyObject.name = $"Enemy_{type}";

        CharacterAgent agent = enemyObject.GetComponent<CharacterAgent>();
        if (agent == null)
        {
            agent = enemyObject.AddComponent<CharacterAgent>();
        }

        EnemyMoveAgent enemy = enemyObject.GetComponent<EnemyMoveAgent>();
        if (enemy == null)
        {
            Debug.LogWarning($"{enemyObject.name} に EnemyBase 継承コンポーネントが付いていません。");
            return;
        }

        agent.SetUp(this, cell);

        if (playerAgent != null)
        {
            Debug.Log("設定");
            enemy.Initialize(this, playerAgent.transform);
        }
    }

    private GameObject GetEnemyPrefab(EnemySpawnType type)
    {
        switch (type)
        {
            case EnemySpawnType.Red:
                return enemyRedPrefab;

            case EnemySpawnType.Pink:
                return enemyPinkPrefab;

            case EnemySpawnType.Blue:
                return enemyBluePrefab;

            case EnemySpawnType.Orange:
                return enemyOrangePrefab;

            default:
                return null;
        }
    }


    private void ClearGeneratedRoots()
    {
        string[] generatedRootNames =
        {
        "Floor",
        "Walls",
        "Pellets"
    };

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

            bool shouldDelete = false;
            for (int j = 0; j < generatedRootNames.Length; j++)
            {
                if (child.name == generatedRootNames[j])
                {
                    shouldDelete = true;
                    break;
                }
            }

            if (!shouldDelete)
                continue;

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }

    //ステージ内か？
    public bool IsInside(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;
    }

    //移動できるセルか？
    public bool IsWalkable(Vector2Int cell)
    {
        if (!IsInside(cell))
            return false;

        return !wallMap[cell.x, cell.y];
    }

    //セルの位置をワールド座標に
    public Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(cell.x * cellSize, 0.5f, cell.y * cellSize);
    }

    //ワールド座標をセルに
    public Vector2Int WorldToCell(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x / cellSize);
        int y = Mathf.RoundToInt(worldPosition.z / cellSize);
        return new Vector2Int(x, y);
    }
    private void CreateFloor(Transform parent)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(parent);

        float centerX = (width - 1) * cellSize * 0.5f;
        float centerZ = (height - 1) * cellSize * 0.5f;

        floor.transform.position = new Vector3(centerX, -floorThickness * 0.5f, centerZ);
        floor.transform.localScale = new Vector3(width * cellSize, floorThickness, height * cellSize);

        Renderer renderer = floor.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(0.08f, 0.08f, 0.12f);
        }
    }

    private void CreateWall(Transform parent, Vector2Int cell)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = $"Wall_{cell.x}_{cell.y}";
        wall.transform.SetParent(parent);

        Vector3 pos = CellToWorld(cell);
        wall.transform.position = new Vector3(pos.x, wallHeight * 0.5f, pos.z);
        wall.transform.localScale = new Vector3(cellSize, wallHeight, cellSize);

        int wallLayer = LayerMask.NameToLayer(wallLayerName);
        if (wallLayer >= 0)
        {
            wall.layer = wallLayer;
        }

        Renderer renderer = wall.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(1, 1f, 1f);
        }
    }
    private void CreatePellet(Transform parent, Vector2Int cell)
    {
        Vector3 pos = CellToWorld(cell);

        GameObject pellet;
        if (pelletPrefab != null)
        {
            pellet = Instantiate(pelletPrefab, pos + Vector3.up * -0.15f, Quaternion.identity, parent);
        }
        else
        {
            pellet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pellet.transform.SetParent(parent);
            pellet.transform.position = pos + Vector3.up * -0.15f;
            pellet.transform.localScale = Vector3.one * (cellSize * 0.2f);

            Renderer renderer = pellet.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.yellow;
            }
        }

        pellet.name = $"Pellet_{cell.x}_{cell.y}";

        Collider col = pellet.GetComponent<Collider>();
        if (col == null)
        {
            col = pellet.AddComponent<SphereCollider>();
        }

        col.isTrigger = true;


    }

    private Transform CreateRoot(string rootName)
    {
        GameObject root = new GameObject(rootName);
        root.transform.SetParent(transform);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;
        return root.transform;
    }


    /// <summary>
    /// 指定セルがワープマスか
    /// </summary>
    private bool IsWarpCell(Vector2Int cell)
    {
        if (!IsInside(cell))
        {
            return false;
        }

        int row = height - 1 - cell.y;
        int col = cell.x;

        return mapData[row, col] == CELL_WARP;
    }

    /// <summary>
    /// 現在セルから指定方向へ進もうとした時、ワープが発生するなら
    /// 反対側のワープ先セルを返す
    /// </summary>
    public bool TryGetWarpDestination(Vector2Int currentCell, MoveDirection direction, out Vector2Int destination)
    {
        destination = currentCell;

        if (direction == MoveDirection.NONE)
        {
            return false;
        }

        // 今いる場所がワープマスでないならワープしない
        if (!IsWarpCell(currentCell))
        {
            return false;
        }

        Vector2Int nextCell = currentCell + MoveDirectionUtility.ToVector2Int(direction);

        // まだステージ内なら普通移動
        if (IsInside(nextCell))
        {
            return false;
        }

        destination = FindWarpDestination(currentCell, direction);

        // 自分自身しか見つからなかったらワープ失敗扱い
        return destination != currentCell;
    }

    /// <summary>
    /// 反対側のワープマスを探す
    /// 左右ワープを想定して、同じ行の反対側の10を探す
    /// </summary>
    private Vector2Int FindWarpDestination(Vector2Int currentCell, MoveDirection direction)
    {
        int row = height - 1 - currentCell.y;

        switch (direction)
        {
            case MoveDirection.LEFT:
                for (int col = width - 1; col >= 0; col--)
                {
                    if (col == currentCell.x)
                    {
                        continue;
                    }

                    if (mapData[row, col] == CELL_WARP)
                    {
                        return new Vector2Int(col, currentCell.y);
                    }
                }
                break;

            case MoveDirection.RIGHT:
                for (int col = 0; col < width; col++)
                {
                    if (col == currentCell.x)
                    {
                        continue;
                    }

                    if (mapData[row, col] == CELL_WARP)
                    {
                        return new Vector2Int(col, currentCell.y);
                    }
                }
                break;
        }

        return currentCell;
    }

    /// <summary>
    /// 通常の隣接セルを返す
    /// ワープ処理はしない
    /// </summary>
    public Vector2Int GetAdjacentCell(Vector2Int currentCell, MoveDirection direction)
    {
        return currentCell + MoveDirectionUtility.ToVector2Int(direction);
    }

    /// <summary>
    /// 現在セルから指定方向へ進めるか
    /// ワープも含めて判定する
    /// </summary>
    public bool CanMoveTo(Vector2Int currentCell, MoveDirection direction)
    {
        if (direction == MoveDirection.NONE)
        {
            return false;
        }

        //if (TryGetWarpDestination(currentCell, direction, out _))
        //{
        //    return true;
        //}

        Vector2Int nextCell = GetAdjacentCell(currentCell, direction);
        return IsWalkable(nextCell);
    }

}
