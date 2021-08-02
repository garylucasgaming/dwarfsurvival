using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSI2Showcase_Target : MonoBehaviour
{
    public int health = 10;
    public Transform particle;
    public Transform creator;
    void Damage(int damage)
    {
        health -= damage;
        
        if (health <= 0) {
            particle.SetParent(null);
            particle.rotation = Quaternion.identity;
            particle.gameObject.SetActive(true);
            creator.SendMessage("Dead");
            Destroy(gameObject); 
        }
    }
}
