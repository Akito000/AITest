using System;
using UnityEngine;

/// <summary>
/// パックマンのゲーム進行管理
/// ・取得数管理
/// ・残りドット管理
/// ・フルーツ出現判定
/// ・パワーモード管理
/// ・クリア / 失敗判定
/// をまとめて管理する
/// </summary>
public class GameSystem : MonoBehaviour
{
    public static GameSystem Instance { get; private set; }

    [Header("パワーモード設定")]
    [SerializeField] private float powerModeDuration = 6.0f;

    [Header("フルーツ出現条件（残りドット数以下で出現）")]
    [SerializeField] private int firstFruitAppearRemainingDots = 170;
    [SerializeField] private int secondFruitAppearRemainingDots = 70;

    [Header("参照")]
    //[SerializeField] private FruitSpawner fruitSpawner;

    //==================================================
    // 基本状態
    //==================================================

    /// <summary>クリア対象となる総ドット数</summary>
    public int TotalDots { get; private set; }

    /// <summary>取得済みドット数</summary>
    public int CollectedDots { get; private set; }

    /// <summary>残りドット数</summary>
    public int RemainingDots => Mathf.Max(0, TotalDots - CollectedDots);

    /// <summary>現在パワーモード中か</summary>
    public bool IsPowerMode { get; private set; }

    /// <summary>クリア済みか</summary>
    public bool IsCleared { get; private set; }

    /// <summary>ゲームオーバー済みか</summary>
    public bool IsGameOver { get; private set; }

    /// <summary>パワーモード残り時間</summary>
    public float PowerModeRemainingTime => powerModeTimer;

    //==================================================
    // 内部状態
    //==================================================

    /// <summary>パワーモード残り時間タイマー</summary>
    private float powerModeTimer;

    /// <summary>1回目のフルーツ出現が終わったか</summary>
    private bool hasSpawnedFirstFruit;

    /// <summary>2回目のフルーツ出現が終わったか</summary>
    private bool hasSpawnedSecondFruit;

    //==================================================
    // イベント
    //==================================================

    /// <summary>
    /// ドット数更新通知
    /// 第1引数: 取得済み数
    /// 第2引数: 総数
    /// </summary>
    public event Action<int, int> OnDotCountChanged;

    /// <summary>パワーモード開始通知（継続秒数を渡す）</summary>
    public event Action<float> OnPowerModeStarted;

    /// <summary>パワーモード終了通知</summary>
    public event Action OnPowerModeEnded;

    /// <summary>フルーツ出現通知</summary>
    public event Action OnFruitSpawned;

    /// <summary>フルーツ取得通知</summary>
    public event Action OnFruitCollected;

    /// <summary>ゲームクリア通知</summary>
    public event Action OnGameCleared;

    /// <summary>ゲームオーバー通知</summary>
    public event Action OnGameOver;

    //==================================================
    // Unity
    //==================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        UpdatePowerMode();
    }

    //==================================================
    // 初期化
    //==================================================

    /// <summary>
    /// ステージ生成側から総ドット数を設定する
    /// ステージ開始時の状態もここで初期化する
    /// </summary>
    public void SetTotalDots(int totalDots)
    {
        TotalDots = Mathf.Max(0, totalDots);
        CollectedDots = 0;

        IsCleared = false;
        IsGameOver = false;

        IsPowerMode = false;
        powerModeTimer = 0f;

        hasSpawnedFirstFruit = false;
        hasSpawnedSecondFruit = false;

        OnDotCountChanged?.Invoke(CollectedDots, TotalDots);
    }

    //==================================================
    // 取得通知
    //==================================================

    /// <summary>
    /// 通常ドット取得通知
    /// </summary>
    public void NotifyDotCollected()
    {
        if (IsCleared || IsGameOver) return;

        AddCollectedDotCount(1);
        CheckFruitSpawn();
        CheckGameClear();
    }

    /// <summary>
    /// パワークッキー取得通知
    /// パワークッキーもクリア対象なのでドット取得数を進める
    /// </summary>
    public void NotifyPowerCookieCollected()
    {
        if (IsCleared || IsGameOver) return;

        AddCollectedDotCount(1);

        StartPowerMode();

        CheckFruitSpawn();
        CheckGameClear();
    }

    /// <summary>
    /// フルーツ取得通知
    /// フルーツは通常クリア対象のドット数には含めない
    /// </summary>
    public void NotifyFruitCollected()
    {
        if (IsCleared || IsGameOver) return;

        OnFruitCollected?.Invoke();
    }

    //==================================================
    // プレイヤー状態通知
    //==================================================

    /// <summary>
    /// プレイヤー死亡通知
    /// 残機制をまだ入れないなら、いったん即ゲームオーバーでよい
    /// </summary>
    public void NotifyPlayerDead()
    {
        if (IsCleared || IsGameOver) return;

        IsGameOver = true;

        // ゲームオーバー時はパワーモードも終了扱いにする
        if (IsPowerMode)
        {
            IsPowerMode = false;
            powerModeTimer = 0f;
            OnPowerModeEnded?.Invoke();
        }

        OnGameOver?.Invoke();
    }

    //==================================================
    // 内部処理
    //==================================================

    /// <summary>
    /// 取得済みドット数を加算する
    /// </summary>
    private void AddCollectedDotCount(int addCount)
    {
        CollectedDots += addCount;

        // 念のため上限を超えないようにする
        if (CollectedDots > TotalDots)
        {
            CollectedDots = TotalDots;
        }

        OnDotCountChanged?.Invoke(CollectedDots, TotalDots);
    }

    /// <summary>
    /// パワーモード開始
    /// すでに発動中なら残り時間をリセットする
    /// </summary>
    private void StartPowerMode()
    {
        IsPowerMode = true;
        powerModeTimer = powerModeDuration;
        OnPowerModeStarted?.Invoke(powerModeDuration);
    }

    /// <summary>
    /// パワーモード更新
    /// </summary>
    private void UpdatePowerMode()
    {
        if (!IsPowerMode) return;
        if (IsCleared || IsGameOver) return;

        powerModeTimer -= Time.deltaTime;

        if (powerModeTimer <= 0f)
        {
            powerModeTimer = 0f;
            IsPowerMode = false;
            OnPowerModeEnded?.Invoke();
        }
    }

    /// <summary>
    /// フルーツ出現判定
    /// パックマンでは一定タイミングでフルーツが出るため、
    /// 今回は「残りドット数」で出現判定を行う
    /// </summary>
    private void CheckFruitSpawn()
    {
        //if (fruitSpawner == null) return;

        //// 1回目
        //if (!hasSpawnedFirstFruit && RemainingDots <= firstFruitAppearRemainingDots)
        //{
        //    hasSpawnedFirstFruit = true;
        //    fruitSpawner.SpawnFruit();
        //    OnFruitSpawned?.Invoke();
        //    return;
        //}

        //// 2回目
        //if (!hasSpawnedSecondFruit && RemainingDots <= secondFruitAppearRemainingDots)
        //{
        //    hasSpawnedSecondFruit = true;
        //    fruitSpawner.SpawnFruit();
        //    OnFruitSpawned?.Invoke();
        //}
    }

    /// <summary>
    /// 全ドット取得でクリア
    /// </summary>
    private void CheckGameClear()
    {
        if (IsCleared || IsGameOver) return;
        if (TotalDots <= 0) return;

        if (CollectedDots >= TotalDots)
        {
            IsCleared = true;

            // クリア時にパワーモード中なら終了扱いにする
            if (IsPowerMode)
            {
                IsPowerMode = false;
                powerModeTimer = 0f;
                OnPowerModeEnded?.Invoke();
            }

            OnGameCleared?.Invoke();
        }
    }
}