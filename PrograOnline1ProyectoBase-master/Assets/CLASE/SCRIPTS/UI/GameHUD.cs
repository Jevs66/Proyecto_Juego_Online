using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [Header("Health")]

    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;

    [Header("Ammo")]

    [SerializeField] private TMP_Text ammoText;

    [Header("Bomb")]

    [SerializeField] private TMP_Text bombStatusText;

    [Header("Interaction")]

    [SerializeField] private TMP_Text interactionText;

    [Header("Match Result")]

    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;

    private NetworkObject localPlayer;
    private Health health;
    private WeaponHandler weaponHandler;
    private PlayerInteraction playerInteraction;

    private void Start()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        if (bombStatusText != null)
        {
            bombStatusText.gameObject.SetActive(false);
        }

        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        FindLocalPlayer();

        if (localPlayer == null)
        {
            return;
        }

        UpdateHealth();
        UpdateAmmo();
        UpdateBomb();
        UpdateInteraction();
        UpdateMatchResult();
    }

    private void FindLocalPlayer()
    {
        if (localPlayer != null)
        {
            return;
        }

        if (MatchManager.Instance == null || MatchManager.Instance.Runner == null)
        {
            return;
        }

        localPlayer = MatchManager.Instance.Runner.GetPlayerObject(MatchManager.Instance.Runner.LocalPlayer);

        if (localPlayer == null)
        {
            return;
        }

        health = localPlayer.GetComponent<Health>();
        weaponHandler = localPlayer.GetComponent<WeaponHandler>();
        playerInteraction = localPlayer.GetComponent<PlayerInteraction>();
    }

    private void UpdateHealth()
    {
        if (health == null)
        {
            return;
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = health.MaxHealth;
            healthSlider.value = health.ActualHealth;
        }

        if (healthText != null)
        {
            healthText.text = "VIDA: " + health.ActualHealth + " / " + health.MaxHealth;
        }
    }

    private void UpdateAmmo()
    {
        if (weaponHandler == null || weaponHandler.CurrentWeapon == null)
        {
            return;
        }

        Weapon weapon = weaponHandler.CurrentWeapon;

        if (ammoText != null)
        {
            ammoText.text = "MUNICIÓN: " + weapon.actualAmmo + " / " + weapon.maxAmmoCapacity;
        }
    }

    private void UpdateBomb()
    {
        if (bombStatusText == null || BombController.Instance == null)
        {
            return;
        }

        BombController bomb = BombController.Instance;

        if (bomb.CurrentState == BombState.Carried && bomb.Carrier == MatchManager.Instance.Runner.LocalPlayer)
        {
            bombStatusText.text = "TIENES LA BOMBA";
            bombStatusText.gameObject.SetActive(true);
            return;
        }

        if (bomb.CurrentState == BombState.Planted)
        {
            bombStatusText.text = "BOMBA PLANTADA";
            bombStatusText.gameObject.SetActive(true);
            return;
        }

        bombStatusText.gameObject.SetActive(false);
    }

    private void UpdateInteraction()
    {
        if (interactionText == null || playerInteraction == null || BombController.Instance == null)
        {
            return;
        }

        BombZone zone = playerInteraction.CurrentZone;

        if (zone == null)
        {
            interactionText.gameObject.SetActive(false);
            return;
        }

        BombController bomb = BombController.Instance;

        bool isOwner = playerInteraction.PlayerIndex == zone.OwnerPlayerIndex;

        if (bomb.CurrentState == BombState.Carried && bomb.Carrier == MatchManager.Instance.Runner.LocalPlayer && !isOwner)
        {
            interactionText.text = "MANTENER E PARA PLANTAR";
            interactionText.gameObject.SetActive(true);
            return;
        }

        if (bomb.CurrentState == BombState.Planted && bomb.PlantedZoneIndex == zone.ZoneIndex && isOwner)
        {
            interactionText.text = "MANTENER E PARA DESACTIVAR";
            interactionText.gameObject.SetActive(true);
            return;
        }

        interactionText.gameObject.SetActive(false);
    }

    private void UpdateMatchResult()
    {
        if (resultPanel == null || resultText == null)
        {
            return;
        }

        if (MatchManager.Instance == null ||
            MatchManager.Instance.Runner == null)
        {
            return;
        }

        if (!MatchManager.Instance.MatchFinished)
        {
            resultPanel.SetActive(false);
            return;
        }

        PlayerRef localPlayer = MatchManager.Instance.Runner.LocalPlayer;

        resultPanel.SetActive(true);

        if (MatchManager.Instance.Winner == localPlayer)
        {
            resultText.text = "VICTORIA";
        }
        else
        {
            resultText.text = "DERROTA";
        }
    }
}
