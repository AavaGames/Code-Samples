using Assets.App.Scripts.Matchmaking;
using FishNet.Example.ColliderRollbacks;
using UnityEngine;

namespace Assets.App.Scripts.Players
{
    public class PlayerController : Controller
    {
        public CursorController CursorController { get; private set; }

        private new void Awake()
        {
            base.Awake();

            Input = GetComponent<PlayerInputController>();
        }

        private void OnDisable()
        {
            if (IsOwner)
            {
                CursorController.CaptureMode = CursorController.CursorActiveCaptureState.Menu;
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (IsOwner)
            {
                SetCamera();
                CursorController.CaptureMode = CursorController.CursorActiveCaptureState.Menu;
            }
        }

        private void SetCamera()
        {
            var gameSession = FindObjectOfType<GameSession>();
            if (gameSession == null)
            {
                Debug.LogWarning("gameSession is null");
                return; // we're probably not being used inside the new "GameSession" controlled scenese...
            }

            Assets.App.Scripts.Events.UIEvents.SetMenuCameraActive?.Invoke(false);
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();

            gameObject.name = $"Player ({OwnerId})";
        }
    }
}