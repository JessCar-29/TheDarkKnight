using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Over")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public Button reiniciarButton;
    public Button menuButton;

    [Header("Victoria")]
    public GameObject victoryPanel;
    public TextMeshProUGUI victoryCoinsText;
    public TextMeshProUGUI victoryStarsText;
    public Button victoryReiniciarButton;
    public Button victoryMenuButton;

    [Header("Escenas")]
    public string mainMenuSceneName = "MainMenu";

    private bool gameOverActivo = false;
    private bool victoriaActiva = false;

    private int coins = 0;
    private int stars = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (reiniciarButton != null)
            reiniciarButton.onClick.AddListener(ReiniciarEscena);

        if (menuButton != null)
            menuButton.onClick.AddListener(IrAlMenu);

        if (victoryReiniciarButton != null)
            victoryReiniciarButton.onClick.AddListener(ReiniciarEscena);

        if (victoryMenuButton != null)
            victoryMenuButton.onClick.AddListener(IrAlMenu);
    }

    public void AddCoin(int amount = 1)
    {
        coins += amount;
    }

    public void AddStar(int amount = 1)
    {
        stars += amount;
    }

    public void GameOver()
    {
        if (gameOverActivo || victoriaActiva) return;

        gameOverActivo = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (gameOverText != null)
        {
            gameOverText.text = "GAME OVER\n\nR - Reiniciar\nESC - Menú Principal";
        }
    }

    public void Victory()
    {
        if (victoriaActiva || gameOverActivo) return;

        victoriaActiva = true;

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        if (victoryCoinsText != null)
            victoryCoinsText.text = "Monedas: " + coins;

        if (victoryStarsText != null)
            victoryStarsText.text = "Estrellas: " + stars;
    }

    void ReiniciarEscena()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void IrAlMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}