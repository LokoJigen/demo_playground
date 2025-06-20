using UnityEngine;

/// <summary>
/// Utility class to make a game object persist across scenes.
/// </summary>
public class DontDestroyOnLoad : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
