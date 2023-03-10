using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

using FrigidBlackwaters.Core;

namespace FrigidBlackwaters.Game
{
    public class ItemInterfaceGrid : FrigidMonoBehaviour
    {
        [Header("Slots")]
        [SerializeField]
        private Image backgroundImage;
        [SerializeField]
        private RectTransform slotsTransform;
        [SerializeField]
        private ItemInterfaceStashSlot slotPrefab;

        [Header("Transitions")]
        [SerializeField]
        private float transitionDuration;

        [Header("Optimizations")]
        [SerializeField]
        private int numberSlotsPreparedInAdvance;

        private List<ItemInterfaceStashSlot> currentSlots;
        private RecyclePool<ItemInterfaceStashSlot> slotPool;

        private Vector2 onScreenLocalPosition;
        private Vector2 offScreenBottomLocalPosition;
        private Vector2 offScreenTopLocalPosition;

        private FrigidCoroutine transitionRoutine;

        public void Populate(
            ItemStorageGrid itemStorageGrid, 
            ItemInterfaceHand hand,
            ItemInterfaceTooltip tooltip, 
            Vector2 localCenterPosition
            )
        {
            this.slotPool.Cycle(
                this.currentSlots,
                itemStorageGrid.Dimensions.x * itemStorageGrid.Dimensions.y
                );

            RectTransform backgroundRectTransform = (RectTransform)this.backgroundImage.transform;
            this.backgroundImage.sprite = itemStorageGrid.ItemContainer.Background;
            backgroundRectTransform.sizeDelta = new Vector2(this.backgroundImage.sprite.rect.width, this.backgroundImage.sprite.rect.height);

            RectTransform slotRectTransform = (RectTransform)this.slotPrefab.transform;
            float slotHeight = slotRectTransform.rect.width;
            float slotWidth = slotRectTransform.rect.height;

            this.slotsTransform.sizeDelta = new Vector2(itemStorageGrid.Dimensions.x * slotWidth, itemStorageGrid.Dimensions.y * slotHeight);
            Vector2 topLeftCorner = new Vector2(-this.slotsTransform.rect.width + slotWidth, this.slotsTransform.rect.height - slotHeight) / 2;
            for (int x = 0; x < itemStorageGrid.Dimensions.x; x++)
            {
                for (int y = 0; y < itemStorageGrid.Dimensions.y; y++)
                {
                    ItemInterfaceStashSlot slot = this.currentSlots[y * itemStorageGrid.Dimensions.x + x];
                    slot.transform.SetSiblingIndex(y * itemStorageGrid.Dimensions.x + x);
                    slot.transform.localPosition = topLeftCorner + new Vector2(x * slotWidth, -y * slotHeight);
                    if (itemStorageGrid.TryGetStash(new Vector2Int(x, y), out ContainerItemStash containerItemStash)) 
                    {
                        slot.PopulateForInteraction(itemStorageGrid, containerItemStash, hand, tooltip);
                    }
                }
            }

            RectTransform rectTransform = (RectTransform)this.transform;
            rectTransform.sizeDelta = new Vector2(
                Mathf.Max(this.slotsTransform.rect.width, backgroundRectTransform.rect.width),
                Mathf.Max(this.slotsTransform.rect.height, backgroundRectTransform.rect.height)
                );

            RectTransform parentRectTransform = (RectTransform)this.transform.parent;
            this.onScreenLocalPosition = localCenterPosition;
            this.offScreenBottomLocalPosition = new Vector2(localCenterPosition.x, -(parentRectTransform.rect.height + rectTransform.rect.height) / 2);
            this.offScreenTopLocalPosition = new Vector2(localCenterPosition.x, (parentRectTransform.rect.height + rectTransform.rect.height) / 2);

            this.transform.localPosition = this.offScreenBottomLocalPosition;
        }

        public void TransitionOnScreen(bool moveUpwards)
        {
            FrigidCoroutine.Kill(this.transitionRoutine);
            this.transitionRoutine = FrigidCoroutine.Run(
                TweenCoroutine.Value(
                    this.transitionDuration,
                    moveUpwards ? this.offScreenBottomLocalPosition : this.offScreenTopLocalPosition,
                    this.onScreenLocalPosition,
                    useRealTime: true,
                    onValueUpdated: (Vector2 localPosition) => this.transform.localPosition = localPosition
                    ), 
                this.gameObject
                );
        }

        public void TransitionOffScreen(bool moveUpwards, Action onComplete = null)
        {
            FrigidCoroutine.Kill(this.transitionRoutine);
            this.transitionRoutine = FrigidCoroutine.Run(
                TweenCoroutine.Value(
                    this.transitionDuration,
                    this.onScreenLocalPosition,
                    moveUpwards ? this.offScreenTopLocalPosition : this.offScreenBottomLocalPosition,
                    useRealTime: true,
                    onValueUpdated: (Vector2 localPosition) => this.transform.localPosition = localPosition,
                    onComplete: onComplete
                    ),
                this.gameObject
                );
        }

        protected override void Awake()
        {
            base.Awake();
            this.currentSlots = new List<ItemInterfaceStashSlot>();
            this.slotPool = new RecyclePool<ItemInterfaceStashSlot>(
                this.numberSlotsPreparedInAdvance,
                () => FrigidInstancing.CreateInstance<ItemInterfaceStashSlot>(this.slotPrefab, this.slotsTransform, false),
                (ItemInterfaceStashSlot stashSlot) => FrigidInstancing.DestroyInstance(stashSlot)
                );
        }

#if UNITY_EDITOR
        protected override bool OwnsGameObject() { return true; }
#endif
    }
}
