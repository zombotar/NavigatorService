using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace WcfService1
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени интерфейса "IService1" в коде и файле конфигурации.
    [ServiceContract]
    public interface IService1
    {

        [OperationContract]
        string GetData(int value);

        [OperationContract]
        AuthenticationResult Authentication(string _username, string _p_hash);

        [OperationContract]
        CompositeType GetDataUsingDataContract(CompositeType composite);

        [OperationContract]
        BrowserDataInfo AfterAuth(int _idUser);

        [OperationContract]
        BrowserDataInfo GetListOfData(int _idUser, string _path);
        // TODO: Добавьте здесь операции служб
    }


    // Используйте контракт данных, как показано в примере ниже, чтобы добавить составные типы к операциям служб.
    [DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }

    [DataContract]
    public class AuthenticationResult
    {
        [DataMember]
        public int resultCode { get; set; }

        [DataMember]
        public string errMessage { get; set; }

        [DataMember]
        public int userId { get; set; }
    }

    [DataContract]
    public class File
    {
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string FILENAME { get; set; }

        [DataMember]
        public int FILESIZE { get; set; }

        [DataMember]
        public byte[] DATA { get; set; }

        [DataMember]
        public int OWNER_ID { get; set; }

        [DataMember]
        public Nullable<int> GROUP_ID { get; set; } 

        [DataMember]
        public int ACCESS_BITSET { get; set; }
    }

    [DataContract]
    public class AddFileResult
    {
        [DataMember]
        public int resultCode { get; set; }

        [DataMember]
        public int fileID { get; set; }

        [DataMember]
        public string errMessage { get; set; }
    }

    [DataContract]
    public class BrowserDataInfo
    {
        [DataMember]
        public List<MyFileInfo> mFiles { get; set; }

        [DataMember]
        public List<MyDirectoryInfo> mDirectories { get; set; }

        [DataMember]
        public int mErrCode { get; set; }

        [DataMember]
        public string mErrMessage { get; set; }

        [DataMember]
        public string rootPath { get; set; }

        [DataMember]
        public string currPath { get; set; }
    }

    public class MyFileInfo
    {
        public FileInfo mFileInfo { get; set; }
        public ObjectInfo mMetaObject { get; set; }

        public MyFileInfo()
        {
            mFileInfo = null;
        }

        public MyFileInfo(string path)
        {
            mFileInfo = new FileInfo(path);
        }
    }

    public class MyDirectoryInfo
    {
        public DirectoryInfo mDirectoryInfo { get; set; }
        public ObjectInfo mMetaObject { get; set; }

        public MyDirectoryInfo()
        {
            mDirectoryInfo = null;
        }

        public MyDirectoryInfo(string path)
        {
            mDirectoryInfo = new DirectoryInfo(path);
        }
    }

    public class ObjectInfo
    {
        public string filePath { get; set; }
        public int id { get; set; }
        public bool isExists { get; set; }
        public bool isDirectory { get; set; }
    }
}
