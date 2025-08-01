using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using MCRGame.Game;
using MCRGame.Common;


namespace MCRGame.Game
{
    public class RightClickManager : MonoBehaviour
    {

        private InputAction _rightClickAction;

        void OnEnable()
        {
            _rightClickAction = new InputAction(
                name: "RightClick",
                type: InputActionType.Button,
                binding: "<Mouse>/rightButton"
            );
            _rightClickAction.performed += OnRightClickPerformed;
            _rightClickAction.Enable();
        }

        void OnDisable()
        {
            _rightClickAction.performed -= OnRightClickPerformed;
            _rightClickAction.Disable();
            _rightClickAction.Dispose();
        }

        private void OnRightClickPerformed(InputAction.CallbackContext ctx)
        {
            if (GameManager.Instance == null)
            {
                return;
            }
            if (GameManager.Instance.IsRightClickTsumogiri == false)
            {
                return;
            }
            if (GameManager.Instance.isActionUIActive)
            {
                if (GameManager.Instance.isAfterTsumoAction)
                    GameManager.Instance.OnSkipButtonClickedAfterTsumo();
                else
                    GameManager.Instance.OnSkipButtonClicked();
            }
            else
            {
                if (GameManager.Instance.isAfterTsumoAction && GameManager.Instance.CanClick)
                {
                    GameManager.Instance.isAfterTsumoAction = false;
                    GameManager.Instance.CanClick = false;
                    StartCoroutine(GameManager.Instance.GameHandManager.RunExclusive(
                        GameManager.Instance.GameHandManager.RequestDiscardRightmostTileSequence()));
                }
            }
        }
    }
}