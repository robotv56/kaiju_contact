using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour
{
    public RectTransform Selector;

    public Button railgunButton;
    public Button minigunButton;
    public Button laserButton;

    public Toggle monsterToggle;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnRailgunButtonPressed()
    {
        Selector.transform.position = railgunButton.transform.position;
    }

    public void OnMiniGunButtonPressed()
    {
        Selector.transform.position = minigunButton.transform.position;
    }

    public void OnLaserButtonPressed()
    {
        Selector.transform.position = laserButton.transform.position;
    }
}
