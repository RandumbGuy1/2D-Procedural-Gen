using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public interface IInteractable {
    PlayerRef Player { get; set; }
    GameObject GameObject { get; }
    string GetDescription(PlayerRef player);

    void OnInteract(PlayerRef player);
    void OnHover(PlayerRef player);
}

[System.Serializable]
public class HoverEffects {
    [SerializeField] private TextMeshPro text;
    public int HoverFrames { get; set; } = 0;

    private int lastHoverFrames = 0;
    public void UpdateHover() {
        text.transform.rotation = Quaternion.identity;

        if (HoverFrames == 0) return;

        if (HoverFrames == lastHoverFrames) {
            text.gameObject.SetActive(false);
            HoverFrames = 0;
            lastHoverFrames = 0;
            return;
        }

        text.gameObject.SetActive(true);
        lastHoverFrames = HoverFrames;
    }
}
