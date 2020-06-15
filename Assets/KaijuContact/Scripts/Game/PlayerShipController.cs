using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XboxCtrlrInput;
using static GlobalVars;

public class PlayerShipController : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput = PlayerInput.KEYBOARD;
    private ShipCore core;
    private ShipTurret turret;
    private GameObject cameraPivot;
    private Camera mainCamera;
    private Vector2 cameraAim;
    [SerializeField] private float cameraMaxY_up = 15f;
    [SerializeField] private float cameraMaxY_down = 30f;
    private int playerMask = (1 << 9) + (1 << 10) + (1 << 11);
    private Image crosshair;

    public bool isLocal;
    public bool isAlive;

    private GameObject hud;

    private void Start()
    {
        core = GetComponent<ShipCore>();
        turret = transform.Find("Ship Pivot").Find("Ship Body").Find("Turret Base").Find("Turret Horizontal").GetComponent<ShipTurret>();
        cameraPivot = transform.Find("Camera Pivot").gameObject;
        mainCamera = cameraPivot.transform.Find("Ship Camera").GetComponent<Camera>();
        GlobalVars.globalGameObjects.TryGetValue("hud", out hud);
        crosshair = hud.transform.Find("Crosshair").GetComponent<Image>();
    }
    
    private void Update()
    {
        if (isLocal)
        {
            HideCursor(gameMaster.IsGameInProgress());
            if (playerInput == PlayerInput.KEYBOARD)
            {
                if (isAlive)
                {
                    if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
                    {
                        core.SetThrottle(1f);
                    }
                    else if (Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W))
                    {
                        core.SetThrottle(-1f);
                    }
                    else
                    {
                        core.SetThrottle(0f);
                    }
                    if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
                    {
                        core.SetRudder(-1f);
                    }
                    else if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A))
                    {
                        core.SetRudder(1f);
                    }
                    else
                    {
                        core.SetRudder(0f);
                    }
                }
                cameraAim = new Vector2(cameraAim.x + Input.GetAxis("Mouse X"), Mathf.Clamp(cameraAim.y - Input.GetAxis("Mouse Y"), -cameraMaxY_up, cameraMaxY_down));
                cameraPivot.transform.rotation = Quaternion.Euler(cameraAim.y, cameraAim.x, 0f);
                if (isAlive)
                {
                    if (Input.GetMouseButton(0))
                    {
                        turret.FireCannon(playerMask);
                    }
                }
            }

            if (isAlive)
            {
                Ray aimRay = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                if (Physics.Raycast(aimRay, out RaycastHit aimHit, 50000f, playerMask + (1 << 14)))
                {
                    //Debug.Log("Raycast hit: " + aimHit.transform.name);
                    if (aimHit.transform.gameObject.layer == 9 || aimHit.transform.gameObject.layer == 13)
                    {
                        KaijuCore enemy = aimHit.transform.root.Find("Kaiju").GetComponent<KaijuCore>();
                        //turret.UpdateAim(enemy.transform.position + Quaternion.Euler(0f, enemy.transform.Find("Ship Pivot").eulerAngles.y, 0f) * new Vector3(0f, 0.25f, -0.65f), enemy.GetMovement());//enemy.GetVelocity());
                        turret.UpdateAim(aimHit.point, enemy.GetMovement());
                        //Vector3 point = enemy.transform.position + Quaternion.Euler(0f, enemy.transform.Find("Ship Pivot").eulerAngles.y, 0f) * new Vector3(0f, 0.25f, -0.65f);
                        Vector3 point = aimHit.point;
                        Debug.DrawLine(point, turret.transform.position, Color.red);
                        crosshair.color = new Color(1f, 0.25f, 0f, 0.65f);
                    }
                    else
                    {
                        turret.UpdateAim(aimHit.point, Vector3.zero);
                        crosshair.color = new Color(1f, 1f, 1f, 0.65f);
                    }
                }
                else
                {
                    Vector3 targetPointTemp = mainCamera.gameObject.transform.position + aimRay.direction * 50000f;
                    if (targetPointTemp.y < 0f)
                    {
                        targetPointTemp = new Vector3(targetPointTemp.x, 0f, targetPointTemp.z);
                    }
                    turret.UpdateAim(targetPointTemp, Vector3.zero);
                    crosshair.color = new Color(0.75f, 0.75f, 0.75f, 0.50f);
                }
            }
        }
        
        //scoreUI.text = "Score: " + score;

        /*if (core.GetHealth() == 0f) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }*/
    }

    /*public void IncreaseScore() {
        score++;
    }*/

    private void HideCursor(bool hidden)
    {
        if (hidden)
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
        else
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }
        if (Cursor.visible == hidden)
        {
            Cursor.visible = !hidden;
        }
    }

    private XboxController GetXbox()
    {
        switch (playerInput)
        {
        case PlayerInput.XBOX_P1:
            return XboxController.First;
        case PlayerInput.XBOX_P2:
            return XboxController.Second;
        case PlayerInput.XBOX_P3:
            return XboxController.Third;
        case PlayerInput.XBOX_P4:
            return XboxController.Fourth;
        default:
            return XboxController.Any;
        }
    }
}
