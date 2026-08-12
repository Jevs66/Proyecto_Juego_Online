using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MatchRestartUI : MonoBehaviour
{
    [Header("Restart")]

    [SerializeField] private Button restartButton;

    [Header("Pause")]

    [SerializeField] private GameObject pausePanel;

    private bool isPlayerOne;
    private bool isPaused;

    private void Start()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartMatch);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    private void Update()
    {
        UpdatePlayerPermission();

        if (!isPlayerOne)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    private void UpdatePlayerPermission()
    {
        if (MatchManager.Instance == null || MatchManager.Instance.Runner == null)
        {
            return;
        }

        NetworkObject playerObject = MatchManager.Instance.Runner.GetPlayerObject(MatchManager.Instance.Runner.LocalPlayer);

        if (playerObject == null)
        {
            return;
        }

        PlayerInteraction playerInteraction = playerObject.GetComponent<PlayerInteraction>();

        if (playerInteraction == null)
        {
            return;
        }

        isPlayerOne = playerInteraction.PlayerIndex == 0;

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(isPlayerOne);
        }

        if (pausePanel != null && !isPlayerOne)
        {
            pausePanel.SetActive(false);
            isPaused = false;
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;

        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }

        if (isPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void RestartMatch()
    {
        if (!isPlayerOne)
        {
            return;
        }

        if (MatchManager.Instance == null)
        {
            return;
        }

        MatchManager.Instance.RequestRestart();

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
