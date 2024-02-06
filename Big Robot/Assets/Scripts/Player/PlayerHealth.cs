using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 8;
    public int health = 8;

    [SerializeField] int invincibilityFrames = 240;
    public int invincibilityTimer = 0;

    public bool infiniteHealth = false;

    private void FixedUpdate()
    {
        if (invincibilityTimer > 0)
        {
            invincibilityTimer--;
            //Player flickers off on odd frames and on on even frames
            foreach(Renderer renderer in gameObject.GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = ((invincibilityTimer % 2) == 0);
            }
        }
    }

    public int DealDamage(int damage)
    {
        if (!infiniteHealth && invincibilityTimer <= 0)
        {
            health -= damage;
            if (health <= 0)
            {
                Die();
                return -1;
            }
            //If damage isn't lethal, add invincibility frames.
            invincibilityTimer = invincibilityFrames;
            return health;
        }
        return -2;
    }

    public void Die()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
