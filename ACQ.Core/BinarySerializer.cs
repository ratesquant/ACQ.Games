using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace ACQ.Core
{
    public interface IKamlexBinarySerialize
    {
        void Write(BinaryWriter writer);
        void Read(BinaryReader reader);
    }

    public class KamlexBinaryWriter : BinaryWriter
    {
        private readonly static Dictionary<Type, System.Reflection.MethodInfo> m_writeMethods = new Dictionary<Type, System.Reflection.MethodInfo>();
        private readonly Dictionary<Type, Delegate> m_write = new Dictionary<Type, Delegate>();

        static KamlexBinaryWriter()
        {   
            MethodInfo[] info = typeof(BinaryWriter).GetMethods();

            for (int i = 0; i < info.Length; i++)
            {
                if (info[i].Name.Equals("Write"))
                {
                    ParameterInfo[] pinfo = info[i].GetParameters();

                    if (pinfo.Length == 1)                    
                    {
                        Type type = pinfo[0].ParameterType;
                        m_writeMethods[type] = info[i];
                    }
                }
            }             
        }

        private delegate void WriteFuction<T>(T obj);
     
        public KamlexBinaryWriter(Stream s)
            : base(s)
        {            
            foreach (Type type in m_writeMethods.Keys)
            {
                m_write[type] = Delegate.CreateDelegate((typeof(WriteFuction<>)).MakeGenericType(new System.Type[] { type }), this, m_writeMethods[type]);                
            }            
        }

        /// <summary>
        /// CreateGeneric(typeof(List<>), typeof(string))
        /// </summary>
        /// <param name="generic"></param>
        /// <param name="innerType"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object CreateGeneric(Type generic, Type innerType, params object[] args)
        {
            System.Type specificType = generic.MakeGenericType(new System.Type[] { innerType });
            return Activator.CreateInstance(specificType, args);
        }

        #region Writers
        public void Write<T>(List<T> list)
        {
            base.Write(list.Count);

            for (int i = 0; i < list.Count; i++)
                WriteObject(list[i]);
        }

        public void Write<T, U>(Dictionary<T, U> dic)
        {
            base.Write(dic.Count);

            foreach (KeyValuePair<T, U> p in dic)
            {
                WriteObject(p.Key);
                WriteObject(p.Value);
                //Write<T>(p.Key);
                //Write<U>(p.Value);
            }
        }

        public void WriteObject(object obj)
        {
            if (obj == null)
                return;

            if (obj is IKamlexBinarySerialize)
                ((IKamlexBinarySerialize)obj).Write(this);

            MethodInfo method = null;
            switch (obj.GetType().Name)
            {
                case "String": base.Write((string)obj); break;
                case "Int32": base.Write((int)obj); break;
                default:
                    if(m_writeMethods.TryGetValue(obj.GetType(), out method))
                        method.Invoke(this, new object[] { obj });
                    break;
            }           
         
        }
        public void Write<T>(T obj)
        {
            if (obj == null)
                return;

            if (obj is IKamlexBinarySerialize)
                ((IKamlexBinarySerialize)obj).Write(this);
            
            Delegate method = null;
            if (m_write.TryGetValue(typeof(T), out method))
                ((WriteFuction<T>)method)(obj);           
        }
        #endregion   
    }

    public class KamlexBinaryReader: BinaryReader
    {
        static KamlexBinaryReader()
        {          

        }

        public KamlexBinaryReader(Stream s)
            : base(s)
        { }

        #region Reader
        public List<T> ReadList<T>(Type type)
        {
            int count = base.ReadInt32();

            List<T> list = new List<T>(count);

            for (int i = 0; i < count; i++)
            {
                T item = (T)ReadObject(type);
               list.Add(item);
            }

            return list;
        } 
       
        public Dictionary<T, U> ReadDictionary<T, U>(Type keyType, Type valueType)
        {
            int count = base.ReadInt32();

            Dictionary<T, U> dic = new Dictionary<T, U>(count);

            for (int i = 0; i < count; i++)
            {
                T key = (T)ReadObject(keyType);
                U value = (U)ReadObject(valueType);
                dic[key] = value;
            }

            return dic;
        }   

        public object ReadObject(Type type)
        {
           switch(type.Name)
           {
               case "Int32": return (object)ReadInt32();
               case "String": return (object)base.ReadString();
               default: return null; 
           }
        }
        #endregion
    }
}

