using UnityEngine;
using UnityEngine.UI;   

public class testFieldButtonController : MonoBehaviour
{
    [Header("Button List: ")]
    public Button collisionDetectionButton;

    void Start()
    {
        collisionDetectionButton.onClick.AddListener(OnCollisionDetectionButtonClicked);
    }

    void OnCollisionDetectionButtonClicked()
    {
        sceneManager.Instance.LoadScene(sceneManager.Scene.Collision);
    }

}
