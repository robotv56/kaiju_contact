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
    public Image[] playerReadyStatusIndicators;

    public GameObject[] weapons;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i != 6; i++)
        {
            if (GlobalVars.players[i])
            {
                playerReadyStatusIndicators[i].color = Color.green;
            }
            else
            {
                playerReadyStatusIndicators[i].color = Color.red;
            }
        }
    }

    public void OnRailgunButtonPressed()
    {
        Selector.transform.position = railgunButton.transform.position;
        weapons[0].SetActive(true);
        weapons[1].SetActive(false);
        weapons[2].SetActive(false);
    }

    public void OnMiniGunButtonPressed()
    {
        Selector.transform.position = minigunButton.transform.position;

        weapons[0].SetActive(false);
        weapons[1].SetActive(false);
        weapons[2].SetActive(true);
    }

    public void OnLaserButtonPressed()
    {
        Selector.transform.position = laserButton.transform.position;

        weapons[0].SetActive(false);
        weapons[1].SetActive(true);
        weapons[2].SetActive(false);
    }

    public void OnReadyButtonPressed()
    {
        GlobalVars.players[0] = !GlobalVars.players[0];
    }
}
