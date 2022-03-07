using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using RTSCourse.Units;
using RTSCourse.Combat;

namespace RTSCourse.Controllers
{
    public class UnitCommandGiver : MonoBehaviour
    {
        [SerializeField] private UnitSelectionHandler unitSelectionHandler = null;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask targetLayerMask;

        // Start is called before the first frame update
        void Start()
        {
            mainCamera = Camera.main;    
            GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
        }

        private void OnDestroy() {
            GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
        }

        // Update is called once per frame
        void Update()
        {
            if(Mouse.current.rightButton.wasReleasedThisFrame)
            {
                Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                if(!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, targetLayerMask)){ return; }
                
                if(hit.collider.TryGetComponent<Targetable>(out Targetable target))
                {
                    if(target.hasAuthority)
                    {
                        TryMove(hit.point);
                    }
                    else
                    {
                        TryTarget(target);
                        return;
                    }
                }

                TryMove(hit.point);
            }
        }

        private void TryMove(Vector3 position)
        {
            foreach(Unit unit in unitSelectionHandler.SelectedUnits)
            {
                unit.GetUnitMovement().CmdMove(position);
            }
        }

        private void TryTarget(Targetable target)
        {
            foreach(Unit unit in unitSelectionHandler.SelectedUnits)
            {
                unit.GetTargeter().CmdSetTarget(target.gameObject);
            }
        }

        private void ClientHandleGameOver(string winner)
        {
            enabled = false;
        }
    }
}