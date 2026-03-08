using System;
using System.Collections.Generic;
using Cavrnus.SpatialConnector.API;
using UnityEngine;

namespace Cavrnus.SpatialConnector.Setup
{
	public class CavrnusUserFlag : MonoBehaviour
    {
        private CavrnusUser user;
        private readonly List<Action<CavrnusUser>> callbacks = new ();
        
        internal CavrnusUser User
        {
            set
            {
                user = value;

                foreach (var cb in callbacks)
                    cb?.Invoke(user);

                callbacks.Clear();
            }
        }

        /// <summary>
        /// If the user is already set, call immediately.
        /// Otherwise, enqueue callback until User is set.
        /// </summary>
        public void AwaitUser(Action<CavrnusUser> onUserArrived)
        {
            if (user != null)
                onUserArrived?.Invoke(user);
            else
                callbacks.Add(onUserArrived);
        }
    }
}