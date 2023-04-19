﻿using MongoDB.Bson.Serialization.Attributes;

namespace Common.Base
{
    /// <summary>
    /// Component的Id与Owner Entity Id一样
    /// </summary>
    public abstract class Component<T>: Object where T: Entity<T>
    {
        private T owner;

        [BsonIgnore]
        public T Owner
        {
            get
            {
                return owner;
            }
            set
            {
                this.owner = value;
                this.Id = this.owner.Id;
            }
        }
    }
}