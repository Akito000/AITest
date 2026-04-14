
//敵の最上位ゴール
using UnityEngine;

public enum EnemyGoalType
{
    None,
    ATTACK,  //プレイヤーを攻撃する
    WANDER, //徘徊
    ESCAPE, //にげる
    RESPAWN,//リスポーン
}

public enum EnemySubGoalType
{
    None,

    Attack_DecideTargetCell,//移動セルを決める
    Attack_MoveToTargetCell,//決めたセルまで移動する

    Wander_SelectPatrolPoint,//徘徊するpointを決める
    Wander_MoveToPatrolPoint,//決めたpointまで移動する

    Escape_SelectSafeCell,//にげるのに適したセルを決める
    Escape_MoveToSafeCell,//決めたセルまで移動する

    Respawn_Move,//リスポーン位置まで移動する
}

//毎frameチェックする情報
public struct EnemyContext
{
    public Vector2Int thisCell; //自分の位置
    public Vector2Int playerCell;   //プレイヤーの位置
    public Vector2Int playerForward;//プレイヤーの進行方向

    public bool isCower;//いじけ中？
    public bool isRespawn;//リスポーン中か？

    public bool isWander;//徘徊中か？
    public Vector2Int respawnCell; //リスポーン先タイル

    public bool reachedTargetCell;//ターゲットセルに到着したか？
}

public class EnemyMemory
{
    public Vector2Int CurrentTargetCell;//現在のターゲットセル
    public EnemyGoalType currentGoal = EnemyGoalType.None;//現在選んでるゴール
    public EnemySubGoalType currentSubGoal = EnemySubGoalType.None;//現在のサブゴール

    public Vector2Int lastMoveDirection = Vector2Int.zero;//前回進んだ進行方向
    public Vector2Int lastPlayerCell;//最後にプレイヤーを認知したプレイヤータイル

    public float lastReplanTime;//前回プランを作った時間


    public int currentPatrolIndex = 0;//パトロールインデックス
}

//今回実行するプラン　最上位ゴールとその評価
public struct EnemyPlan
{
    public EnemyGoalType goalType;
    public float score;
    public Vector2Int targetCell;

    public EnemyPlan(EnemyGoalType _goalType, float _score, Vector2Int _targetTile)
    {
        goalType = _goalType;
        score = _score;
        targetCell = _targetTile;
    }

}