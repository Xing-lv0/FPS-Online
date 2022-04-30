using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthControl : MonoBehaviour
{
    public float HP = 100;
    public Slider HP_slider;
    public float maxHP = 100;

    public GameObject deathParticle; //À¿Õˆ¡£◊”Ãÿ–ß£ª

    // Start is called before the first frame update
    void Start()
    {
        HP_slider = GameObject.FindWithTag("Player_HP_Slider").GetComponent<Slider>();
        deathParticle = GameObject.FindWithTag("BotExplosionParticle");

        HP = maxHP;
        if (HP_slider)
        {
            HP_slider.value = HP / maxHP * HP_slider.maxValue;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Damage(float damage)
    {
        if (HP > 0)
        {
            HP -= damage;
            if (HP_slider)
            {
                HP_slider.value = HP / maxHP * HP_slider.maxValue;
            }
        }
        if (HP <= 0)
        {
            //À¿Õˆ£∫
            DeathParticleExplode();
            Destroy(this.gameObject);
        }
    }

    private void DeathParticleExplode()
    {
        if (deathParticle)
        {
            GameObject newParticle = Instantiate(deathParticle, this.transform.position, deathParticle.transform.rotation);

            Destroy(newParticle, 3);
        }
    }
}
