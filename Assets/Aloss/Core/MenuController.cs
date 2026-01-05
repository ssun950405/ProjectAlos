using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void OnClickStart()
    {
        SceneManager.LoadScene("BattleScene");
    }
}

