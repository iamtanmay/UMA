using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    public GameObject explosionFx;
    public float explosionForce = 30f;
    public float explosionRadius = 3f;
    public float explosionDamage = 70f;
    public GameObject afterBurner;
    private bool hasExploded = false;
    // Use this for initialization
    void Start()
    {
    }


    void OnCollisionEnter(Collision other)
    {
        if (!hasExploded)
        {
            hasExploded = true;
            Instantiate(explosionFx, transform.position, Quaternion.identity);
            if (GetComponent<AudioSource>())
                GetComponent<AudioSource>().Stop(); // stop trail sound

            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider col in colliders)
            {
                if (!col.GetComponent<Rigidbody>()) continue;
                //if (col.tag == "Enemy")
                //{
                //    //float distFromExplosion = Vector3.Distance(transform.position, col.transform.position);
                //    //col.GetComponent<Enemy>().health -= explosionDamage / (distFromExplosion < .03 ? 1 : distFromExplosion);  // no npc enemy at the moment

                //}
                col.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRadius, 1, ForceMode.Impulse);
            }
        }
    }

    void OnCollisionStay(Collision other)
    {
        if (hasExploded)
            Destroy(gameObject);

    }


}
