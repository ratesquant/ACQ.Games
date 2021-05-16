using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ACQ.Core
{
    [Serializable]
    public class ApplicationData : Hashtable
    {
        // File name. Let us use the entry assembly name with .dat as the extension.
        private readonly string m_FileName;

        public ApplicationData() :
            this(System.Reflection.Assembly.GetEntryAssembly().GetName().Name + ".dat", true)
        {            
        }


        // The default constructor.
        public ApplicationData(string filename, bool bLoadData)
        {
            m_FileName = filename;

            if(bLoadData)
                LoadData();
        }
        
        // This constructor is required for deserializing our class from persistent storage.
        protected ApplicationData(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private void LoadData()
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            if (isoStore.GetFileNames(m_FileName).Length == 0)
            {
                // File not exists. Let us NOT try to DeSerialize it.
                return;
            }

            // Read the stream from Isolated Storage.
            Stream stream = new IsolatedStorageFileStream(m_FileName, FileMode.OpenOrCreate, isoStore);
            if (stream != null)
            {
                try
                {
                    // DeSerialize the Hashtable from stream.
                    IFormatter formatter = new BinaryFormatter();
                    Hashtable appData = (Hashtable)formatter.Deserialize(stream);

                    // Enumerate through the collection and load our base Hashtable.
                    IDictionaryEnumerator enumerator = appData.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        this[enumerator.Key] = enumerator.Value;
                    }
                }
                finally
                {
                    // We are done with it.
                    stream.Close();
                }
            }
        }

        public void ReLoad()
        {
            LoadData();
        }

        // Saves the configuration data to the persistent storage.
        public void Save()
        {
            // Open the stream from the IsolatedStorage.
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User
                        | IsolatedStorageScope.Assembly, null, null);
            Stream stream = new IsolatedStorageFileStream(m_FileName,
                        FileMode.Create, isoStore);

            if (stream != null)
            {
                try
                {
                    // Serialize the Hashtable into the IsolatedStorage.
                    IFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, (Hashtable)this);
                }
                finally
                {
                    stream.Close();
                }
            }
        }
    }
}
