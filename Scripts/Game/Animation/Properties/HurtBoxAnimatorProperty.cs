using UnityEngine;

using FrigidBlackwaters.Utility;

namespace FrigidBlackwaters.Game
{
    public class HurtBoxAnimatorProperty : DamageReceiverBoxAnimatorProperty<HurtBox, HitBox, HitInfo>
    {
        [SerializeField]
        [HideInInspector]
        private Nested2DList<int> damageMitigations;

        public int DamageMitigation
        {
            get
            {
                return this.DamageReceiverBox.DamageMitigation;
            }
            set
            {
                this.DamageReceiverBox.DamageMitigation = value;
            }
        }

        public int GetDamageMitigation(int animationIndex, int frameIndex)
        {
            return this.damageMitigations[animationIndex][frameIndex];
        }

        public void SetDamageMitigation(int animationIndex, int frameIndex, int damageMitigation)
        {
            if (this.damageMitigations[animationIndex][frameIndex] != damageMitigation)
            {
                FrigidEditMode.RecordPotentialChanges(this);
                this.damageMitigations[animationIndex][frameIndex] = damageMitigation;
            }
        }

        public override void Created()
        {
            FrigidEditMode.RecordPotentialChanges(this);
            this.damageMitigations = new Nested2DList<int>();
            for (int animationIndex = 0; animationIndex < this.Body.GetAnimationCount(); animationIndex++)
            {
                this.damageMitigations.Add(new Nested1DList<int>());
                for (int frameIndex = 0; frameIndex < this.Body.GetFrameCount(animationIndex); frameIndex++)
                {
                    this.damageMitigations[animationIndex].Add(0);
                }
            }
            base.Created();
        }

        public override void AnimationAddedAt(int animationIndex)
        {
            FrigidEditMode.RecordPotentialChanges(this);
            this.damageMitigations.Insert(animationIndex, new Nested1DList<int>());
            for (int frameIndex = 0; frameIndex < this.Body.GetFrameCount(animationIndex); frameIndex++)
            {
                this.damageMitigations[animationIndex].Add(0);
            }
            base.AnimationAddedAt(animationIndex);
        }

        public override void AnimationRemovedAt(int animationIndex)
        {
            FrigidEditMode.RecordPotentialChanges(this);
            this.damageMitigations.RemoveAt(animationIndex);
            base.AnimationRemovedAt(animationIndex);
        }

        public override void FrameAddedAt(int animationIndex, int frameIndex)
        {
            FrigidEditMode.RecordPotentialChanges(this);
            this.damageMitigations[animationIndex].Insert(frameIndex, 0);
            base.FrameAddedAt(animationIndex, frameIndex);
        }

        public override void FrameRemovedAt(int animationIndex, int frameIndex)
        {
            FrigidEditMode.RecordPotentialChanges(this);
            this.damageMitigations[animationIndex].RemoveAt(frameIndex);
            base.FrameRemovedAt(animationIndex, frameIndex);
        }

        public override void CopyPasteToAnotherFrame(AnimatorProperty otherProperty, int fromAnimationIndex, int toAnimationIndex, int fromFrameIndex, int toFrameIndex)
        {
            HurtBoxAnimatorProperty otherHurtBoxProperty = otherProperty as HurtBoxAnimatorProperty;
            if (otherHurtBoxProperty)
            {
                otherHurtBoxProperty.SetDamageMitigation(toAnimationIndex, toFrameIndex, GetDamageMitigation(fromAnimationIndex, fromFrameIndex));
            }
            base.CopyPasteToAnotherFrame(otherProperty, fromAnimationIndex, toAnimationIndex, fromFrameIndex, toFrameIndex);
        }

        public override void SetFrameEnter(int animationIndex, int frameIndex, float elapsedDuration, int loopsElapsed)
        {
            this.DamageReceiverBox.DamageMitigation += GetDamageMitigation(animationIndex, frameIndex);
            base.SetFrameEnter(animationIndex, frameIndex, elapsedDuration, loopsElapsed);
        }

        public override void SetFrameExit(int animationIndex, int frameIndex)
        {
            this.DamageReceiverBox.DamageMitigation -= GetDamageMitigation(animationIndex, frameIndex);
            base.SetFrameExit(animationIndex, frameIndex);
        }
    }
}
