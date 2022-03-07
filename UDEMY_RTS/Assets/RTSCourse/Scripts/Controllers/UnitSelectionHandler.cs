using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using RTSCourse.Units;
using System;
using RTSCourse.Networking;
using Mirror;

namespace RTSCourse.Controllers
{

    public class UnitSelectionHandler : MonoBehaviour
    {
        private Camera mainCamera;
        public List<Unit> SelectedUnits{get;} = new List<Unit>();
        [SerializeField] private LayerMask targetLayerMask = new LayerMask();
        [SerializeField] private RectTransform unitSelectionArea = null;

        private RTSPlayer player;
        private Vector2 dragStartPosition;

        private void Start() 
        {
            mainCamera = Camera.main;
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
            GameOverHandler.ClientOnGameOver += CleintHandleGameOver;
            Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawn;
        }

        private void OnDestroy() 
        {
            Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawn;
            GameOverHandler.ClientOnGameOver -= CleintHandleGameOver;
        }

        private void AuthorityHandleUnitDespawn(Unit unit)
        {
            SelectedUnits.Remove(unit);
        }

        private void Update() 
        {
            if(Mouse.current.leftButton.wasPressedThisFrame)
            {
                StartSelectionArea();
            }
            else if(Mouse.current.leftButton.wasReleasedThisFrame)
            {
                ClearSelectionArea();
            }else if(Mouse.current.leftButton.isPressed)
            {
                UpdateSelectionArea();
            }
        }

        private void StartSelectionArea()
        {
            if(!Keyboard.current.leftShiftKey.isPressed)
            {
                foreach (Unit selectedUnit in SelectedUnits)
                {
                    selectedUnit.Deselect();
                }
            }
            SelectedUnits.Clear();

            unitSelectionArea.gameObject.SetActive(true);

            dragStartPosition = Mouse.current.position.ReadValue();

            UpdateSelectionArea();
        }

        private void UpdateSelectionArea()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            float areaWidth = mousePosition.x - dragStartPosition.x;
            float areaHeight = mousePosition.y - dragStartPosition.y;

            unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
            unitSelectionArea.anchoredPosition = dragStartPosition + new Vector2(areaWidth/2, areaHeight/2);

        }

        private void ClearSelectionArea()
        {

            unitSelectionArea.gameObject.SetActive(false);

            if(unitSelectionArea.sizeDelta.magnitude == 0) // single click
            {
                Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

                if(!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, targetLayerMask)) {return;}
                
                if(!hit.collider.TryGetComponent<Unit>(out Unit unit)) {return;}

                if(!unit.hasAuthority) {return;}

                if(SelectedUnits.Contains(unit)) return;

                SelectedUnits.Add(unit);

                foreach(Unit selectedUnit in SelectedUnits)
                {
                    selectedUnit.Select();
                }

                return;
            }

            Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta/2);
            Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta/2);

            foreach(Unit unit in player.GetMyUnits())
            {
                if(SelectedUnits.Contains(unit)) continue;


                Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);
                if(screenPosition.x > min.x && 
                   screenPosition.x < max.x &&
                   screenPosition.y > min.y && 
                   screenPosition.y < max.y)
                {
                    
                    SelectedUnits.Add(unit);
                    unit.Select();
                }
            }
        }

        private void CleintHandleGameOver(string winnerName)
        {
            enabled = false;
        }
    }
}