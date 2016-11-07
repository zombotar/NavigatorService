﻿using System;
using System.Collections.Generic;
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
    }
}
