using UnityEngine;

/// <summary>
/// ゲームクリアUIを表示するクラス
/// </summary>
public class GameClearUI : MonoBehaviour
{
    [SerializeField] private GameObject clearPanel;

    private void Awake()
    {
        // 初期状態では非表示
        if (clearPanel != null)
        {
            clearPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        // GameSystemのイベントを購読
        if (GameSystem.Instance != null)
        {
            GameSystem.Instance.OnGameCleared += HandleGameCleared;
        }
    }

    private void OnDisable()
    {
        if (GameSystem.Instance != null)
        {
            GameSystem.Instance.OnGameCleared -= HandleGameCleared;
        }
    }

    /// <summary>
    /// ゲームクリア時の処理
    /// </summary>
    private void HandleGameCleared()
    {
        Debug.Log("ゲームクリアUI表示");

        if (clearPanel != null)
        {
            clearPanel.SetActive(true);
        }

        // 必要なら時間停止
        Time.timeScale = 0f;
    }
}