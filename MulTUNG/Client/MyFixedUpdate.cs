using PiTung.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace MulTUNG
{
    public class MyFixedUpdate : CustomFixedUpdate
    {
        public static bool NextInstanceIsNice = false;
        public static MyFixedUpdate Instance { get; private set; }

        private OnFixedUpdateCallback Callback { get; }
        private MethodInfo UpdateMethod;
        private BehaviorManager BehaviorInstance;

        public MyFixedUpdate(OnFixedUpdateCallback aCallback) : base(aCallback)
        {
            if (NextInstanceIsNice)
            {
                NextInstanceIsNice = false;
                Instance = this;

                base.updateRate = 0;
            }

            this.Callback = aCallback;
            this.UpdateMethod = typeof(BehaviorManager).GetMethod("OnCircuitLogicUpdate", BindingFlags.Static | BindingFlags.NonPublic);
            this.BehaviorInstance = GameObject.FindObjectOfType<BehaviorManager>();
        }

        public MyFixedUpdate(float aTimeStep, OnFixedUpdateCallback aCallback) : base(aTimeStep, aCallback)
        {
        }

        public MyFixedUpdate(OnFixedUpdateCallback aCallback, float aFPS) : base(aCallback, aFPS)
        {
        }

        public MyFixedUpdate(float aTimeStep, OnFixedUpdateCallback aCallback, float aMaxAllowedTimestep) : base(aTimeStep, aCallback, aMaxAllowedTimestep)
        {
        }

        public MyFixedUpdate(OnFixedUpdateCallback aCallback, float aFPS, float aMaxAllowedTimestep) : base(aCallback, aFPS, aMaxAllowedTimestep)
        {
        }

        public void ForceUpdate()
        {
            this.UpdateMethod.Invoke(this.BehaviorInstance, new object[] { 1 / this.updateRate });
        }
    }
}
