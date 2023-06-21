using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFX : MonoBehaviour {
    [SerializeField] private ParticleSystem dust;
    [SerializeField] private PlayerRef player;

    private bool facingRight = true;
    void Awake() {
        player.PlayerMovement.OnPlayerSmoke += () => dust.Play();
    }

    void FixedUpdate()
    {
        Vector2 mousePos = player.PlayerCam.ScreenToWorldPoint(Input.mousePosition);
        float fromTo = mousePos.x - transform.position.x;

        if ((fromTo > 0 && !facingRight) || (fromTo < 0 && facingRight))
        {
            transform.Rotate(0, 180, 0);
            facingRight = !facingRight;
        }
    }
}
