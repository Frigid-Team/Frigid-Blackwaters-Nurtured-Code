using System;
using UnityEngine;

namespace FrigidBlackwaters.Core
{
    public class CountingSemaphore
    {
        private int counter;
        private Action onFirstRequest;
        private Action onLastRelease;

        public static implicit operator bool(CountingSemaphore semaphore)
        {
            return semaphore.counter > 0;
        }

        public CountingSemaphore(int counter = 0) 
        { 
            this.counter = counter; 
        }

        public Action OnFirstRequest
        {
            get
            {
                return this.onFirstRequest;
            }
            set
            {
                this.onFirstRequest = value;
            }
        }

        public Action OnLastRelease
        {
            get
            {
                return this.onLastRelease;
            }
            set
            {
                this.onLastRelease = value;
            }
        }

        public void Request()
        {
            if (this.counter == 0)
            {
                this.onFirstRequest?.Invoke();
            }
            this.counter += 1;
        }

        public void Release()
        {
            if (this.counter == 1)
            {
                this.onLastRelease?.Invoke();
            }
            this.counter = Mathf.Max(0, this.counter - 1);
        }
    }
}
