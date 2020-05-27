using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;

public class PlayerKaijuController : MonoBehaviour
{

    [SerializeField] private PlayerInput playerInput = PlayerInput.KEYBOARD;
    private KaijuCore core;
    private GameObject cameraPivot;
    private Camera mainCamera;
    private Vector2 cameraAim;
    [SerializeField] private float cameraMaxY_up = 25f;
    [SerializeField] private float cameraMaxY_down = 45f;
    private int playerMask = (1 << 8) + (1 << 10) + (1 << 11);
    

    private void Start()
    {
        core = GetComponent<KaijuCore>();
        cameraPivot = transform.Find("Camera Pivot").gameObject;
        mainCamera = cameraPivot.transform.Find("Kaiju Camera").GetComponent<Camera>();
    }
    
    private void Update()
    {
        HideCursor(true);
        if (playerInput == PlayerInput.KEYBOARD)
        {
            int vertical = 0;
            if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
            {
                vertical = 1;
            }
            else if (Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W))
            {
                vertical = -1;
            }
            int horizontal = 0;
            if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A))
            {
                horizontal = 1;
            }
            else if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
            {
                horizontal = -1;
            }
            core.SetDesiredMovement(NormalizeKeyboardInput(horizontal, vertical));

            cameraAim = new Vector2(cameraAim.x + Input.GetAxis("Mouse X"), Mathf.Clamp(cameraAim.y - Input.GetAxis("Mouse Y"), -cameraMaxY_up, cameraMaxY_down));
            cameraPivot.transform.rotation = Quaternion.Euler(cameraAim.y, cameraAim.x, 0f);
            core.SetDesiredRotation(cameraPivot.transform.eulerAngles.y);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                core.SetSubmerge();
            }

            if (Input.GetMouseButtonDown(0))
            {
                core.TriggerSlash();
            }

            if (Input.GetMouseButtonDown(1) || core.IcebeamFiring())
            {
                Ray aimRay = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                Physics.Raycast(aimRay, out RaycastHit aimHit, core.GetIcebeamRange(), (1 << 10));
                core.UpdateIcebeam(aimHit.point);
                if (Input.GetMouseButtonDown(1))
                {
                    core.TriggerIcebeam();
                }
            }
        }
    }

    private Vector3 NormalizeKeyboardInput(float x, float y)
    {
        Vector3 dir = new Vector3(x, 0f, y);
        if (dir.magnitude > 1f)
        {
            dir = dir.normalized;
        }
        return dir;
    }

    private void HideCursor(bool hidden)
    {
        if (hidden)
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        } else
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
