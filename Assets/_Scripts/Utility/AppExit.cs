using UnityEngine;

/// <summary>
/// This class allows the user to quit the application by pressing the escape key.
/// </summary>
public class AppExit : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
