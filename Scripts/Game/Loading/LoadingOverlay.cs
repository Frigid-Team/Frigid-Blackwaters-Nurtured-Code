using UnityEngine;
using System;

using FrigidBlackwaters.Core;

namespace FrigidBlackwaters.Game
{
    public class LoadingOverlay : FrigidMonoBehaviour
    {
        private static LoadingOverlay instance;

        [SerializeField]
        private CanvasGroup overlayCanvasGroup;
        [SerializeField]
        private FloatSerializedReference fadeDuration;

        private FrigidCoroutine loadingScreenRoutine;
        private CountingSemaphore isLoading;
        private Action onScreenFullyShown;

        public static void RequestLoad(Action onReadyForLoad)
        {
            if (instance.IsScreenFullyShown)
            {
                onReadyForLoad?.Invoke();
            }
            else
            {
                instance.onScreenFullyShown += onReadyForLoad;
            }
            instance.isLoading.Request();
        }

        public static void ReleaseLoad()
        {
            instance.isLoading.Release();
        }

        protected override void Awake()
        {
            base.Awake();
            instance = this;
            this.isLoading = new CountingSemaphore();
            this.isLoading.OnFirstRequest += instance.ShowScreen;
            this.isLoading.OnLastRelease += instance.HideScreen;
            FrigidInstancing.DontDestroyInstanceOnLoad(this);
        }

        private bool IsScreenFullyShown
        {
            get
            {
                return this.overlayCanvasGroup.alpha >= 1;
            }
        }

        private bool IsScreenPartiallyShown
        {
            get
            {
                return this.overlayCanvasGroup.alpha > 0;
            }
        }

        private void ShowScreen()
        {
            if (!this.IsScreenPartiallyShown)
            {
                CharacterInput.Disabled.Request();
                InterfaceInput.Disabled.Request();
                TimePauser.Paused.Request();
                AudioPauser.Paused.Request();
            }
            FrigidCoroutine.Kill(this.loadingScreenRoutine);
            this.loadingScreenRoutine =
                FrigidCoroutine.Run(
                    TweenCoroutine.Value(
                        this.fadeDuration.ImmutableValue * (1 - this.overlayCanvasGroup.alpha),
                        this.overlayCanvasGroup.alpha, 
                        1,
                        useRealTime: true,
                        onValueUpdated: (float alpha) => { this.overlayCanvasGroup.alpha = alpha; },
                        onComplete: () => { this.onScreenFullyShown?.Invoke(); this.onScreenFullyShown = null; }
                        ), 
                    this.gameObject
                    );
        }

        private void HideScreen()
        {
            FrigidCoroutine.Kill(this.loadingScreenRoutine);
            this.loadingScreenRoutine =
                FrigidCoroutine.Run(
                    TweenCoroutine.Value(
                        this.fadeDuration.ImmutableValue * this.overlayCanvasGroup.alpha,
                        this.overlayCanvasGroup.alpha,
                        0,
                        useRealTime: true,
                        onValueUpdated: (float alpha) => { this.overlayCanvasGroup.alpha = alpha; },
                        onComplete: () => 
                        {
                            CharacterInput.Disabled.Release();
                            InterfaceInput.Disabled.Release();
                            TimePauser.Paused.Release();
                            AudioPauser.Paused.Release();
                        }
                        ),
                    this.gameObject
                    );
        }

    }
}
