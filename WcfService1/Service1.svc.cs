using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace WcfService1
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "Service1" в коде, SVC-файле и файле конфигурации.
    // ПРИМЕЧАНИЕ. Чтобы запустить клиент проверки WCF для тестирования службы, выберите элементы Service1.svc или Service1.svc.cs в обозревателе решений и начните отладку.
    public class Service1 : IService1
    {
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }

        private int GetUserID(string _username, string _p_hash)
        {
            int id = -1;

            SQLiteConnection connection =
                new SQLiteConnection(string.Format("Data Source={0};", MyClass.Instance.currentDirectory + MyClass.Instance.databaseName));
            string commandText = String.Format("select id from USERS where name = \"{0}\" and p_hash = \"{1}\" ;", _username, _p_hash);
            SQLiteCommand command =
                new SQLiteCommand(commandText, connection);

            try
            {
                connection.Open();
                var result = command.ExecuteReader();
                if (result.Read())
                    id = Int32.Parse(result["id"].ToString());
            }
            catch (Exception e)
            {

            }
            finally
            {
                connection.Close();
            }

            return id;
        }

        public AuthenticationResult Authentication(string _username, string _p_hash)
        {
            AuthenticationResult result = new AuthenticationResult();
            result.resultCode = -1;
            var x = MyClass.Instance;
            result.userId = GetUserID(_username, _p_hash);
            return result;
        }
    }

    public class MyClass
    {
        // file objects
        public string mainDirectoryName = "fileStorage";
        public string databaseName = "database.db";
        public string currentDirectory = "";

        // tables
        

        static object _locker = new object();
        static MyClass _instance;

        private MyClass()
        {
        }

        public static MyClass Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                lock (_locker)
                {
                    if (_instance != null)
                    {
                        return _instance;
                    }

                    _instance = new MyClass();

                    _instance.Initialization();

                    return _instance;
                }
            }
        }

        public void Initialization()
        {
            Console.WriteLine("Initialization");

            currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            SQLiteConnection connection =
                new SQLiteConnection(string.Format("Data Source={0};", currentDirectory + databaseName));
            string commandText = String.Format
                (
                "CREATE TABLE IF NOT EXISTS \"USERS\" (\"id\" INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL , \"name\" TEXT NOT NULL , \"p_hash\" BLOB NOT NULL  UNIQUE ); " +
                "CREATE TABLE IF NOT EXISTS \"GROUPS\" (\"id\" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, \"groupname\" TEXT NOT NULL); " +
                "CREATE TABLE IF NOT EXISTS \"USER_TO_GROUP\" (\"userid\" INTEGER, \"groupid\" INTEGER, PRIMARY KEY (userid, groupid), FOREIGN KEY (userid) REFERENCES USERS(id), FOREIGN KEY (groupid) REFERENCES GROUPS(id) ); " +
                "CREATE TABLE IF NOT EXISTS \"FILE_META\" (\"id\" INTEGER PRIMARY KEY  AUTOINCREMENT NOT NULL, \"filepath\" TEXT, \"filesize\" INTEGER, \"ownerid\" INTEGER, \"groupid\" INTEGER, \"access_bitset\" INTEGER NOT NULL, FOREIGN KEY (ownerid) REFERENCES USERS(id), FOREIGN KEY (groupid) REFERENCES GROUPS(id) );" +
                "CREATE TABLE IF NOT EXISTS \"LOG_TABLE\" (\"id\" INTEGER PRIMARY KEY AUTOINCREMENT, \"userid\" INTEGER NOT NULL, \"groupid\" INTEGER, \"date_time\" TEXT, \"value\" TEXT, \"sql_value\" TEXT, FOREIGN KEY (userid) REFERENCES USERS(id) );"
                );
            SQLiteCommand command =
                new SQLiteCommand(commandText, connection);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
            } catch (Exception e)
            {
                
            }
            finally
            {
                connection.Close();
            }

            if (!Directory.Exists(currentDirectory + mainDirectoryName))
            {
                Directory.CreateDirectory(currentDirectory + mainDirectoryName);
            }
        }

        public void LoadConfig(string configfile)
        {
            Console.WriteLine("Load Config");
        }
        public void Deinitialization()
        {

        }

        public int MyMethod1(int a) { return a; }
        public int MyMethod2(int a) { return a; }
        public int MyMethod3(int a) { return a; }

        ~MyClass()
        {
            Deinitialization();
        }
    }
}
