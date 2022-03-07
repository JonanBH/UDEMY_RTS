using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace RTSCourse.Combat
{
    public class Projectile : NetworkBehaviour
    {
        [SerializeField]
        private Rigidbody rb = null;
        [SerializeField]
        private float launchForce = 10f;
        [SerializeField]
        private float destroyAfterSeconds = 5f;
        [SerializeField]
        private int damageToDeal = 20;

        private void Start() 
        {
            rb.velocity = transform.forward * launchForce;
            Invoke(nameof(DestroySelf), destroyAfterSeconds);
        }

        [Server]
        private void OnTriggerEnter(Collider other) 
        {
            if(other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))
            {
                if(networkIdentity.connectionToClient == connectionToClient){ return;}
            }

            if(other.TryGetComponent<Health>(out Health health))
            {
                health.DealDamage(damageToDeal);
            }

            DestroySelf();
        }

        [Server]
        private void DestroySelf()
        {
            NetworkServer.Destroy(gameObject);
        }

    }
}
