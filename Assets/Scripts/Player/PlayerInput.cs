using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour {
    public delegate void ReceieveVector2Input(Vector2 input);
    public delegate void ReceieveBoolInput(bool input);
    public delegate void ReceieveIntInput(int input);
    public delegate void ReceieveFloatInput(float input);

    public event ReceieveVector2Input OnMoveInput;
    public event ReceieveVector2Input OnMouseInput;

    public event ReceieveBoolInput OnJumpInput;
    public event ReceieveBoolInput OnJumpHoldInput;
    public event ReceieveBoolInput OnSwimSinkInput;
    public event ReceieveBoolInput OnCrouchInput;
    public event ReceieveBoolInput OnInteractInput;
    public event ReceieveBoolInput OnDialogueInput;
    public event ReceieveIntInput OnDialogueOptionsInput;

    public event ReceieveIntInput OnClubSelectInput;
    public event ReceieveBoolInput OnClubDropInput;

    public event ReceieveBoolInput OnPauseToggle;

    public event ReceieveIntInput OnMouseButtonDownInput;
    public event ReceieveIntInput OnMouseButtonInput;

    [Header("Movement Keybinds")]
    [SerializeField] private KeyCode jumpKey;
    [SerializeField] private KeyCode crouchKey;
    [SerializeField] private KeyCode sinkSwimKey;

    [Header("Interact Keybinds")]
    [SerializeField] private KeyCode interactKey;
    [SerializeField] private KeyCode dialogueKey;
    [SerializeField] private List<KeyCode> dialogueOptionsKeys = new List<KeyCode>();

    [Header("Item Keybinds")]
    [SerializeField] private KeyCode itemDropKey;
    [SerializeField] private List<KeyCode> itemSwitchKeys = new List<KeyCode>();

    [Header("UI Keybinds")]
    [SerializeField] private KeyCode pauseMenuKey;

    public KeyCode JumpKey => jumpKey;
    public KeyCode CrouchKey => crouchKey;
    public KeyCode SinkSwimKey => sinkSwimKey;
    public KeyCode InteractKey => interactKey;
    public KeyCode PauseMenuKey => pauseMenuKey;
    public KeyCode DialogueKey => dialogueKey;
    public List<KeyCode> DialogueOptionsKeys => dialogueOptionsKeys;

    void Update() {
        OnPauseToggle?.Invoke(Input.GetKeyDown(pauseMenuKey));

        OnMoveInput?.Invoke(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")));
        OnMouseInput?.Invoke(new Vector2(Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X")));

        OnJumpInput?.Invoke(Input.GetKeyDown(jumpKey));
        OnJumpHoldInput?.Invoke(Input.GetKey(jumpKey));
        OnCrouchInput?.Invoke(Input.GetKey(crouchKey));

        OnMouseButtonDownInput?.Invoke(MouseButtonDown());
        OnMouseButtonInput?.Invoke(MouseButton());

        OnSwimSinkInput?.Invoke(Input.GetKey(sinkSwimKey));
        OnInteractInput?.Invoke(Input.GetKeyDown(interactKey));
        OnClubDropInput?.Invoke(Input.GetKeyDown(itemDropKey));
        OnClubSelectInput?.Invoke(IterateKeyBinds(itemSwitchKeys));
        OnDialogueInput?.Invoke(Input.GetKeyDown(dialogueKey));
        OnDialogueOptionsInput?.Invoke(IterateKeyBinds(dialogueOptionsKeys));
    }

    int IterateKeyBinds(List<KeyCode> keys) {
        foreach (KeyCode key in keys) if (Input.GetKeyDown(key)) return keys.IndexOf(key);
        return -1;
    }

    int MouseButton() {
        if (Input.GetMouseButton(0)) return 0;
        if (Input.GetMouseButton(1)) return 1;
        if (Input.GetMouseButton(2)) return 2;
        return -1;
    }

    int MouseButtonDown() {
        if (Input.GetMouseButtonDown(0)) return 0;
        if (Input.GetMouseButtonDown(1)) return 1;
        if (Input.GetMouseButtonDown(2)) return 2;
        return -1;
    }
}
