using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ACQ.Core
{
    /// <summary>
    /// Set is a collection that contains no duplicate elements 
    /// 
    /// NOTE: Internally, always use m_vDictionary rather than member functions that are intended for external users of Set
    /// i.e. use this.m_vDictionary.Remove(); instead of this.Remove();
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]    
    public class Set<T> : ICollection<T>, ICloneable
    {
        #region Properties
        private const int m_value = 0; //dummy value used in dictionary
        private Dictionary<T, int> m_vDictionary;

        // An integer that is changed every time the Set changes.
        // Used so that enumerations throw an exception if the Set is changed
        // during enumeration.
        private uint m_nChanges = 0; 
        

        #endregion // Properties        

        #region Constructors

        /// <summary>
        /// Constructs a Set from the collection, elements of collection do not have to be unique
        /// </summary>
        /// <param name="collection"></param>
        public Set(ICollection<T> collection)
            : this()
        {
            foreach (T item in collection)
                m_vDictionary[item] = m_value;
        }

        /// <summary>
        /// Constructs a Set from the array, elements of the arrays do not have to be unique
        /// </summary>
        /// <param name="array"></param>
        public Set(T[] array)
            : this()
        {
            foreach (T item in array)
                m_vDictionary[item] = m_value;
        }

        /// <summary>
        /// Constructs empty set 
        /// </summary>
        public Set()
            : this(EqualityComparer<T>.Default)
        {   
        }
        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="set"></param>
        public Set(Set<T> set)            
        {
            m_vDictionary = new Dictionary<T, int>(set.m_vDictionary);
        }
       
        public Set(IEqualityComparer<T> comparer)
        {   
            m_vDictionary = new Dictionary<T, int>(comparer);
        }    

        #endregion // Constructors

        #region Clone
        public Set<T> Clone()
        {
            return new Set<T>(this);
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        } 
        #endregion

        #region Casts

        public static explicit operator T[](Set<T> a)
        {
            T[] array = new T[a.m_vDictionary.Count];

            a.m_vDictionary.Keys.CopyTo(array, 0);

            return array;
        }

        #endregion

        #region ICollection<T>

        public void Add(T item)
        {
            if (!m_vDictionary.ContainsKey(item))
            {
                m_nChanges++;                
                m_vDictionary.Add(item, m_value);
            }
        }

        public void Clear()
        {
            m_nChanges++;
            m_vDictionary.Clear();
        }

        public bool Contains(T item)
        {
            return m_vDictionary.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            m_vDictionary.Keys.CopyTo(array, arrayIndex);            
        }

        public int Count
        {
            get
            {
                return this.m_vDictionary.Count;
            }
        }
        
        public bool Remove(T item)
        {
            if (m_vDictionary.Remove(item))
            {
                m_nChanges++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }
        #endregion // ICollection<T>

        #region IEnumerable<T>

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            if (m_vDictionary.Count > 0)
            {
                uint changes = m_nChanges;

                foreach (T item in m_vDictionary.Keys)
                {
                    yield return item;
                    //after item is returned to the parent foreach cycle using yield keyword program jumps to parent foreach cycle 
                    //when one parent cycle is completed progam jumps back and we can check if collection was modified during parent cycle

                    if (changes != m_nChanges)
                        throw new InvalidOperationException("Collection was modified during an enumeration.");
                }
            }
        }
        
        /// <summary>
        /// non-generic enumerator
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        #endregion // IEnumerable<T>        
        
        #region Operators
        public Set<T> Union(Set<T> a)
        {
            Set<T> res = this.Clone();

            if (a != null)
            {
                foreach (T item in a.m_vDictionary.Keys)
                    res.m_vDictionary[item] = m_value;
            }
            return res;
        }

        /// <summary>
        /// A.Union(B) returns a set containing all the elements from A and B.
        /// </summary>
        /// <returns>returns a set containing all the elements from A and B</returns>
        public static Set<T> Union(Set<T> a, Set<T> b)
        {
            if (a == null && b == null)
                return null;
            else if (a == null)
                return b.Clone();
            else if (b == null)
                return a.Clone();
            else
                return a.Union(b);
        }

        /// <summary>
        /// A | B returns a set containing all the elements from A and B
        /// </summary>
        public static Set<T> operator | (Set<T> a, Set<T> b)
        {
            return Union(a, b);
        }

        /// <summary>
        /// Returns a set containing all the elements that are in both sets
        /// </summary>
        public Set<T> Intersect(Set<T> set)
        {
            if (set == null)
                return null;

            Set<T> res = new Set<T>(m_vDictionary.Comparer);

            foreach (T item in set.m_vDictionary.Keys)
                if (m_vDictionary.ContainsKey(item))
                    res.m_vDictionary.Add(item, m_value);
                
            return res;
        }

        /// <summary>
        /// Returns a set containing all the elements that are in both A and B
        /// </summary>
        public static Set<T> Intersect(Set<T> a, Set<T> b)
        {
            if (a == null && b == null)
                return null;
            else if (a == null)
                return b.Intersect(a);
            else
                return a.Intersect(b);            
        }
        
        /// <summary>
        /// A & B returns a set containing all the elements that are in both A and B.
        /// </summary>
        /// <returns></returns>
        public static Set<T> operator & (Set<T> a, Set<T> b)
        {
            return Intersect(a, b);
        }
        
        /// <summary>
        /// Returns values that are not in A
        /// </summary>
        /// <returns>returns the values of set that are not in A. </returns>
        public Set<T> Diff(Set<T> a)
        {
            Set<T> res = this.Clone();
            if (a != null)
            {
                foreach(T item in a.m_vDictionary.Keys)
                    res.m_vDictionary.Remove(item);
            }                
            return res;
        }

    
        /// <summary>
        /// Returns the values in A that are not in B.
        /// </summary>
        public static Set<T> Diff(Set<T> a, Set<T> b)
        {
            if (a == null)
                return null;
            else
                return a.Diff(b);
        }
        
        /// <summary>
        /// Returns the values in A that are not in B. 
        /// </summary>
        public static Set<T> operator -(Set<T> a, Set<T> b)
        {
            return Diff(a, b);
        }


        /// <summary>
        /// Computes an "exclusive-or" XOR of the two sets, 
        /// returns the values that are not in the intersection of this set and A
        /// </summary>
        public virtual Set<T> XOR(Set<T> a)
        {
            Set<T> res = this.Clone();
            foreach (T item in a.m_vDictionary.Keys)
            {
                if (res.m_vDictionary.ContainsKey(item))
                    res.m_vDictionary.Remove(item);
                else
                    res.m_vDictionary.Add(item, m_value);                    
            }
            return res;
        }

        /// <summary>
        /// returns the values that are not in the intersection of A and B
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Set<T> XOR(Set<T> a, Set<T> b)
        {
            if (a == null && b == null)
                return null;
            else if (a == null)
                return b.Clone();
            else if (b == null)
                return a.Clone();
            else
                return a.XOR(b);
        }
        
        /// <summary>
        /// returns the values that are not in the intersection of A and B
        /// </summary>
        /// <returns>A ^ B returns a set containing the elements that are in A or in B, but are not in both A and B.</returns>
        public static Set<T> operator ^(Set<T> a, Set<T> b)
        {
            return XOR(a, b);
        }

        public static bool operator == (Set<T> a, Set<T> b)
        {
            //if both A and B are null then they are considered equal 
            //note that we have to cast reference to object otherwise we get an infinite loop
            if ((object)a == null && (object)b == null)
                return true;

            //here they can not be both null since this case is handled by the first if            
            if (((object)a == null || (object)b == null))
                return false;

            //here A and B are not null and we test for value equality
            return a.Equals(b);
        }

        public static bool operator !=(Set<T> a, Set<T> b)
        {
            return !(a==b);
        }
        #endregion

        #region Object 
        /// <summary>
        /// Test for value equality instead of referential equality. 
        /// Equals return true if the two objects have the same "value", 
        /// even if they are not the same instance
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public override bool Equals(object o)
        {
            //we have compare types using GetType instead of (o is Set<T>)
            if (o == null || (o.GetType() != GetType()) || ((Set<T>)o).Count != this.Count)
                return false;
            else
            {
                Set<T> a = o as Set<T>;

                foreach (T item in a.m_vDictionary.Keys)
                {
                    if (!m_vDictionary.ContainsKey(item))
                        return false;
                }
                return true;
            }
        }
        
        /// <summary>
        /// Returns a Set hash code
        /// Types that override Equals must also override GetHashCode; otherwise, Hashtable might not work correctly
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return m_vDictionary.GetHashCode();
        }
        #endregion
    }
}
