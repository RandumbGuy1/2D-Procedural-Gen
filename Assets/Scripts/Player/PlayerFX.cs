using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFX : MonoBehaviour {
    [SerializeField] private ParticleSystem dust;
    [SerializeField] private PlayerRef player;

    void Awake() {
        player.PlayerMovement.OnPlayerSmoke += () => dust.Play();
    }
}
