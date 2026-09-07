using DevLib.ModuleSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace CIW.Code.System
{
    public abstract class Entity : ModuleOwner
    {
        bool _isDead;

        public bool IsDead
        {
            get => _isDead;
            set
            {
                if (_isDead) return;

                _isDead = value;
            }
        }

        public UnityEvent OnDeadEvent;

        protected Dictionary<Type, IModule> _components;

        protected override void Awake()
        {
            base.Awake();
            _components = new Dictionary<Type, IModule>();
            AddComponents();
        }

        protected override void Start()
        {
        }

        protected virtual void AddComponents()
        {
            GetComponentsInChildren<IModule>().ToList().ForEach(compo => _components.Add(compo.GetType(), compo));
        }

        public void DestroyEntity()
        {
            Die();
            Destroy(gameObject);
        }

        protected virtual void Die() { }
    }
}