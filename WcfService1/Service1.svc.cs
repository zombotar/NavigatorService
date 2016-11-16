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
        public static int OWNER_MASK = 48;
        public static int GROUP_MASK = 12;
        public static int OTHER_MASK = 3;

        public static int OWNER_READ = 32;
        public static int OWNER_WRITE = 16;
        public static int GROUP_READ = 8;
        public static int GROUP_WRITE = 4;
        public static int OTHER_READ = 2;
        public static int OTHER_WRITE = 1;

        public static int READ = 32;
        public static int WRITE = 16;
        public static int READ_AND_WRITE = READ | WRITE;

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

        private List<int> GetGroupIDs(int _idUser)
        {
            List<int> groupsIds = new List<int>();

            SQLiteConnection connection =
                new SQLiteConnection(string.Format("Data Source={0};", MyClass.Instance.currentDirectory + MyClass.Instance.databaseName));
            string commandText = String.Format("select gr.id from groups gr inner join user_to_group u_to_g on gr.id = u_to_g.groupid inner join users u on u.id = u_to_g.userid where u.id =  {0} ;", _idUser);
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
                    WriteToLogTable("unknown user with name = \"" + _username + "\" and passwordHash = \"" + _p_hash + "\" has provided unvalid certificate!");
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

        /*public List<File> GetAvailableListOfFiles(int _idUser)
        {
            List<File> files = new List<File>();
            List<int> groupIDs = GetGroupIDs(_idUser);

            return files;
        }*/

        public ObjectInfo IsObjectExists(int _id, bool _isDirectory)
        {
            ObjectInfo result = new ObjectInfo();
            result.isExists = false;
            result.id = _id;

            SQLiteConnection connection =
                new SQLiteConnection(string.Format("Data Source={0};", MyClass.Instance.currentDirectory + MyClass.Instance.databaseName));
            string commandText = String.Format("select filepath from FILE_META where id = {0} and directory_flag = {1};", _id, (_isDirectory == true ? 1 : 0));
            SQLiteCommand command =
                new SQLiteCommand(commandText, connection);
            string filepath = "";
            try
            {
                
                connection.Open();
                var reader1 = command.ExecuteReader();
                if (reader1.Read())
                {
                    filepath = reader1["filepath"].ToString();
                    result.filePath = filepath;
                    result.isExists = true;
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                connection.Close();
            }

            connection.Close();

            bool isExistOnFileSystem = false;
            if (_isDirectory)
            {
                isExistOnFileSystem = Directory.Exists(filepath);
            }
            else
            {
                FileInfo file = new FileInfo(filepath);
                isExistOnFileSystem = file.Exists;
            }

            result.isExists = (result.isExists && isExistOnFileSystem);

            return result;
        }

        public ObjectInfo IsObjectExists(string _path, bool _isDirectory)
        {
            ObjectInfo result = new ObjectInfo();
            result.isExists = false;
            result.filePath = _path;

            bool isExistOnFileSystem = false;
            if (_isDirectory)
            {
                isExistOnFileSystem = Directory.Exists(_path);
            } else
            {
                FileInfo file = new FileInfo(_path);
                isExistOnFileSystem = file.Exists;
            }

            SQLiteConnection connection = 
                new SQLiteConnection(string.Format("Data Source={0};", MyClass.Instance.currentDirectory + MyClass.Instance.databaseName));
            string commandText = String.Format("select id from FILE_META where filepath = \"{0}\" and directory_flag = {1};", _path, (_isDirectory == true ? 1 : 0));
            SQLiteCommand command =
                new SQLiteCommand(commandText, connection);
            try
            {
                int id = -1;
                connection.Open();
                var reader1 = command.ExecuteReader();
                if (reader1.Read())
                {
                    id = Int32.Parse(reader1["id"].ToString());
                    result.id = id;
                    result.isExists = (true && isExistOnFileSystem);
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                connection.Close();
            }

            connection.Close();
            return result;
        }

        public int CheckOwnerAccess(int _idUser, int _idObject)
        {
            SQLiteConnection connection =
                new SQLiteConnection(string.Format("Data Source={0};", MyClass.Instance.currentDirectory + MyClass.Instance.databaseName));
            string commandText = String.Format("select * from FILE_META where id = {0} and ownerid = {1}; ", _idObject, _idUser);
            SQLiteCommand command =
                new SQLiteCommand(commandText, connection);
            try
            {
                int access_bitset = 0;
                connection.Open();
                var reader1 = command.ExecuteReader();
                if (reader1.Read())
                {
                    access_bitset = Int32.Parse(reader1["access_bitset"].ToString());
                    return (access_bitset & 48); // owner only
                }
            }
            catch (Exception e)
            {
                return -1;
            }
            finally
            {
                connection.Close();
            }

            return -1;
        }

        public int CheckGroupAccess(int _idUser, int _idGroup, int _idObject)
        {
            SQLiteConnection connection =
                new SQLiteConnection(string.Format("Data Source={0};", MyClass.Instance.currentDirectory + MyClass.Instance.databaseName));
            string commandText = String.Format("select * from FILE_META where id = {0} and ifnull(groupid, -1) = {1}; ", _idObject, _idGroup);
            SQLiteCommand command =
                new SQLiteCommand(commandText, connection);
            try
            {
                int access_bitset = 0;
                connection.Open();
                var reader1 = command.ExecuteReader();
                if (reader1.Read())
                {
                    access_bitset = Int32.Parse(reader1["access_bitset"].ToString());
                    return (access_bitset & 12); // group only
                }
            }
            catch (Exception e)
            {
                return -1;
            }
            finally
            {
                connection.Close();
            }

            return -1;
        }

        public int CheckOtherAccess(int _idUser, int _idObject)
        {
            SQLiteConnection connection =
                new SQLiteConnection(string.Format("Data Source={0};", MyClass.Instance.currentDirectory + MyClass.Instance.databaseName));
            string commandText = String.Format("select * from FILE_META where id = {0} ; ", _idObject);
            SQLiteCommand command =
                new SQLiteCommand(commandText, connection);
            try
            {
                int access_bitset = 0;
                connection.Open();
                var reader1 = command.ExecuteReader();
                if (reader1.Read())
                {
                    access_bitset = Int32.Parse(reader1["access_bitset"].ToString());
                    return (access_bitset & OTHER_MASK); // other only
                }
            }
            catch (Exception e)
            {
                return -1;
            }
            finally
            {
                connection.Close();
            }

            return -1;
        }

        private BrowserDataInfo MakeDirsAndFilesList(string _path)
        {
            //1 - dirs
            // проверить по id членство в группах
            // забить флаги доступности (или проверить сразу)
            BrowserDataInfo result = new BrowserDataInfo();
            result.mDirectories = new List<MyDirectoryInfo>();
            string[] stringDirectories = Directory.GetDirectories(_path);

            //loop throught all directories
            foreach (string stringDir in stringDirectories)
            {
                MyDirectoryInfo metaNodeDir = new MyDirectoryInfo();
                DirectoryInfo systemNodeDir = new DirectoryInfo(stringDir);
                metaNodeDir.mDirectoryInfo = systemNodeDir;
                var objectMeta = IsObjectExists(stringDir, true);
                objectMeta.isDirectory = true;
                if (objectMeta.isExists)
                {
                    metaNodeDir.mMetaObject = objectMeta;
                    result.mDirectories.Add(metaNodeDir);
                }
            }

            result.mFiles = new List<MyFileInfo>();
            string[] stringFiles = Directory.GetFiles(_path);
            foreach(string stringFile in stringFiles)
            {
                MyFileInfo metaNodeFile = new MyFileInfo();
                FileInfo systemNodeFile = new FileInfo(stringFile);
                metaNodeFile.mFileInfo = systemNodeFile;
                var objectMeta = IsObjectExists(stringFile, false);
                objectMeta.isDirectory = false;
                if (objectMeta.isExists)
                {
                    metaNodeFile.mMetaObject = objectMeta;
                    result.mFiles.Add(metaNodeFile);
                }
            }

            return result;
        }

        public BrowserDataInfo AfterAuth(int _idUser)
        {
            var rootFilepath = MyClass.Instance.currentDirectory + MyClass.Instance.mainDirectoryName;
            var result = GetListOfData(_idUser, rootFilepath);
            result.rootPath = rootFilepath;
            return result;
        }

        public bool CheckACL(int _idUser, int _idObject, int _perms)
        {
            int permMask = _perms;
            if (permMask < 0 || permMask > 48)
                return false;
            bool access = false; // может более информативное возвращаемое значение?
            SQLiteConnection connection =
                new SQLiteConnection(string.Format("Data Source={0};", MyClass.Instance.currentDirectory + MyClass.Instance.databaseName));
            string commandText = String.Format("select * from FILE_META where id = {0};", _idObject);
            SQLiteCommand command =
                new SQLiteCommand(commandText, connection);
            try
            {
                connection.Open();
                var reader1 = command.ExecuteReader();
                if (reader1.Read())
                {
                    int resultAccess = Int32.Parse(reader1["access_bitset"].ToString());
                    // if user is owner
                    int ownerID = -1;
                    try
                    {
                        ownerID = Int32.Parse(reader1["ownerid"].ToString());
                    } catch (Exception e1)
                    {
                        ownerID = -1;
                    }
                    if (ownerID == _idUser)
                    {
                        resultAccess = resultAccess & permMask;
                        if (resultAccess != 0)
                        {
                            access = true;
                        }
                        return access;
                    }
                    // if user in group: owner
                    permMask = permMask >> 2;
                    var groupIds = GetGroupIDs(_idUser);
                    int objectGroupID = -1;
                    try
                    {
                        objectGroupID = Int32.Parse(reader1["groupid"].ToString());
                    }
                    catch (Exception e2)
                    {
                        objectGroupID = -1;
                    }
                    if (groupIds != null && objectGroupID != -1 && groupIds.Contains(objectGroupID))
                    {
                        resultAccess = resultAccess & permMask;
                        if (resultAccess != 0)
                        {
                            access = true;
                        }
                        return access;
                    }
                    // if user in group: other 
                    permMask = permMask >> 2;
                    resultAccess = resultAccess & permMask;
                    if (resultAccess != 0)
                    {
                        access = true;
                    }
                    return access;
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                connection.Close();
            }

            connection.Close();

            return false;

        }

        public BrowserDataInfo GetListOfData(int _idUser, string _path)
        {
            BrowserDataInfo result = new BrowserDataInfo();
            var objectInfo = IsObjectExists(_path, true);
            if (!objectInfo.isExists)
            {
                result.mErrCode = -1;
                result.mErrMessage = "Ошибка! Не существует каталога!";
                return result;
            }

            bool access = CheckACL(_idUser, objectInfo.id, READ);
            if (access)
            {
                result = MakeDirsAndFilesList(_path); 
                result.mErrCode = 0;
                return result;
            }
            

            return result;
        } 

        public AddFileResult AddFile(File file, int _idUser, int _idGroup, int _accessBitset)
        {
            // должна быть проверка, на существование такого же файла перед тем, как его добавить (дабы копии не плодить)
            // или предупреждать что-ли
            // а вот логическое И должно помочь в моменте
            // который касается того, что в 9 случаях разрешено, в одном - нет
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
                "CREATE TABLE IF NOT EXISTS \"FILE_META\" (\"id\" INTEGER PRIMARY KEY  AUTOINCREMENT NOT NULL, \"directory_flag\" INTEGER, \"filepath\" TEXT, \"filesize\" INTEGER, \"ownerid\" INTEGER, \"groupid\" INTEGER, \"access_bitset\" INTEGER NOT NULL, FOREIGN KEY (ownerid) REFERENCES USERS(id), FOREIGN KEY (groupid) REFERENCES GROUPS(id) );" +
                "CREATE TABLE IF NOT EXISTS \"LOG_TABLE\" (\"id\" INTEGER PRIMARY KEY AUTOINCREMENT, \"userid\" INTEGER NOT NULL, \"groupid\" INTEGER, \"date_time\" TEXT, \"value\" TEXT, \"sql_value\" TEXT, FOREIGN KEY (userid) REFERENCES USERS(id) );"
                );
            SQLiteCommand command =
                new SQLiteCommand(commandText, connection);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();

                string commandText2 = String.Format("select * from users; ");
                SQLiteCommand command1 =
                    new SQLiteCommand(commandText2, connection);
                var reader1 = command1.ExecuteReader();
                if (!reader1.Read())
                {
                    commandText2 = String.Format("insert into USERS (name, p_hash) values (\"admin\", \"21232f297a57a5a743894a0e4a801fc3\"); ");
                    SQLiteCommand command2 =
                    new SQLiteCommand(commandText2, connection);
                    command2.ExecuteNonQuery();
                }

                commandText2 = String.Format("select * from groups; ");
                SQLiteCommand command3 =
                    new SQLiteCommand(commandText2, connection);
                var reader2 = command3.ExecuteReader();
                if (!reader2.Read())
                {
                    commandText2 = String.Format("insert into GROUPS (groupname) values (\"admin\"); ");
                    SQLiteCommand command4 =
                    new SQLiteCommand(commandText2, connection);
                    command4.ExecuteNonQuery();
                }

                commandText2 = String.Format("select * from user_to_group; ");
                SQLiteCommand command5 =
                    new SQLiteCommand(commandText2, connection);
                var reader3 = command5.ExecuteReader();
                if (!reader3.Read())
                {
                    commandText2 = String.Format("insert into user_to_group (userid, groupid) values (1, 1); ");
                    SQLiteCommand command6 =
                    new SQLiteCommand(commandText2, connection);
                    command6.ExecuteNonQuery();
                }

                commandText2 = String.Format("select * from file_meta; ");
                SQLiteCommand command7 =
                    new SQLiteCommand(commandText2, connection);
                var reader4 = command7.ExecuteReader();
                if (!reader4.Read())
                {
                    commandText2 = String.Format("insert into FILE_META (directory_flag, filepath, filesize, ownerid, groupid, access_bitset) values (1, \"{0}\", 0, 1, null, 63); ", currentDirectory + mainDirectoryName);
                    SQLiteCommand command8 =
                    new SQLiteCommand(commandText2, connection);
                    command8.ExecuteNonQuery();
                }

                
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
