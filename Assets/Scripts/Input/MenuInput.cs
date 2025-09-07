using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuInput : MonoBehaviour
{
    public static MenuInput Instance { get; private set; }

    public Action OnMouseClick;
    public Action OnEscepeClick;
    public Action OnEnterClick;

    private LobbyInputAction actions;

    private void Awake()
    {
        if (Instance == null )
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        actions = new LobbyInputAction();
        actions.Enable();
        actions.Menu.LeftClick.started += LeftClick_Started;
        actions.Menu.RightClick.started += RightClick_Started;
        actions.Menu.Esc.started += Esc_Started;
        actions.Menu.Enter.started += Enter_Started;
    }

    private void LeftClick_Started(InputAction.CallbackContext obj) => OnMouseClick?.Invoke();

    private void RightClick_Started(InputAction.CallbackContext obj) => OnMouseClick?.Invoke();

    private void Esc_Started(InputAction.CallbackContext obj) => OnEscepeClick?.Invoke();

    private void Enter_Started(InputAction.CallbackContext obj) => OnEnterClick?.Invoke();

    private void OnDestroy()
    {
        if (Instance ==  this)
        {
            Instance = null;
        }

        actions.Dispose();
        actions.Disable();

        actions.Menu.LeftClick.started -= LeftClick_Started;
        actions.Menu.RightClick.started -= RightClick_Started;
        actions.Menu.Esc.started -= Esc_Started;
        actions.Menu.Enter.started -= Enter_Started;
    }
}
