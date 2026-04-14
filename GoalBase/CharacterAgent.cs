//using UnityEngine;

//public class CharacterAgent : MonoBehaviour
//{
//    [SerializeField] private float moveSpeed = 4.0f;
//    [SerializeField] private Transform model;

//    private Rigidbody rb;
//    private IMoveAgent agent;
//    private Stage stage;

//    private bool isMoving = false;
//    private Vector3 targetWorldPosition;
//    private Vector2Int targetCell;
//    public Vector2Int CurrentCell { get; private set; }
//    public MoveDirection CurrentMoveDirection { get; private set; } = MoveDirection.NONE;
//    public MoveDirection NextMoveDirection { get; private set; } = MoveDirection.NONE;

//    private void Awake()
//    {
//        rb = GetComponent<Rigidbody>();
//        agent = GetComponent<IMoveAgent>();

//        rb.useGravity = false;
//        rb.isKinematic = true;
//        rb.interpolation = RigidbodyInterpolation.Interpolate;

//        if (model == null)
//        {
//            model = transform;
//        }
//    }

//    public void SetUp(Stage _stage, Vector2Int _startCell)
//    {
//        stage = _stage;
//        CurrentCell = _startCell;
//        targetWorldPosition = stage.CellToWorld(CurrentCell);
//        targetCell = _startCell;
//        transform.position = targetWorldPosition;

//    }

//    private void Update()
//    {
//        if (agent != null)
//        {
//            NextMoveDirection = agent.NextDirection();
//        }
//        else
//        {
//            NextMoveDirection = MoveDirection.NONE;
//        }
//    }

//    private void FixedUpdate()
//    {
//        if (stage == null)
//            return;

//        if (isMoving)
//        {
//            bool reached = MoveTowardsTarget();

//            if (reached)
//            {
//                CurrentCell = targetCell;
//                isMoving = false;

//                TryStartMove();
//            }

//            return;
//        }

//        TryStartMove();
//    }
//    private bool MoveTowardsTarget()
//    {
//        Vector3 current = rb.position;
//        Vector3 next = Vector3.MoveTowards(current, targetWorldPosition, moveSpeed * Time.fixedDeltaTime);

//        rb.MovePosition(next);

//        Vector3 flatDir = targetWorldPosition - current;
//        flatDir.y = 0f;

//        if (flatDir.sqrMagnitude > 0.0001f)
//        {
//            model.forward = flatDir.normalized;
//        }

//        bool reached = (next - targetWorldPosition).sqrMagnitude <= 0.00001f;
//        if (reached)
//        {
//            rb.MovePosition(targetWorldPosition);
//        }

//        return reached;
//    }


//    private void TryStartMove()
//    {
//        MoveDirection selectedDirection = MoveDirection.NONE;

//        // 先行入力を最優先
//        if (CanMove(NextMoveDirection))
//        {
//            selectedDirection = NextMoveDirection;
//        }
//        else if (CanMove(CurrentMoveDirection))
//        {
//            // 入力方向に行けないなら、今の方向を継続
//            selectedDirection = CurrentMoveDirection;
//        }

//        if (selectedDirection == MoveDirection.NONE)
//        {
//            CurrentMoveDirection = MoveDirection.NONE;
//            return;
//        }

//        CurrentMoveDirection = selectedDirection;

//        targetCell = CurrentCell + MoveDirectionUtility.ToVector2Int(CurrentMoveDirection);
//        targetWorldPosition = stage.CellToWorld(targetCell);
//        isMoving = true;
//    }


//    private bool CanMove(MoveDirection _direction)
//    {
//        if (_direction == MoveDirection.NONE || stage == null)
//            return false;

//        Vector2Int nextCell = CurrentCell + MoveDirectionUtility.ToVector2Int(_direction);
//        return stage.IsWalkable(nextCell);

//    }




//}

using UnityEngine;

/// <summary>
/// gridベース移動
/// 移動は transform で行い、Rigidbody は Trigger 用にだけ残す想定
/// </summary>
public class CharacterAgent : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4.0f;
    [SerializeField] private Transform model;

    private IMoveAgent agent;
    private Stage stage;

    private bool isMoving = false;
    private Vector3 targetWorldPosition;
    private Vector2Int targetCell;

    public Vector2Int CurrentCell { get; private set; }
    public MoveDirection CurrentMoveDirection = MoveDirection.NONE;
    public MoveDirection NextMoveDirection = MoveDirection.NONE;

    private void Awake()
    {
        agent = GetComponent<IMoveAgent>();

        if (model == null)
        {
            model = transform;
        }
    }

    public void SetUp(Stage _stage, Vector2Int _startCell)
    {
        stage = _stage;
        CurrentCell = _startCell;
        targetCell = _startCell;
        targetWorldPosition = stage.CellToWorld(CurrentCell);
        transform.position = targetWorldPosition;
    }

    private void Update()
    {
        RefreshNextMoveDirection();
        MoveUpdate(Time.deltaTime);
    }

    private void RefreshNextMoveDirection()
    {
        if (agent != null)
        {
            NextMoveDirection = agent.NextDirection();
        }
        else
        {
            NextMoveDirection = MoveDirection.NONE;
        }
    }

    private void MoveUpdate(float deltaTime)
    {
        if (stage == null)
            return;

        if (isMoving)
        {
            bool reached = MoveTowardsTarget(deltaTime);

            if (reached)
            {
                CurrentCell = targetCell;
                isMoving = false;

                // 新しいセルに到着した後の情報で、もう一度方向を決め直す
                RefreshNextMoveDirection();

                TryStartMove();
            }

            return;
        }

        TryStartMove();
    }

    //private bool MoveTowardsTarget(float deltaTime)
    //{
    //    Vector3 current = transform.position;
    //    Vector3 next = Vector3.MoveTowards(current, targetWorldPosition, moveSpeed * deltaTime);

    //    transform.position = next;

    //    Vector3 flatDir = targetWorldPosition - current;
    //    flatDir.y = 0f;

    //    if (flatDir.sqrMagnitude > 0.0001f)
    //    {
    //        model.forward = flatDir.normalized;
    //    }

    //    bool reached = (next - targetWorldPosition).sqrMagnitude <= 0.00001f;
    //    if (reached)
    //    {
    //        transform.position = targetWorldPosition;
    //    }

    //    return reached;
    //}

    private bool MoveTowardsTarget(float deltaTime)
    {
        Vector3 current = transform.position;

        // 今いる位置から目標までのベクトル
        Vector3 toTarget = targetWorldPosition - current;
        toTarget.y = 0f;

        // 目標までの距離
        float distance = toTarget.magnitude;

        // 今回進める距離
        float moveDistance = moveSpeed * deltaTime;

        // すでにかなり近いなら到着扱い
        if (distance <= 0.0001f)
        {
            transform.position = targetWorldPosition;
            return true;
        }

        // 向きだけ更新
        model.forward = toTarget.normalized;

        // 今回で到着できるなら、ぴったり合わせる
        if (distance <= moveDistance)
        {
            transform.position = targetWorldPosition;
            return true;
        }

        // まだ届かないなら、そのぶんだけ進む
        Vector3 next = current + toTarget.normalized * moveDistance;
        transform.position = next;

        return false;
    }



    private void TryStartMove()
    {
        MoveDirection selectedDirection = MoveDirection.NONE;

        // 先行入力を優先
        if (CanMove(NextMoveDirection))
        {
            selectedDirection = NextMoveDirection;
        }
        else if (CanMove(CurrentMoveDirection))
        {
            selectedDirection = CurrentMoveDirection;
        }

        if (selectedDirection == MoveDirection.NONE)
        {
            CurrentMoveDirection = MoveDirection.NONE;
            return;
        }

        CurrentMoveDirection = selectedDirection;

        // ここでワープを通常移動と分ける
        if (stage.TryGetWarpDestination(CurrentCell, CurrentMoveDirection, out Vector2Int warpCell))
        {
            // 反対側のワープマスへ瞬間移動
            CurrentCell = warpCell;
            targetCell = warpCell;
            targetWorldPosition = stage.CellToWorld(warpCell);
            transform.position = targetWorldPosition;

            // ワープ先のセル基準で、もう一度方向を取り直して通常移動を始める
            RefreshNextMoveDirection();
            TryStartMove();
            return;
        }

        // 普通の1マス移動
        targetCell = stage.GetAdjacentCell(CurrentCell, CurrentMoveDirection);
        targetWorldPosition = stage.CellToWorld(targetCell);
        isMoving = true;
    }

    public bool CanMove(MoveDirection direction)
    {
        //if (direction == MoveDirection.NONE || stage == null)
        //    return false;

        //Vector2Int nextCell = CurrentCell + MoveDirectionUtility.ToVector2Int(direction);
        //return stage.IsWalkable(nextCell);

        if (direction == MoveDirection.NONE || stage == null)
        {
            return false;
        }

        return stage.CanMoveTo(CurrentCell, direction);
    }
}
