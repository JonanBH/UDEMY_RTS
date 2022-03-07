using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace RTSCourse.Combat
{
    public class Targetable : NetworkBehaviour
    {
        [SerializeField] private Transform aimAtPoint = null;

        public Transform GetAinAtPoint()
        {
            return aimAtPoint;
        }

    }
}