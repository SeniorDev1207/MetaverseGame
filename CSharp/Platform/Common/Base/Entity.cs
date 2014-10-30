﻿using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace Common.Base
{
    public abstract class Entity<K> : Object where K : Entity<K>
    {
        [BsonElement] 
        [BsonIgnoreIfNull]
        private HashSet<Component<K>> components;

        private Dictionary<Type, Component<K>> componentDict = new Dictionary<Type, Component<K>>();

        public T AddComponent<T>() where T : Component<K>, new()
        {
            if (this.componentDict.ContainsKey(typeof (T)))
            {
                throw new Exception(
                    string.Format("AddComponent, component already exist, id: {0}, component: {1}", 
                    this.Id, typeof(T).Name));
            }

            if (this.components == null)
            {
                this.components = new HashSet<Component<K>>();
            }

            T t = new T { Owner = (K)this };
            this.components.Add(t);
            this.componentDict.Add(typeof (T), t);
            return t;
        }

        public void RemoveComponent<T>() where T : Component<K>
        {
            Component<K> t;
            if (!this.componentDict.TryGetValue(typeof (T), out t))
            {
                throw new Exception(
                    string.Format("RemoveComponent, component not exist, id: {0}, component: {1}",
                    this.Id, typeof(T).Name));
            }
            
            this.components.Remove(t);
            this.componentDict.Remove(typeof(T));

            if (this.components.Count == 0)
            {
                this.components = null;
            }
        }

        public T GetComponent<T>() where T : Component<K>
        {
            Component<K> t;
            if (!this.componentDict.TryGetValue(typeof (T), out t))
            {
                return default (T);
            }
            return (T) t;
        }

        public Component<K>[] GetComponents()
        {
            return this.components.ToArray();
        }

        public override void BeginInit()
        {
            base.BeginInit();
            this.components = new HashSet<Component<K>>();
            this.componentDict = new Dictionary<Type, Component<K>>();
        }

        public override void EndInit()
        {
            base.EndInit();
            if (this.components.Count == 0)
            {
                this.components = null;
                return;
            }
            foreach (var component in this.components)
            {
                component.Owner = (K)this;
                this.componentDict.Add(component.GetType(), component);
            }
        }
    }
}