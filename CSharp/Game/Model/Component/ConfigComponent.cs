﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Common.Base;
using Common.Config;

namespace Model
{
    public class ConfigComponent : Component<World>, IAssemblyLoader
    {
        public Dictionary<Type, ICategory> allConfig;

        public void Load(Assembly assembly)
        {
            allConfig = new Dictionary<Type, ICategory>();
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                object[] attrs = type.GetCustomAttributes(typeof(ConfigAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }

                object obj = (Activator.CreateInstance(type));

                ICategory iCategory = obj as ICategory;
                if (iCategory == null)
                {
                    throw new Exception(
                        string.Format("class: {0} not inherit from ACategory", type.Name));
                }
                iCategory.BeginInit();
                iCategory.EndInit();

                allConfig[iCategory.ConfigType] = iCategory;
            }
        }

        public T Get<T>(int id) where T : AConfig
        {
            Type type = typeof (T);
            ICategory configCategory;
            if (!this.allConfig.TryGetValue(type, out configCategory))
            {
                throw new KeyNotFoundException(string.Format("ConfigComponent not found key: {0}", type.FullName));
            }
            return ((ACategory<T>) configCategory)[id];
        }

        public T[] GetAll<T>() where T : AConfig
        {
            Type type = typeof(T);
            ICategory configCategory;
            if (!this.allConfig.TryGetValue(type, out configCategory))
            {
                throw new KeyNotFoundException(string.Format("ConfigComponent not found key: {0}", type.FullName));
            }
            return ((ACategory<T>)configCategory).GetAll();
        }

        public T GetCategory<T>() where T : class, ICategory, new()
        {
            T t = new T();
            Type type = t.ConfigType;
            ICategory category;
            bool ret = this.allConfig.TryGetValue(type, out category);
            return ret? (T) category : null;
        }
    }
}