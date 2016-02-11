﻿using System;
using Static_Interface.Internal.MultiplayerFramework;
using UnityEngine;

namespace Static_Interface.API.NetworkFramework
{
    public abstract class NetworkedBehaviour : MonoBehaviour
    {
        public Channel Channel { get; private set; }

        protected virtual void Awake()
        {
            Channel = GetComponent<Channel>();
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            // Channel = GetComponent<Channel>() ?? gameObject.AddComponent<Channel>();
            if (Channel != null) return;
            Channel = gameObject.AddComponent<Channel>();
            Channel.Setup();
        }

        protected virtual void Start()
        {
            
        }

        protected virtual void Update()
        {
            
        }

        protected virtual void FixedUpdate()
        {

        }

        protected virtual void OnDestroy()
        {
            
        }

        public void CheckServer()
        {
            if (!Connection.IsServer())
            {
                throw new Exception("This can be only called from server-side!");
            }
        }
    }
}