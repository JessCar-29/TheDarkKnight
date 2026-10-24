using UnityEngine;
using UnityEngine.SceneManagement;
public class ManeMenu : MonoBehaviour
{
   public GameObject optionsMenu;
   public GameObject mainMenu;

   public void OpenOptionsPanel(){
    mainMenu.SetActive(false);
    optionsMenu.SetActive(true);
   }

   public void OpenMainPanel(){
    mainMenu.SetActive(false);
    optionsMenu.SetActive(true);
   }

   public void QuitGame(){
    Application.Quit();
   }

   public void PlayGame(){
    SceneManager.LoadScene("Level1");
   }
}
