﻿using System;
using System.Collections.Generic;
using Common.Base;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Model
{
    public class BuffComponent: Component<Unit>
    {
        [BsonElement]
        private HashSet<Buff> buffs;

        private Dictionary<ObjectId, Buff> idBuff;

        private MultiMap<BuffType, Buff> typeBuff;

        public BuffComponent()
        {
            this.buffs = new HashSet<Buff>();
            this.idBuff = new Dictionary<ObjectId, Buff>();
            this.typeBuff = new MultiMap<BuffType, Buff>();
        }

        public override void BeginInit()
        {
            base.BeginInit();

            this.buffs = new HashSet<Buff>();
            this.idBuff = new Dictionary<ObjectId, Buff>();
            this.typeBuff = new MultiMap<BuffType, Buff>();
        }

        public override void EndInit()
        {
            base.EndInit();

            foreach (var buff in this.buffs)
            {
                this.idBuff.Add(buff.Id, buff);
                this.typeBuff.Add(buff.Config.Type, buff);
            }
        }

        public void Add(Buff buff)
        {
            if (this.buffs.Contains(buff))
            {
                throw new ArgumentException(string.Format("already exist same buff, Id: {0} ConfigId: {1}", buff.Id, buff.Config.Id));
            }

            if (this.idBuff.ContainsKey(buff.Id))
            {
                throw new ArgumentException(string.Format("already exist same buff, Id: {0} ConfigId: {1}", buff.Id, buff.Config.Id));
            }

            this.buffs.Add(buff);
            this.idBuff.Add(buff.Id, buff);
            this.typeBuff.Add(buff.Config.Type, buff);
        }

        public Buff GetById(ObjectId id)
        {
            if (!this.idBuff.ContainsKey(id))
            {
                return null;
            }

            return this.idBuff[id];
        }

        public Buff GetOneByType(BuffType type)
        {
            return this.typeBuff.GetOne(type);
        }

        public Buff[] GetByType(BuffType type)
        {
            return this.typeBuff.GetByKey(type);
        }

        private bool Remove(Buff buff)
        {
            if (buff == null)
            {
                return false;
            }

            this.buffs.Remove(buff);
            this.idBuff.Remove(buff.Id);
            this.typeBuff.Remove(buff.Config.Type, buff);

            return true;
        }

        public bool RemoveById(ObjectId id)
        {
            Buff buff = this.GetById(id);
            return this.Remove(buff);
        }

        public void RemoveByType(BuffType type)
        {
            Buff[] allbuffs = this.GetByType(type);
            foreach (Buff buff in allbuffs)
            {
                this.Remove(buff);
            }
        }
    }
}