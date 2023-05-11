using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private PlayerInput playerInput;
    [SerializeField]
    private float maxCameraHeight;
    [SerializeField]
    private float minCameraHeight;


    public void OnCameraMove(InputAction.CallbackContext context)
    {
        Debug.Log(context.ReadValue<Vector2>().normalized);
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("Clicked");
        }

    }

    public void OnZoom(InputAction.CallbackContext context)
    {
        Debug.Log(context.ReadValue<Vector2>().normalized);
    }
}
