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

        public void WriteToLogTable(string _message)
        {
            SQLiteConnection connection =
                new SQLiteConnection(string.Format("Data Source={0};", MyClass.Instance.currentDirectory + MyClass.Instance.databaseName));
            string commandText = " insert into LOG_TABLE (userid, groupid, date_time, value) values (0, null, datetime('now'), \"{0}\"); ";
            commandText = String.Format(commandText, _message);
            SQLiteCommand command =
                new SQLiteCommand(commandText, connection);

            try
            {
                connection.Open();
                var result = command.ExecuteNonQuery();
            }
            catch (Exception e)
            {

            }
            finally
            {
                connection.Close();
            }
        }

        public void WriteToLogTable(string _message, int _idUser, int _idGroup)
        {
            SQLiteConnection connection =
                new SQLiteConnection(string.Format("Data Source={0};", MyClass.Instance.currentDirectory + MyClass.Instance.databaseName));
            string commandText = "";
            if (_idGroup > 0)
            {
                commandText = " insert into LOG_TABLE (userid, groupid, date_time, value) values ({0}, {1}, datetime('now'), \"{2}\"); ";
                commandText = String.Format(commandText, _idUser, _idGroup, _message);
            } else
            {
                commandText = " insert into LOG_TABLE (userid, groupid, date_time, value) values ({0}, null, datetime('now'), \"{1}\"); ";
                commandText = String.Format(commandText, _idUser, _message);
            }
            
            SQLiteCommand command =
                new SQLiteCommand(commandText, connection);

            try
            {
                connection.Open();
                var result = command.ExecuteNonQuery();
            }
            catch (Exception e)
            {

            }
            finally
            {
                connection.Close();
            }
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

        private bool isExistsUser(int _id)
        {
            SQLiteConnection connection =
                new SQLiteConnection(string.Format("Data Source={0};", MyClass.Instance.currentDirectory + MyClass.Instance.databaseName));
            string commandText = String.Format("select * from USERS where id = {0} ;", _id);
            SQLiteCommand command =
                new SQLiteCommand(commandText, connection);

            try
            {
                connection.Open();
                var result = command.ExecuteReader();
                if (result.Read())
                    return true;
                else
                    return false;
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
                connection.Close();
            }
        }

        private List<int> GetGroupIDs(int _id)
        {
            List<int> groupsIds = new List<int>();

            SQLiteConnection connection =
                new SQLiteConnection(string.Format("Data Source={0};", MyClass.Instance.currentDirectory + MyClass.Instance.databaseName));
            string commandText = String.Format("select gr.id from groups gr inner join user_to_group u_to_g on gr.id = u_to_g.groupid inner join users u on u.id = u_to_g.userid where u.id =  {1} ;", _id);
            SQLiteCommand command =
                new SQLiteCommand(commandText, connection);

            try
            {
                int id = -1;
                connection.Open();
                var result = command.ExecuteReader();
                while (result.Read())
                {
                    id = Int32.Parse(result["id"].ToString());
                    groupsIds.Add(id);
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                connection.Close();
            }

            return groupsIds;
        }

        public bool CheckCert()
        {
            return true;
        }

        public AuthenticationResult Authentication(string _username, string _p_hash)
        {
            AuthenticationResult result = new AuthenticationResult();
            result.resultCode = -1;
            var x = MyClass.Instance;
            result.userId = GetUserID(_username, _p_hash);
            bool isCertChecked = CheckCert();
            if (result.userId > 0 && isCertChecked)
            {
                result.resultCode = 0;
            } 
            if (result.userId < 0)
            {
                WriteToLogTable("user with name = \"" + _username + "\" and passwordHash = \"" + _p_hash + "\" not found!");
            }
            if (!isCertChecked)
            {
                if (result.userId < 0)
                {
                    WriteToLogTable("user with name = \"" + _username + "\" and passwordHash = \"" + _p_hash + "\" has provided unvalid certificate!");
                }
                else
                {
                    var grouplist = GetGroupIDs(result.userId);
                    if (grouplist != null && grouplist.Count > 0)
                    {
                        WriteToLogTable("user with name = \"" + _username + "\" and passwordHash = \"" + _p_hash + "\" has provided unvalid certificate!", result.userId, grouplist.ElementAt(0));
                    } else
                    {
                        WriteToLogTable("user with name = \"" + _username + "\" and passwordHash = \"" + _p_hash + "\" has provided unvalid certificate!", result.userId, -1);
                    }
                }
            }
            return result;
        }

        public List<File> GetAvailableListOfFiles(int _id)
        {
            List<File> files = new List<File>();

            SQLiteConnection connection =
                new SQLiteConnection(string.Format("Data Source={0};", MyClass.Instance.currentDirectory + MyClass.Instance.databaseName));
            string commandText = String.Format("select gr.id from groups gr inner join user_to_group u_to_g on gr.id = u_to_g.groupid inner join users u on u.id = u_to_g.userid where u.id =  {1} ;", _id);
            SQLiteCommand command =
                new SQLiteCommand(commandText, connection);

            try
            {
                int id = -1;
                connection.Open();
                var result = command.ExecuteReader();
                while (result.Read())
                {
                    id = Int32.Parse(result["id"].ToString());
                    //groupsIds.Add(id);
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                connection.Close();
            }

            return files;
        }

        public AddFileResult AddFile(File file, int _idUser, int _idGroup, int _accessBitset)
        {
            AddFileResult result = new AddFileResult();
            result.errMessage = "";
            result.resultCode = -1;
            int idFileMeta = -1;

            if (isExistsUser(_idUser))
            {
                SQLiteConnection connection =
                    new SQLiteConnection(string.Format("Data Source={0};", MyClass.Instance.currentDirectory + MyClass.Instance.databaseName));
                string commandText = "";
                string commandText2 = "";
                string errCommandText = "";
                errCommandText = " insert into LOG_TABLE (userid, groupid, date_time, value) values ({0}, {1}, datetime('now'), \"error when add new file: {2}, cause {3} \"); ";
                if (_idGroup != -1)
                {
                    commandText = String.Format(" insert into FILE_META (filepath, filesize, ownerid, groupid, access_bitset) values (\"{0}\", {1}, {2}, {3}, {4} ); select last_insert_rowid() as id; ", MyClass.Instance.currentDirectory + MyClass.Instance.mainDirectoryName + file.FILENAME, file.FILESIZE, _idUser, _idGroup, _accessBitset);
                    commandText2 = String.Format(" insert into LOG_TABLE (userid, groupid, date_time, value) values ({0}, {1}, datetime('now'), \"add new file: {2} \"); ", _idUser, _idGroup, MyClass.Instance.currentDirectory + MyClass.Instance.mainDirectoryName + file.FILENAME);
                     //, _idUser, _idGroup, MyClass.Instance.currentDirectory + MyClass.Instance.mainDirectoryName + file.FILENAME;
                }
                else
                {
                    commandText = String.Format(" insert into FILE_META (filepath, filesize, ownerid, groupid, access_bitset) values (\"{0}\", {1}, {2}, \"{3}\", {4} ); select last_insert_rowid() as id; ", MyClass.Instance.currentDirectory + MyClass.Instance.mainDirectoryName + file.FILENAME, file.FILESIZE, _idUser, "null", _accessBitset);
                    commandText2 = String.Format(" insert into LOG_TABLE (userid, groupid, date_time, value) values ({0}, \"{1}\", datetime('now'), \"add new file: {2} \"); ", _idUser, "null", MyClass.Instance.currentDirectory + MyClass.Instance.mainDirectoryName + file.FILENAME);
                }

                try
                {
                    FileStream newFile = System.IO.File.Create(MyClass.Instance.currentDirectory + MyClass.Instance.mainDirectoryName + file.FILENAME);
                    byte[] secretKey = Crypto.GenerateKey(MyClass.Instance.currentDirectory + MyClass.Instance.mainDirectoryName + Path.GetFileNameWithoutExtension(file.FILENAME) + ".key");
                    byte[] encryptFile = null;
                    Crypto.AES_Encrypt(file.DATA, ref encryptFile, secretKey);
                    newFile.Write(encryptFile, 0, file.FILESIZE);
                    newFile.Flush();
                    newFile.Close();
                } catch (Exception e)
                {
                    result.errMessage = "Ошибка при записи файла!";
                    if (_idGroup != -1)
                    {
                        errCommandText = String.Format(errCommandText, _idUser, _idGroup, MyClass.Instance.currentDirectory + MyClass.Instance.mainDirectoryName + file.FILENAME, e.Message);
                    } else
                    {
                        errCommandText = String.Format(errCommandText, _idUser, "null", MyClass.Instance.currentDirectory + MyClass.Instance.mainDirectoryName + file.FILENAME, e.Message);
                    }
                    try
                    {
                        SQLiteCommand command =
                        new SQLiteCommand(errCommandText, connection);
                        connection.Open();
                        command.ExecuteNonQuery();
                    } catch (Exception ee)
                    {
                        result.errMessage += "\nОшибка при записи в лог!";
                    }
                    return result;
                }

                try
                {
                    SQLiteCommand command =
                        new SQLiteCommand(commandText, connection);
                    connection.Open();
                    var resultComand = command.ExecuteReader();
                    if (resultComand.Read())
                    {
                        idFileMeta = Int32.Parse(resultComand["id"].ToString());
                    }

                    command =
                        new SQLiteCommand(commandText2, connection);
                    command.ExecuteNonQuery();

                } catch (Exception e)
                {
                    result.errMessage = "Ошибка при вставке в ACL таблицу!";
                    return result;
                }
                finally
                {
                    connection.Close();
                }

                result.fileID = idFileMeta;
                result.resultCode = 0;
                return result;
            }

            result.errMessage = "Не найден пользователь с таким id!";
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
