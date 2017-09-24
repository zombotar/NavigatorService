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
    public class NavigatorService : NavigatorIService
    {

        public void WriteToLogTable(string message)
        {
            string commandText = " insert into LOG_TABLE (Userid, Groupid, date_time, value) values (0, null, datetime('now'), \"{0}\"); ";
            commandText = String.Format(commandText, message);
            bool flag = false;
            if (SingletHelper.Instance.mConnection.State == System.Data.ConnectionState.Closed)
            {
                SingletHelper.Instance.mConnection.Open();
                flag = true;
            }
            using (SQLiteCommand command =
                new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
            {
                try
                {
                    var result = command.ExecuteNonQuery();
                }
                catch (Exception e)
                {

                }
                finally
                {
                    if (flag)
                    {
                        SingletHelper.Instance.mConnection.Close();
                    }
                }
            }
        }

        public void WriteToLogTable(string message, int idUser, int idGroup)
        {
            string commandText = "";
            if (idGroup > 0)
            {
                commandText = " insert into LOG_TABLE (Userid, Groupid, date_time, value) values ({0}, {1}, datetime('now'), \"{2}\"); ";
                commandText = String.Format(commandText, idUser, idGroup, message);
            } else
            {
                commandText = " insert into LOG_TABLE (Userid, Groupid, date_time, value) values ({0}, null, datetime('now'), \"{1}\"); ";
                commandText = String.Format(commandText, idUser, message);
            }
            bool flag = false;
            if (SingletHelper.Instance.mConnection.State == System.Data.ConnectionState.Closed)
            {
                SingletHelper.Instance.mConnection.Open();
                flag = true;
            }
            using (SQLiteCommand command =
                new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
            {
                try
                {
                    var result = command.ExecuteNonQuery();
                }
                catch (Exception e)
                {

                }
                finally
                {
                    if (flag)
                    {
                        SingletHelper.Instance.mConnection.Close();
                    }
                }
            } 
        }

        private int GetUserID(string username, string p_hash)
        {
            int id = -1;

            bool connectionFlag = false;
            string commandText = String.Format("select id from USERS where name = \"{0}\" and p_hash = \"{1}\" ;", username, p_hash);
            if (SingletHelper.Instance.mConnection.State == System.Data.ConnectionState.Closed)
            {
                SingletHelper.Instance.mConnection.Open();
                connectionFlag = true;
            }
            using (SQLiteCommand command =
                new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
            {
                try
                {
                    using (var result = command.ExecuteReader())
                    {
                        if (result.Read())
                            id = Int32.Parse(result["id"].ToString());
                    }
                }
                catch (Exception e)
                {
                    WriteToLogTable(e.Message + " <GetUserID> ", id, -1);
                }
                finally
                {
                    if (connectionFlag)
                    {
                        SingletHelper.Instance.mConnection.Close();
                    }
                }
            }

            return id;
        }

        private bool isExistsUser(int id)
        {
            string commandText = String.Format("select * from USERS where id = {0} ;", id);
            bool flag = false;
            bool connectionFlag = false;
            if (SingletHelper.Instance.mConnection.State == System.Data.ConnectionState.Closed)
            {
                SingletHelper.Instance.mConnection.Open();
                connectionFlag = true;
            }
            using (SQLiteCommand command =
                new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
            {
                try
                {
                    using (var result = command.ExecuteReader())
                    {
                        if (result.Read())
                            flag = true;
                        else
                            flag = false;
                    }
                }
                catch (Exception e)
                {
                    flag = false;
                    WriteToLogTable(e.Message + " <isExistUser> ", id, -1);
                }
                finally
                {
                    if (connectionFlag)
                    {
                        SingletHelper.Instance.mConnection.Close();
                    }
                }
            }

            return flag;
        }

        private bool isExistsGroup(int id)
        {
            string commandText = String.Format("select * from GROUPS where id = {0} ;", id);
            bool flag = false;

            bool connectionFlag = false;

            if (SingletHelper.Instance.mConnection.State == System.Data.ConnectionState.Closed)
            {
                SingletHelper.Instance.mConnection.Open();
                connectionFlag = true;
            }
            using (SQLiteCommand command =
                new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
            {
                try
                {
                    using (var result = command.ExecuteReader())
                    {
                        if (result.Read())
                            flag = true;
                        else
                            flag = false;
                    }
                }
                catch (Exception e)
                {
                    flag = false;
                    WriteToLogTable(e.Message + " <isExistGroup> ", 0, id);
                }
                finally
                {
                    if (connectionFlag)
                    {
                        SingletHelper.Instance.mConnection.Close();
                    }
                }
            }

            return flag;
        }

        private List<Group> GetGroupsForUser(int idUser)
        {
            List<Group> result = new List<Group>();

            string commandText = String.Format("select gr.id, gr.Groupname from GROUPS gr " +
                " inner join USER_TO_GROUP u_to_g on gr.id = u_to_g.Groupid " +
                " inner join USERS u on u.id = u_to_g.Userid where u.id =  {0} ;", idUser);
            bool flag = false;

            if (SingletHelper.Instance.mConnection.State == System.Data.ConnectionState.Closed)
            {
                SingletHelper.Instance.mConnection.Open();
                flag = true;
            }
            using (SQLiteCommand command =
                new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
            {
                try
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Group Group = new Group();
                            Group.mId = Int32.Parse(reader["id"].ToString());
                            Group.mName = reader["Groupname"].ToString();
                            result.Add(Group);
                        }
                    }
                }
                catch (Exception e)
                {
                    WriteToLogTable(e.Message + " <GetGroupsForUser> ", idUser, -1);
                }
                finally
                {
                    if (flag)
                    {
                        SingletHelper.Instance.mConnection.Close();
                    }
                }
            }

            return result;
        }

        private List<int> GetGroupIDs(int _idUser)
        {
            List<int> GroupIds = new List<int>();

            /*SQLiteConnection connection =
                new SQLiteConnection(string.Format("Data Source={0};", SingletHelper.Instance.currentDirectory + SingletHelper.Instance.databaseName));*/
            string commandText = String.Format("select gr.id from Groups gr inner join User_to_Group u_to_g on gr.id = u_to_g.Groupid inner join Users u on u.id = u_to_g.Userid where u.id =  {0} ;", _idUser);
            bool flag = false;
            if (SingletHelper.Instance.mConnection.State == System.Data.ConnectionState.Closed)
            {
                SingletHelper.Instance.mConnection.Open();
                flag = true;
            }
            using (SQLiteCommand command =
                new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
            {
                try
                {
                    int id = -1;
                    using (var result = command.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            if (!Int32.TryParse(result["id"].ToString(), out id))
                            {
                                id = -1;
                            }
                            else
                            {
                                GroupIds.Add(id);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    WriteToLogTable(e.Message + " <GetGroupIDs> ", _idUser, -1);
                }
                finally
                {
                    if (flag)
                    {
                        SingletHelper.Instance.mConnection.Close();
                    }
                }
            }

            return GroupIds;
        }

        private List<string> GetGroupNames(int _idUser)
        {
            List<string> GroupsNames = new List<string>();

            /*SQLiteConnection connection =
                new SQLiteConnection(string.Format("Data Source={0};", SingletHelper.Instance.currentDirectory + SingletHelper.Instance.databaseName));*/
            string commandText = String.Format("select gr.Groupname from Groups gr inner join User_to_Group u_to_g on gr.id = u_to_g.Groupid inner join Users u on u.id = u_to_g.Userid where u.id =  {0} ;", _idUser);
            bool flag = false;
            if (SingletHelper.Instance.mConnection.State == System.Data.ConnectionState.Closed)
            {
                SingletHelper.Instance.mConnection.Open();
                flag = true;
            }
            using (SQLiteCommand command =
                new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
            {
                try
                {
                    string Groupname = "";
                    using (var result = command.ExecuteReader())
                    {
                        while (result.Read())
                        {
                            Groupname = result["Groupname"].ToString();
                            GroupsNames.Add(Groupname);
                        }
                    }

                }
                catch (Exception e)
                {
                    WriteToLogTable(e.Message + " <GetGroupNames> ", _idUser, -1);
                }
                finally
                {
                    if (flag)
                    {
                        SingletHelper.Instance.mConnection.Close();
                    }
                }
            }

            return GroupsNames;
        }

        public bool CheckCert()
        {
            // Заглушка)
            return true;
        }

        public AuthenticationResult Authentication(string Username, string p_hash)
        {
            AuthenticationResult result = new AuthenticationResult();
            var x = SingletHelper.Instance;
            result.mUserId = GetUserID(Username, p_hash);
            bool isCertChecked = CheckCert();
            if (result.mUserId > 0 && isCertChecked)
            {
                result.mResult.setErrCode(0);
                result.mGroupNames = GetGroupNames(result.mUserId);
                WriteToLogTable("Успешная авторизация для пользователя <" + Username + "> ", result.mUserId, -1);
            } 
            if (result.mUserId < 0)
            {
                WriteToLogTable("User with name = " + Username + " and passwordHash = " + p_hash + " not found!", 0, -1);
                result.mResult.setErrMessage("Неверные имя пользователя / пароль!");
            }
            if (!isCertChecked)
            {
                if (result.mUserId < 0)
                {
                    WriteToLogTable("unknown User with name = " + Username + " and passwordHash = " + p_hash + " has provided unvalid certificate!");
                    result.mResult.setErrMessage("Неверные имя пользователя / пароль! Предоставлен невалидный сертификат!");
                }
                else
                {
                    var Grouplist = GetGroupIDs(result.mUserId);
                    if (Grouplist != null && Grouplist.Count > 0)
                    {
                        WriteToLogTable("User with name = " + Username + " and passwordHash = " + p_hash + " has provided unvalid certificate!", result.mUserId, Grouplist.ElementAt(0));
                        result.mResult.setErrMessage("Предоставлен невалидный сертификат!");
                    } else
                    {
                        WriteToLogTable("User with name = " + Username + " and passwordHash = " + p_hash + " has provided unvalid certificate!", result.mUserId, -1);
                        result.mResult.setErrMessage("Предоставлен невалидный сертификат!");
                    }
                }
            }
            return result;
        }

        public FilesystemObject IsObjectExists(int id, bool isDirectory)
        {
            FilesystemObject result = new FilesystemObject();
            result.setId(id);
            result.setDirFlag(isDirectory);

            string commandText = String.Format("select filepath from FilesystemObject where id = {0} and directory_flag = {1};", id, (isDirectory == true ? 1 : 0));
            string filepath = "";

            using (SQLiteCommand command =
                new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
            {
                try
                {

                    SingletHelper.Instance.mConnection.Open();
                    using (var reader1 = command.ExecuteReader())
                    {
                        if (reader1.Read())
                        {
                            filepath = reader1["filepath"].ToString();
                            result.setFilepath(filepath);
                            result.setExistsFlag(true);
                        }
                    }

                }
                catch (Exception e)
                {
                    WriteToLogTable(e.Message + " <IsObjectExist> ", 0, -1);
                }
                finally
                {
                    SingletHelper.Instance.mConnection.Close();
                }

                bool isExistOnFileSystem = false;
                if (isDirectory)
                {
                    isExistOnFileSystem = Directory.Exists(filepath);
                }
                else
                {
                    FileInfo file = new FileInfo(filepath);
                    isExistOnFileSystem = file.Exists;
                }

                result.setExistsFlag(result.getIsExistsFlag() && isExistOnFileSystem);
            }

            return result;
        }

        public static string DirectoryCutter(string filepath, string hidePart)
        {
            return filepath.Replace(hidePart, "");
        }

        public FilesystemObject IsObjectExists(string path, bool isDirectory, int idUser)
        {
            FilesystemObject result = new FilesystemObject();
            result.setExistsFlag(false);
            result.setFilepath(path.Replace(SingletHelper.Instance.currentDirectory, ""));

            bool isExistOnFileSystem = false;
            if (isDirectory)
            {
                isExistOnFileSystem = Directory.Exists(path);
            } else
            {
                FileInfo file = new FileInfo(path);
                isExistOnFileSystem = file.Exists;
            }

            bool adminFlag = false;
            var Groups = GetGroupNames(idUser);
            if (Groups.Contains("admin"))
            {
                adminFlag = true;
            }

            string commandText = String.Format("select id, filesize, ownerid, Groupid, access_bitset from FilesystemObject where filepath = \"{0}\" and directory_flag = {1};", path, (isDirectory == true ? 1 : 0));
            bool flag = false;
            if (SingletHelper.Instance.mConnection.State == System.Data.ConnectionState.Closed)
            {
                SingletHelper.Instance.mConnection.Open();
                flag = true;
            }
            using (SQLiteCommand command =
                new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
            {
                try
                {
                    int id = -1;
                    int filesize = 0;

                    int ownerID = -1;
                    int GroupID = -1;
                    int access_bitset = 0;
                    
                    using (var reader1 = command.ExecuteReader())
                    {
                        if (reader1.Read())
                        {
                            id = Int32.Parse(reader1["id"].ToString());
                            filesize = Int32.Parse(reader1["filesize"].ToString());
                            result.setFilesize(filesize);
                            result.setId(id);
                            result.setExistsFlag(isExistOnFileSystem);
                            result.setDirFlag(isDirectory);

                            if (adminFlag)
                            {
                                if (!Int32.TryParse(reader1["ownerid"].ToString(), out ownerID))
                                {
                                    ownerID = -1;
                                }
                                if (!Int32.TryParse(reader1["Groupid"].ToString(), out GroupID))
                                {
                                    GroupID = -1;
                                }
                                if (!Int32.TryParse(reader1["access_bitset"].ToString(), out access_bitset))
                                {
                                    access_bitset = 0;
                                }

                                result.setUid(ownerID);
                                result.setGid(GroupID);
                                result.setAccessBitset(access_bitset);
                            }
                        }
                        else
                        {
                            result.setExistsFlag(false);
                            result.setId(-1);
                        }
                    }
                }
                catch (Exception e)
                {
                    WriteToLogTable(e.Message + " <IsObjectExist> ", idUser, -1);
                }
                finally
                {
                    if (flag)
                    {
                        SingletHelper.Instance.mConnection.Close();
                    }
                }
            }
            
            return result;
        }

        private BrowserDataResult MakeDirsAndFilesList(string path, int idUser)
        {
            //1 - dirs
            // проверить по id членство в группах
            // забить флаги доступности (или проверить сразу)
            BrowserDataResult result = new BrowserDataResult();
            result.mDirectories = new List<FilesystemObject>();
            string[] stringDirectories = Directory.GetDirectories(path);

            //loop throught all directories
            foreach (string stringDir in stringDirectories)
            {
                var objectMeta = IsObjectExists(stringDir, true, idUser);
                if (objectMeta.getIsExistsFlag())
                {
                    result.mDirectories.Add(objectMeta);
                }
            }

            result.mFiles = new List<FilesystemObject>();
            string[] stringFiles = Directory.GetFiles(path);
            foreach(string stringFile in stringFiles)
            {
                var objectMeta = IsObjectExists(stringFile, false, idUser);
                if (objectMeta.getIsExistsFlag())
                {
                    result.mFiles.Add(objectMeta);
                }
            }

            return result;
        }

        public BrowserDataResult GetRootDirForUser(int idUser)
        {
            var rootFilepath = SingletHelper.Instance.currentDirectory + SingletHelper.Instance.mainDirectoryName;
            var result = GetListOfData(idUser, rootFilepath);
            result.rootPath = rootFilepath;
            return result;
        }

        public bool CheckACL(int _idUser, int _idObject, int _perms)
        {
            int permMask = _perms;
            if (permMask < 0 || permMask > 48)
            {
                WriteToLogTable("Некорректная битовая строка! " + " <CheckACL> ", _idUser, -1);
                return false;
            }
            bool access = false; // может более информативное возвращаемое значение?
            /*SQLiteConnection connection =
                new SQLiteConnection(string.Format("Data Source={0};", SingletHelper.Instance.currentDirectory + SingletHelper.Instance.databaseName));*/
            string commandText = String.Format("select * from FilesystemObject where id = {0};", _idObject);
            bool flag = false;
            if (SingletHelper.Instance.mConnection.State == System.Data.ConnectionState.Closed)
            {
                SingletHelper.Instance.mConnection.Open();
                flag = true;
            }
            using (SQLiteCommand command =
                new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
            {
                try
                {
                    using (var reader1 = command.ExecuteReader())
                    {
                        if (reader1.Read())
                        {
                            int resultAccess = Int32.Parse(reader1["access_bitset"].ToString());
                            // if User is owner
                            int ownerID = -1;
                            try
                            {
                                ownerID = Int32.Parse(reader1["ownerid"].ToString());
                            }
                            catch (Exception e1)
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
                                SingletHelper.Instance.mConnection.Close();
                                return access;
                            }
                            // if User in Group: owner
                            permMask = permMask >> 2;
                            var GroupIds = GetGroupIDs(_idUser);
                            int objectGroupID = -1;
                            try
                            {
                                objectGroupID = Int32.Parse(reader1["Groupid"].ToString());
                            }
                            catch (Exception e2)
                            {
                                objectGroupID = -1;
                            }
                            if (GroupIds != null && objectGroupID != -1 && GroupIds.Contains(objectGroupID))
                            {
                                resultAccess = resultAccess & permMask;
                                if (resultAccess != 0)
                                {
                                    access = true;
                                }
                                SingletHelper.Instance.mConnection.Close();
                                return access;
                            }
                            // if User in Group: other 
                            permMask = permMask >> 2;
                            resultAccess = resultAccess & permMask;
                            if (resultAccess != 0)
                            {
                                access = true;
                            }
                            SingletHelper.Instance.mConnection.Close();
                            return access;
                        }
                    }

                }
                catch (Exception e)
                {

                }
                finally
                {
                    if (flag)
                    {
                        SingletHelper.Instance.mConnection.Close();
                    }
                }
            }

            WriteToLogTable("<" + _perms.ToString() + "> " + "Доступ к ресурсу {" + _idObject.ToString() + "} для пользователя c id = " + _idUser.ToString() + " запрещён!" + " <CheckACL> ", _idUser, -1);
            return false;

        }

        public string UpParentFolder(String path)
        {
            if (path != null)
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                DirectoryInfo parentDir = dir.Parent;
                if (parentDir != null)
                {
                    return parentDir.FullName;
                }
            }
            return null;
        }

        public BrowserDataResult GetListOfData(int idUser, string path)
        {
            if (!path.Contains(SingletHelper.Instance.currentDirectory))
            {
                path = SingletHelper.Instance.currentDirectory + path;
            }
            BrowserDataResult result = null; 
            var objectInfo = IsObjectExists(path, true, idUser);
            if (!objectInfo.getIsExistsFlag())
            {
                result = new BrowserDataResult();
                result.currPath = path.Replace(SingletHelper.Instance.currentDirectory, "");
                string root = "";
                if (path.Equals(SingletHelper.Instance.currentDirectory + SingletHelper.Instance.mainDirectoryName)){
                    root = path.Replace(SingletHelper.Instance.currentDirectory, "");
                } else
                {
                    root = UpParentFolder(path).Replace(SingletHelper.Instance.currentDirectory, "");
                }
                result.rootPath = (root != null ? root : path.Replace(SingletHelper.Instance.currentDirectory, ""));
                result.mResult.setErrCode(-1).setErrMessage("Ошибка! Не существует каталога!");
                return result;
            }
            bool access = CheckACL(idUser, objectInfo.getId(), FilesystemObject.READ);
            if (access)
            {
                result = MakeDirsAndFilesList(path, idUser);
                result.mResult.setErrCode(0);
                result.currPath = path.Replace(SingletHelper.Instance.currentDirectory, "");
                string root = "";
                if (path.Equals(SingletHelper.Instance.currentDirectory + SingletHelper.Instance.mainDirectoryName))
                {
                    root = path.Replace(SingletHelper.Instance.currentDirectory, "");
                }
                else
                {
                    root = UpParentFolder(path).Replace(SingletHelper.Instance.currentDirectory, "");
                }
                result.rootPath = (root != null ? root : path.Replace(SingletHelper.Instance.currentDirectory, ""));
                /*result.currPath = _path;
                string root = "";
                if (_path.Equals(SingletHelper.Instance.currentDirectory + SingletHelper.Instance.mainDirectoryName)){
                    root = _path;
                }
                else
                {
                    root = UpParentFolder(_path);
                }
                result.rootPath = (root != null ? root : _path);*/
                //return result;
            }
            else
            {
                result.mResult.setErrCode(-2).setErrMessage("В доступе отказано!");
                WriteToLogTable("В доступе к (" + path + " ) отказано!", idUser, -1);
                result.currPath = path.Replace(SingletHelper.Instance.currentDirectory, "");
                string root = "";
                if (path.Equals(SingletHelper.Instance.currentDirectory + SingletHelper.Instance.mainDirectoryName))
                {
                    root = path.Replace(SingletHelper.Instance.currentDirectory, "");
                }
                else
                {
                    root = UpParentFolder(path).Replace(SingletHelper.Instance.currentDirectory, "");
                }
                result.rootPath = (root != null ? root : path.Replace(SingletHelper.Instance.currentDirectory, ""));
                //return result;
            }
            

            return result;
        }

        public bool WalkDirectoryTree(int idUser, System.IO.DirectoryInfo root, ref List<FilesystemObject> objToDeleteList)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;

            bool result = true;


            files = root.GetFiles("*.*");

            if (files != null)
            {
                foreach (System.IO.FileInfo fi in files)
                {
                    var obj1 = IsObjectExists(fi.FullName, false, idUser);
                    if (obj1.getIsExistsFlag())
                    {
                        bool flag = CheckACL(idUser, obj1.getId(), FilesystemObject.WRITE);
                        if (flag)
                        {
                            objToDeleteList.Add(obj1);
                        }
                        else
                        {
                            objToDeleteList.Clear();
                            return false;
                        }
                    }
                }

                subDirs = root.GetDirectories();
                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    result = result && WalkDirectoryTree(idUser, dirInfo, ref objToDeleteList);
                    if (result == false)
                    {
                        {
                            objToDeleteList.Clear();
                            return false;
                        }
                    }
                }
            }

            var obj2 = IsObjectExists(root.FullName, true, idUser);
            if (obj2.getIsExistsFlag())
            {
                bool flag = CheckACL(idUser, obj2.getId(), FilesystemObject.WRITE);
                if (flag)
                {
                    objToDeleteList.Add(obj2);
                }
                else
                {
                    objToDeleteList.Clear();
                    return false;
                }
            }

            return result;
        }

        public DownloadFileResult DownloadFile(DownloadFilesystemRequest req)
        {
            /*FilesystemObject obj = req.obj;
            int idUser = req.idUser;*/
            /*FilesystemObject obj = reqObj;
            int idUser = reqObj.getUid();*/
            int idUser = req.mUserId;
            DownloadFileResult result = new DownloadFileResult();
            string path = req.mFilename;
            if (!path.Contains(SingletHelper.Instance.currentDirectory))
            {
                path = SingletHelper.Instance.currentDirectory + path;
            }
            var objectInfo = IsObjectExists(path, false, idUser);
            if (!objectInfo.getIsExistsFlag())
            {
                result.mResult.setErrMessage("Не существует такого объекта!");
                return result;
            }
            result.mOrigFilesize = objectInfo.getFilesize();
            bool access = CheckACL(idUser, objectInfo.getId(), FilesystemObject.READ);
            if (access)
            {
                try
                {
                    System.IO.FileInfo file = new System.IO.FileInfo(SingletHelper.Instance.currentDirectory + "\\" + objectInfo.getFilepath());
                    FileStream fs = new FileStream(SingletHelper.Instance.currentDirectory + "\\" + objectInfo.getFilepath(), FileMode.Open, FileAccess.Read);
                    result.mResult.setErrCode(0);
                    result.mFileByteStream = fs;
                    result.mObj = objectInfo;

                    return result;
                } catch (Exception e)
                {
                    result.mResult.setErrCode(-5).setErrMessage(e.Message);
                    return result;
                }

                //.mData.setBaseClassValues(objectInfo);
                
            }
            else
            {
                result.mFileByteStream = new MemoryStream();
                result.mResult.setErrCode(-2).setErrMessage("В доступе отказано!");
                WriteToLogTable("В доступе к (" + objectInfo.getFilepath() + ") отказано!", idUser, -1);
            }

            return result;
        }

        public ManFilesystemObjResult DeleteObject(UserNostreamOperationRequest req)
        {
            FilesystemObject obj = req.obj;
            int idUser = req.idUser;
            ManFilesystemObjResult result = new ManFilesystemObjResult();
            string path = obj.getFilepath();
            if (!path.Contains(SingletHelper.Instance.currentDirectory))
            {
                path = SingletHelper.Instance.currentDirectory + path;
            }
            var objectInfo = IsObjectExists(path, obj.getDirFlag(), idUser);
            if (!objectInfo.getIsExistsFlag())
            {
                result.mResult.setErrCode(-1).setErrMessage("Не существует такого объекта!");
                return result;
            }
            bool access = CheckACL(idUser, objectInfo.getId(), FilesystemObject.WRITE);
            if (access)
            {
                if (objectInfo.getDirFlag())
                {
                    List<FilesystemObject> objToDelete = new List<FilesystemObject>();
                    DirectoryInfo root = new DirectoryInfo(path);
                    bool flag = WalkDirectoryTree(idUser, root, ref objToDelete);
                    if (flag)
                    {
                        if (objToDelete != null)
                        {
                            foreach (FilesystemObject o in objToDelete)
                            {
                                DeleteRowObject(idUser, o.getId());
                            }
                        }
                        root.Delete(true);
                        result.mResult.setErrCode(0);
                    }
                    else
                    {
                        result.mResult.setErrCode(-1).setErrMessage("В доступе отказано!");
                        WriteToLogTable("Невозможно удалить директорию (" + objectInfo.getFilepath() + ")! Недостаточно прав на совершение операции!", idUser, -1);
                    }
                }
                else
                {
                    FileInfo file = new FileInfo(path);
                    var obj1 = IsObjectExists(file.FullName, false, idUser);
                    if (obj1.getIsExistsFlag())
                    {
                        bool flag = CheckACL(idUser, obj1.getId(), FilesystemObject.WRITE);
                        if (flag)
                        {
                            DeleteRowObject(idUser, obj1.getId());
                            file.Delete();
                            result.mResult.setErrCode(0);
                        }
                        else
                        {
                            result.mResult.setErrCode(-1).setErrMessage("В доступе отказано!");
                            WriteToLogTable("Невозможно удалить файл (" + objectInfo.getFilepath() + ")! Недостаточно прав на совершение операции!", idUser, -1);
                        }
                    }
                    else
                    {
                        result.mResult.setErrCode(-2).setErrMessage("Не найден файл!");
                    }
                }
            }
            else
            {
                result.mResult.setErrCode(-1).setErrMessage("В доступе отказано!");
            }

            return result;
        }

        public bool DeleteRowObject(int idUser, int idObject)
        {
            bool result = false;

            string commandText = String.Format("delete from FilesystemObject where id = {0};", idObject);
            bool connectionFlag = false;
            if (SingletHelper.Instance.mConnection.State == System.Data.ConnectionState.Closed)
            {
                SingletHelper.Instance.mConnection.Open();
                connectionFlag = true;
            }
            using (SQLiteCommand command =
                new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
            {
                try
                {
                    int rowCount = command.ExecuteNonQuery();
                    result = true;
                }
                catch (Exception e)
                {

                }
                finally
                {
                    if (connectionFlag)
                    {
                        SingletHelper.Instance.mConnection.Close();
                    }
                }
            }

            if (result)
            {
                WriteToLogTable("Удаление объекта с id = " + idObject.ToString(), idUser, -1);
            }
            return result;
        }

        public bool DeleteRowObject(int idUser, string path)
        {
            bool result = false;

            string commandText = String.Format("delete from FilesystemObject where filepath = \"{0}\";", path);
            bool connectionFlag = false;
            if (SingletHelper.Instance.mConnection.State == System.Data.ConnectionState.Closed)
            {
                SingletHelper.Instance.mConnection.Open();
                connectionFlag = true;
            }
            using (SQLiteCommand command =
                new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
            {
                try
                {
                    int rowCount = command.ExecuteNonQuery();
                    result = true;
                }
                catch (Exception e)
                {

                }
                finally
                {
                    if (connectionFlag)
                    {
                        SingletHelper.Instance.mConnection.Close();
                    }
                }
            }

            if (result)
            {
                WriteToLogTable("Удаление объекта (" + path + ")", idUser, -1);
            }

            return result;
        }

        public ManFilesystemObjResult AddDirectory(UserNostreamOperationRequest req)
        {
            string pathToCurrDir = req.currDir;
            int idUser = req.idUser;
            int idGroup = req.idGroup;
            int accessBitset = req.accessBitset;
            string newDirName = req.newDirName;
            ManFilesystemObjResult result = new ManFilesystemObjResult();
            if (!pathToCurrDir.Contains(SingletHelper.Instance.currentDirectory))
            {
                pathToCurrDir = SingletHelper.Instance.currentDirectory + pathToCurrDir;
            }
            var objectInfo = IsObjectExists(pathToCurrDir, true, idUser);
            if (!objectInfo.getIsExistsFlag())
            {
                result.mResult.setErrCode(-1).setErrMessage("Не существует такой директории, в которую вы бы хотели добавить новую!");
                return result;
            }
            bool access = CheckACL(idUser, objectInfo.getId(), FilesystemObject.WRITE);
            if (access)
            {
                if (isExistsUser(idUser))
                {
                    int idDirMeta = -1;

                    string commandText = "";
                    string commandText2 = "";
                    string errCommandText = "";
                    errCommandText = " insert into LOG_TABLE (Userid, Groupid, date_time, value) values ({0}, {1}, datetime('now'), \"error when add new directory: {2}, cause {3} \"); ";
                    if (idGroup != -1)
                    {
                        commandText = String.Format("insert into FilesystemObject (directory_flag, filepath, filesize, ownerid, Groupid, access_bitset) values (1, \"{0}\", {1}, {2}, {3}, {4} ); ", pathToCurrDir + "\\" + newDirName, 0, idUser, idGroup, accessBitset);
                        commandText2 = String.Format("insert into LOG_TABLE (Userid, Groupid, date_time, value) values ({0}, {1}, datetime('now'), \"add new directory: {2} \"); ", idUser, idGroup, pathToCurrDir + "\\" + newDirName);
                        //, _idUser, _idGroup, SingletHelper.Instance.currentDirectory + SingletHelper.Instance.mainDirectoryName + file.FILENAME;
                    }
                    else
                    {
                        commandText = String.Format("insert into FilesystemObject (directory_flag, filepath, filesize, ownerid, Groupid, access_bitset) values (1, \"{0}\", {1}, {2}, {3}, {4} ); ", pathToCurrDir + "\\" + newDirName, 0, idUser, "null", accessBitset);
                        commandText2 = String.Format("insert into LOG_TABLE (Userid, Groupid, date_time, value) values ({0}, {1}, datetime('now'), \"add new directory: {2} \"); ", idUser, "null", pathToCurrDir + "\\" + newDirName);
                    }

                    try
                    {
                        var objInfo2 = IsObjectExists(pathToCurrDir + "\\" + newDirName, true, idUser);
                        if (!Directory.Exists(pathToCurrDir + "\\" + newDirName) && !objInfo2.getIsExistsFlag())
                        {
                            Directory.CreateDirectory(pathToCurrDir + "\\" + newDirName);
                        } else
                        {
                            result.mResult.setErrCode(-2).setErrMessage("Данная директория уже существует!");
                            if (idGroup != -1)
                            {
                                errCommandText = String.Format(errCommandText, idUser, idGroup, pathToCurrDir + "\\" + newDirName, "Попытка создания уже существующей директории, idDir = " + objInfo2.getId().ToString());
                            }
                            else
                            {
                                errCommandText = String.Format(errCommandText, idUser, "null", pathToCurrDir + "\\" + newDirName, "Попытка создания уже существующей директории, idDir = " + objInfo2.getId().ToString());
                            }
                            bool connectionFlag3 = false;
                            try
                            {
                                /*SQLiteConnection connection =
                                    new SQLiteConnection(string.Format("Data Source={0};", SingletHelper.Instance.currentDirectory + SingletHelper.Instance.databaseName));*/
                                if (SingletHelper.Instance.mConnection.State == System.Data.ConnectionState.Closed)
                                {
                                    SingletHelper.Instance.mConnection.Open();
                                    connectionFlag3 = true;
                                }
                                using (SQLiteCommand command =
                                    new SQLiteCommand(errCommandText, SingletHelper.Instance.mConnection))
                                {
                                    command.ExecuteNonQuery();
                                }  
                            }
                            catch (Exception ee)
                            {
                                result.mResult.setErrMessage(result.mResult.getErrMessage() + "\nОшибка при записи в лог!");
                            }
                            finally
                            {
                                if (connectionFlag3)
                                {
                                    SingletHelper.Instance.mConnection.Close();
                                }
                            }
                            return result;
                        }
                    }
                    catch (Exception e)
                    {
                        result.mResult.setErrMessage("Ошибка при создании новой директории!");
                        if (idGroup != -1)
                        {
                            errCommandText = String.Format(errCommandText, idUser, idGroup, pathToCurrDir + "\\" + newDirName, e.Message);
                        }
                        else
                        {
                            errCommandText = String.Format(errCommandText, idUser, "null", pathToCurrDir + "\\" + newDirName, e.Message);
                        }
                        bool connectionFlag2 = false;
                        try
                        {
                            /*SQLiteConnection connection =
                                new SQLiteConnection(string.Format("Data Source={0};", SingletHelper.Instance.currentDirectory + SingletHelper.Instance.databaseName));*/
                            if (SingletHelper.Instance.mConnection.State == System.Data.ConnectionState.Closed)
                            {
                                SingletHelper.Instance.mConnection.Open();
                                connectionFlag2 = true;
                            }
                            using (SQLiteCommand command =
                                new SQLiteCommand(errCommandText, SingletHelper.Instance.mConnection))
                            {
                                command.ExecuteNonQuery();
                            } 
                        }
                        catch (Exception ee)
                        {
                            result.mResult.setErrMessage(result.mResult.getErrMessage() + "\nОшибка при записи в лог!");
                        }
                        finally
                        {
                            if (connectionFlag2)
                            {
                                SingletHelper.Instance.mConnection.Close();
                            }
                        }
                        return result;
                    }

                    bool connectionFlag = false;
                    try
                    {
                        /*SQLiteConnection connection2 =
                            new SQLiteConnection(string.Format("Data Source={0};", SingletHelper.Instance.currentDirectory + SingletHelper.Instance.databaseName));*/
                        //connection.Close();
                        if (SingletHelper.Instance.mConnection.State == System.Data.ConnectionState.Closed)
                        {
                            SingletHelper.Instance.mConnection.Open();
                            connectionFlag = true;
                        }
                        using (SQLiteCommand command2 =
                            new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
                        {
                            command2.ExecuteNonQuery();
                            
                        }

                        using (SQLiteCommand command3 = new SQLiteCommand("select last_insert_rowid() as id;", SingletHelper.Instance.mConnection))
                        {
                            ;
                            using (var resultCommand = command3.ExecuteReader())
                            {
                                if (resultCommand.Read())
                                {
                                    idDirMeta = Int32.Parse(resultCommand["id"].ToString());
                                }
                            }
                        }

                        using (SQLiteCommand command3 =
                                    new SQLiteCommand(commandText2, SingletHelper.Instance.mConnection))
                        {
                            command3.ExecuteNonQuery();
                        }

                    }
                    catch (Exception e)
                    {
                        result.mResult.setErrMessage("Ошибка при вставке в ACL таблицу!");
                        SingletHelper.Instance.mConnection.Close();
                        return result;
                    }
                    finally
                    {
                        if (connectionFlag)
                        {
                            SingletHelper.Instance.mConnection.Close();
                        }
                    }

                    result.fileId = idDirMeta;
                    result.mResult.setErrCode(0);
                    return result;
                }

                result.mResult.setErrMessage("Не найден пользователь с таким id!");
            }
            else
            {
                result.mResult.setErrCode(-1).setErrMessage("В доступе отказано!");
                WriteToLogTable("Недостаточно прав для добавления новой директории (" + pathToCurrDir + "\\" + newDirName + ")!", idUser, -1);
            }

            SingletHelper.Instance.mConnection.Close();
            return result;
        }

        public ManFilesystemObjResult AddFile(RemoteFileInfo file)
        {
            int idUser,  idGroup,  accessBitset;
            idUser = file.mMetaFile.getUid();
            idGroup = file.mMetaFile.getGid();
            accessBitset = file.mMetaFile.getAccessBitset();

            ManFilesystemObjResult result = new ManFilesystemObjResult();

            string path = file.mMetaFile.getFilepath();
            if (!path.Contains(SingletHelper.Instance.currentDirectory))
            {
                path = SingletHelper.Instance.currentDirectory + path;
            }
            FileInfo fileInfo = new FileInfo(path);
            path = fileInfo.DirectoryName;
            var objectInfo = IsObjectExists(path, true, idUser);
            if (!objectInfo.getIsExistsFlag())
            {
                result.mResult.setErrCode(-1).setErrMessage("Не существует такой директории, в которую вы бы хотели добавить файл!");
                return result;
            }
            bool access = CheckACL(idUser, objectInfo.getId(), FilesystemObject.WRITE);
            if (access)
            {
                int idFileMeta = -1;

                if (isExistsUser(idUser))
                {
                    string commandText = "";
                    string commandText2 = "";
                    string errCommandText = "";
                    errCommandText = " insert into LOG_TABLE (Userid, Groupid, date_time, value) values ({0}, {1}, datetime('now'), \"error when add new file: {2}, cause {3} \"); ";
                    if (idGroup != -1)
                    {
                        commandText = String.Format("insert into FilesystemObject (directory_flag, filepath, filesize, ownerid, Groupid, access_bitset) values (0, \"{0}\", {1}, {2}, {3}, {4} ); ", 
                            SingletHelper.Instance.currentDirectory + file.mMetaFile.getFilepath(), file.mMetaFile.getFilesize(), 
                            idUser, idGroup, accessBitset);
                        commandText2 = String.Format("insert into LOG_TABLE (Userid, Groupid, date_time, value) values ({0}, {1}, datetime('now'), \"add new file: {2} \"); ", 
                            idUser, idGroup, SingletHelper.Instance.currentDirectory + file.mMetaFile.getFilepath());
                        //, _idUser, _idGroup, SingletHelper.Instance.currentDirectory + SingletHelper.Instance.mainDirectoryName + file.FILENAME;
                    }
                    else
                    {
                        commandText = String.Format("insert into FilesystemObject (directory_flag, filepath, filesize, ownerid, Groupid, access_bitset) values (0, \"{0}\", {1}, {2}, {3}, {4} ); ", 
                            SingletHelper.Instance.currentDirectory + file.mMetaFile.getFilepath(), file.mMetaFile.getFilesize(), 
                            idUser, "null", accessBitset);
                        commandText2 = String.Format("insert into LOG_TABLE (Userid, Groupid, date_time, value) values ({0}, {1}, datetime('now'), \"add new file: {2} \"); ", 
                            idUser, "null", SingletHelper.Instance.currentDirectory + file.mMetaFile.getFilepath());
                    }

                    try
                    {
                        DeleteRowObject(idUser, SingletHelper.Instance.currentDirectory + file.mMetaFile.getFilepath());
                        if (file.mFileByteStream != null)
                        {
                            FileStream targetStream = null;
                            using (targetStream = new FileStream(SingletHelper.Instance.currentDirectory + file.mMetaFile.getFilepath(),
                                FileMode.Create, FileAccess.Write, FileShare.None))
                            {
                                const int bufferLen = 65000;
                                byte[] buffer = new byte[bufferLen];
                                int count = 0;
                                while ((count = file.mFileByteStream.Read(buffer, 0, bufferLen)) > 0)
                                {
                                    // save to output stream
                                    targetStream.Write(buffer, 0, count);
                                }
                                targetStream.Close();
                                file.mFileByteStream.Close();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        result.mResult.setErrCode(-2).setErrMessage("Ошибка при записи файла!");
                        if (idGroup != -1)
                        {
                            errCommandText = String.Format(errCommandText, idUser, idGroup, 
                                SingletHelper.Instance.currentDirectory + file.mMetaFile.getFilepath(), e.Message);
                        }
                        else
                        {
                            errCommandText = String.Format(errCommandText, idUser, null,
                                SingletHelper.Instance.currentDirectory + file.mMetaFile.getFilepath(), e.Message);
                        }
                        try
                        {
                            using (SQLiteCommand command =
                                new SQLiteCommand(errCommandText, SingletHelper.Instance.mConnection))
                            {
                                SingletHelper.Instance.mConnection.Open();
                                command.ExecuteNonQuery();
                            }
                        }
                        catch (Exception ee)
                        {
                            result.mResult.setErrMessage(result.mResult.getErrMessage() + "\nОшибка при записи в лог!");
                        }
                        finally
                        {
                            SingletHelper.Instance.mConnection.Close();
                        }
                        return result;
                    }

                    bool connectionFlag = false;
                    try
                    {
                        if (SingletHelper.Instance.mConnection.State == System.Data.ConnectionState.Closed)
                        {
                            SingletHelper.Instance.mConnection.Open();
                            connectionFlag = true;
                        }
                        using (SQLiteCommand command2 =
                            new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
                        {
                            command2.ExecuteNonQuery();
                        }
                        
                        //connection.Close();
                        //connection.Open();

                        using (SQLiteCommand command3 = new SQLiteCommand("select last_insert_rowid() as id;", SingletHelper.Instance.mConnection))
                        {
                            using (var resultCommand = command3.ExecuteReader())
                            {
                                if (resultCommand.Read())
                                {
                                    idFileMeta = Int32.Parse(resultCommand["id"].ToString());
                                }
                            }
                        }



                        using (SQLiteCommand command4 =
                                new SQLiteCommand(commandText2, SingletHelper.Instance.mConnection))
                        {
                            command4.ExecuteNonQuery();
                        }     

                    }
                    catch (Exception e)
                    {
                        result.mResult.setErrCode(-3).setErrMessage("Ошибка при вставке в ACL таблицу!");
                        SingletHelper.Instance.mConnection.Close();
                        return result;
                    }
                    finally
                    {
                        if (connectionFlag)
                        {
                            SingletHelper.Instance.mConnection.Close();
                        }
                    }

                    result.fileId = idFileMeta;
                    result.mResult.setErrCode(0);
                    return result;
                }

                result.mResult.setErrCode(-4).setErrMessage("Не найден пользователь с таким id!");
                return result;
            }
            else
            {
                result.mResult.setErrMessage("В доступе отказано!");
                WriteToLogTable("Недостаточно прав для добавления нового файла (" + path + ")!", idUser, -1);
            }

            SingletHelper.Instance.mConnection.Close();
            return result;
            
        }


        ////// Admin

        public GetGroupsResult GetGroups(int idUser)
        {
            GetGroupsResult result = new GetGroupsResult();

            var Groupnames = GetGroupNames(idUser);
            if (Groupnames.Contains("admin"))
            {
                string commandText = String.Format("select id, Groupname from Groups");
                
                using (SQLiteCommand command =
                    new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
                {
                    try
                    {
                        SingletHelper.Instance.mConnection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Group Group = new Group();
                                Group.mId = Int32.Parse(reader["id"].ToString());
                                Group.mName = reader["Groupname"].ToString();
                                result.mGroups.Add(Group);
                            }
                        }
                        result.mResult.setErrCode(0);
                    }
                    catch (Exception e)
                    {

                    }
                    finally
                    {
                        SingletHelper.Instance.mConnection.Close();
                    }
                }
            }
            else
            {
                result.mResult.setErrMessage("Недостаточно прав для совершения подобной операции!");
                WriteToLogTable("Недостаточно прав для совершения операции <GetGroups>", idUser, -1);
            }

            return result;
        }

        public GetUsersResult GetUsers(int idUser)
        {
            GetUsersResult result = new GetUsersResult();

            var Groupnames = GetGroupNames(idUser);
            if (Groupnames.Contains("admin"))
            {
                string commandText = String.Format("select * from Users;");
                
                using (SQLiteCommand command =
                    new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
                {
                    try
                    {
                        SingletHelper.Instance.mConnection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int idCurrUser = -1;
                                string nameCurrUser = "";
                                idCurrUser = Int32.Parse(reader["id"].ToString());
                                nameCurrUser = reader["name"].ToString();
                                User currUser = new User();
                                currUser.mId = idCurrUser;
                                currUser.mName = nameCurrUser;
                                var Grouplist = GetGroupsForUser(idCurrUser);
                                currUser.Groups = Grouplist;

                                result.mUsers.Add(currUser);
                            }
                        }
                        result.mResult.setErrCode(0);
                    }
                    catch (Exception e)
                    {
                        result.mResult.setErrMessage(e.Message);
                    }
                    finally
                    {
                        SingletHelper.Instance.mConnection.Close();
                    }
                }
            }
            else
            {
                result.mResult.setErrCode(-2).setErrMessage("Недостаточно прав для совершения подобной операции!");
                WriteToLogTable("Недостаточно прав для совершения операции <GetUsers>", idUser, -1);
            }

            return result;
        }

        public OperationResult AddUser(int idUser, string username, string password_hash)
        {
            OperationResult res = new OperationResult();
            string errMessage = "";
            var Groupnames = GetGroupNames(idUser);
            if (Groupnames.Contains("admin"))
            {
                string commandText = String.Format("select * from Users where name = \"{0}\" ;", username);
                
                using (SQLiteCommand command =
                    new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
                {
                    try
                    {
                        SingletHelper.Instance.mConnection.Open();
                        using (var result = command.ExecuteReader())
                        {
                            if (result.Read())
                            {
                                errMessage = "Пользователь с таким именем уже существует!";
                                res.setErrMessage(errMessage);
                                //WriteToLogTable
                            }
                            else
                            {
                                commandText = String.Format("insert into Users (name, p_hash) values (\"{0}\", \"{1}\");", username, password_hash);
                                using (SQLiteCommand command2 = new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
                                {
                                    command2.ExecuteNonQuery();
                                }
                                
                                SingletHelper.Instance.mConnection.Close();
                                WriteToLogTable("Добавлен новый пользователь <" + username + ">", idUser, -1);
                                res.setErrCode(0);
                                return res;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        errMessage = e.Message;
                    }
                    finally
                    {
                        SingletHelper.Instance.mConnection.Close();
                    }
                }

            }
            else
            {
                errMessage = "Недостаточно прав для совершения подобной операции!";
                res.setErrMessage(errMessage);
                WriteToLogTable("Недостаточно прав для совершения операции <AddUser>", idUser, -1);
            }

            return res;
        }

        public OperationResult DeleteUser(int idUser, int idUserToDelete)
        {
            OperationResult res = new OperationResult();
            string errMessage = "";
            var Groupnames = GetGroupNames(idUser);
            if (Groupnames.Contains("admin"))
            {
                string commandText = String.Format("delete from Users where id = {0} ;", idUserToDelete);
                
                using (SQLiteCommand command =
                    new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
                {
                    try
                    {
                        SingletHelper.Instance.mConnection.Open();
                        int executorResult = command.ExecuteNonQuery();
                        WriteToLogTable("Удалён пользователь с id = " + idUserToDelete.ToString(), idUser, -1);
                        res.setErrCode(0);
                    }
                    catch (Exception e)
                    {
                        errMessage = e.Message;
                    }
                    finally
                    {
                        SingletHelper.Instance.mConnection.Close();
                    }
                }
                
            }
            else
            {
                errMessage = "Недостаточно прав для совершения подобной операции!";
                res.setErrMessage(errMessage);
                WriteToLogTable("Недостаточно прав для совершения операции <DeleteUser>", idUser, -1);
            }

            return res;
        }

        public OperationResult AddGroup(int idUser, string groupname)
        {
            OperationResult res = new OperationResult();
            string errMessage = "";
            var Groupnames = GetGroupNames(idUser);
            if (Groupnames.Contains("admin"))
            {
                string commandText = String.Format("select * from Groups where groupname = \"{0}\" ;", groupname);
                
                using (SQLiteCommand command =
                    new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
                {
                    try
                    {
                        SingletHelper.Instance.mConnection.Open();
                        using (var result = command.ExecuteReader())
                        {
                            if (result.Read())
                            {
                                errMessage = "Группа с таким именем уже существует!";
                                res.setErrCode(-2).setErrMessage(errMessage);
                                //WriteToLogTable
                            }
                            else
                            {
                                commandText = String.Format("insert into GroupS (Groupname) values (\"{0}\");", groupname);
                                
                                using (SQLiteCommand command2 = new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
                                {
                                    command2.ExecuteNonQuery();
                                }
                                
                                SingletHelper.Instance.mConnection.Close();

                                WriteToLogTable("Добавлена новая группа <" + groupname + ">", idUser, -1);
                                res.setErrCode(0);
                                return res;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        errMessage = e.Message;
                    }
                    finally
                    {
                        SingletHelper.Instance.mConnection.Close();
                    }
                }

                
            }
            else
            {
                errMessage = "Недостаточно прав для совершения подобной операции!";
                res.setErrMessage(errMessage);
                WriteToLogTable("Недостаточно прав для совершения операции <AddGroup>", idUser, -1);
            }

            return res;
        }

        public OperationResult DeleteGroup(int idUser, int idGroupToDelete)
        {
            OperationResult res = new OperationResult();
            var Groupnames = GetGroupNames(idUser);
            if (Groupnames.Contains("admin"))
            {
                string commandText = String.Format("delete from Groups where id = {0} ;", idGroupToDelete);
                
                using (SQLiteCommand command =
                    new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
                {
                    try
                    {
                        SingletHelper.Instance.mConnection.Open();
                        int executorResult = command.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        res.setErrMessage(e.Message);
                    }
                    finally
                    {
                        SingletHelper.Instance.mConnection.Close();
                    }
                }

                WriteToLogTable("Удалёна группа с id = " + idGroupToDelete.ToString(), idUser, -1);
                res.setErrCode(0);

            }
            else
            {
                res.setErrMessage("Недостаточно прав для совершения подобной операции!");
                WriteToLogTable("Недостаточно прав для совершения операции <DeleteGroup>", idUser, -1);
            }

            return res;
        }

        public OperationResult AddUserToGroup(int idUser, int targetUserID, int targetGroupID)
        {
            OperationResult res = new OperationResult();

            var Groupnames = GetGroupNames(idUser);
            if (Groupnames.Contains("admin"))
            {

                if (isExistsUser(targetUserID))
                {
                    if (isExistsGroup(targetGroupID))
                    {
                        var GroupIDs = GetGroupIDs(targetUserID);
                        if (GroupIDs.Contains(targetGroupID))
                        {
                            res.setErrMessage("Пользователь является членом этой группы!");
                        }
                        else
                        {
                            string commandText = String.Format("insert into User_TO_Group(Userid, Groupid) values ({0}, {1});", 
                                targetUserID, targetGroupID);
                            
                            using (SQLiteCommand command =
                                new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
                            {
                                try
                                {
                                    SingletHelper.Instance.mConnection.Open();
                                    int executorResult = command.ExecuteNonQuery();
                                }
                                catch (Exception e)
                                {
                                    res.setErrCode(-2).setErrMessage(e.Message);
                                }
                                finally
                                {
                                    SingletHelper.Instance.mConnection.Close();
                                }
                            }

                            res.setErrCode(0);
                            WriteToLogTable("Пользователь с id = " + targetUserID.ToString() + " добавлен в группу с id = " + targetGroupID.ToString(), idUser, -1);
                        }
                    }
                    else
                    {
                        res.setErrCode(-3).setErrMessage(String.Format("Не найдена группа с id = {0}", targetGroupID));
                    }
                }
                else
                {
                    res.setErrCode(-4).setErrMessage(String.Format("Не найден пользователь с id = {0}", targetUserID));
                }

            }
            else
            {
                res.setErrMessage("Недостаточно прав для совершения подобной операции!");
                WriteToLogTable("Недостаточно прав для совершения операции <AddUserToGroup>", idUser, -1);
            }

            return res;
        }

        public OperationResult DeleteUserFromGroup(int idUser, int targetUserID, int targetGroupID)
        {
            OperationResult res = new OperationResult();

            var Groupnames = GetGroupNames(idUser);
            if (Groupnames.Contains("admin"))
            {

                if (isExistsUser(targetUserID))
                {
                    if (isExistsGroup(targetGroupID))
                    {
                        string commandText = String.Format("delete from User_TO_Group where Userid = {0} and Groupid = {1};", targetUserID, targetGroupID);
                        
                        using (SQLiteCommand command =
                            new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
                        {
                            try
                            {
                                SingletHelper.Instance.mConnection.Open();
                                int executorResult = command.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                res.setErrMessage(e.Message);
                            }
                            finally
                            {
                                SingletHelper.Instance.mConnection.Close();
                            }
                        }

                        res.setErrCode(0);
                        WriteToLogTable("Пользователь с id = " + targetUserID.ToString() + " исключён из группы с id = " + targetGroupID.ToString(), idUser, -1);
                    }
                    else
                    {
                        res.setErrMessage(String.Format("Не найдена группа с id = {0}", targetGroupID));
                    }
                }
                else
                {
                    res.setErrMessage(String.Format("Не найден пользователь с id = {0}", targetUserID));
                }

            }
            else
            {
                res.setErrMessage("Недостаточно прав для совершения подобной операции!");
                WriteToLogTable("Недостаточно прав для совершения операции <DeleteUserFromGroup>", idUser, -1);
            }

            return res;
        }

        public OperationResult ChangeAccess(int idUser, int idObject, int newAccessBitset, int newOwnerID, int newGroupID)
        {
            OperationResult res = new OperationResult();
            if (!isExistsUser(newOwnerID))
            {
                res.setErrMessage("Некорректное значение ownerid!");
                return res;
            }

            var Groupnames = GetGroupNames(idUser);

            if (Groupnames.Contains("admin"))
            {

                if (newAccessBitset >= 0 && newAccessBitset <= FilesystemObject.ROOT)
                {
                    string commandText = "";
                    if (isExistsGroup(newGroupID))
                    {
                        commandText = String.Format("update FilesystemObject set access_bitset = {0}, ownerid = {1}, Groupid = {2} where id = {3};", 
                            newAccessBitset, newOwnerID, newGroupID, idObject);
                    }
                    else if (newGroupID == -1)
                    {
                        commandText = String.Format("update FilesystemObject set access_bitset = {0}, ownerid = {1}, Groupid = null where id = {2};", 
                            newAccessBitset, newOwnerID, idObject);
                    }
                    else
                    {
                        res.setErrMessage("Некорректное значение Groupid!");
                        return res;
                    }

                    using (SQLiteCommand command =
                        new SQLiteCommand(commandText, SingletHelper.Instance.mConnection))
                    {
                        try
                        {
                            SingletHelper.Instance.mConnection.Open();
                            int executorResult = command.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            res.setErrMessage(e.Message);
                        }
                        finally
                        {
                            SingletHelper.Instance.mConnection.Close();
                        }
                    }

                    res.setErrCode(0).setErrMessage("Изменены права доступа к объекту");
                    WriteToLogTable("Изменены права доступа к объекту с id = " + idObject.ToString() + ". Новое значение access_bitset = " + newAccessBitset.ToString(), idUser, -1);
                }
                else
                {
                    res.setErrMessage("Некорректная битовая строка!");
                }

            }
            else
            {
                res.setErrMessage("Недостаточно прав для совершения подобной операции!");
                WriteToLogTable("Недостаточно прав для совершения операции <ChangeAccessBitset>", idUser, -1);
            }

            return res;
        }
    }

    public class SingletHelper
    {
        public string mainDirectoryName = "fileStorage";
        public string databaseName = "database.db";
        public string currentDirectory = "";
        public SQLiteConnection mConnection;
        

        static object _locker = new object();
        static SingletHelper _instance;

        private SingletHelper()
        {
        }

        public static SingletHelper Instance
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

                    _instance = new SingletHelper();

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
                "CREATE TABLE IF NOT EXISTS \"Users\" (\"id\" INTEGER PRIMARY KEY  AUTOINCREMENT  NOT NULL , \"name\" TEXT NOT NULL , \"p_hash\" BLOB NOT NULL); " +
                "CREATE TABLE IF NOT EXISTS \"Groups\" (\"id\" INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, \"Groupname\" TEXT NOT NULL); " +
                "CREATE TABLE IF NOT EXISTS \"User_TO_Group\" (\"Userid\" INTEGER, \"Groupid\" INTEGER, PRIMARY KEY (Userid, Groupid), FOREIGN KEY (Userid) REFERENCES Users(id), FOREIGN KEY (Groupid) REFERENCES Groups(id) ); " +
                "CREATE TABLE IF NOT EXISTS \"FilesystemObject\" (\"id\" INTEGER PRIMARY KEY  AUTOINCREMENT NOT NULL, \"directory_flag\" INTEGER, \"filepath\" TEXT, \"filesize\" INTEGER, \"ownerid\" INTEGER, \"Groupid\" INTEGER, \"access_bitset\" INTEGER NOT NULL, FOREIGN KEY (ownerid) REFERENCES Users(id), FOREIGN KEY (Groupid) REFERENCES Groups(id) );" +
                "CREATE TABLE IF NOT EXISTS \"LOG_TABLE\" (\"id\" INTEGER PRIMARY KEY AUTOINCREMENT, \"Userid\" INTEGER NOT NULL, \"Groupid\" INTEGER, \"date_time\" TEXT, \"value\" TEXT, \"sql_value\" TEXT, FOREIGN KEY (Userid) REFERENCES UserS(id) );"
                );
            SQLiteCommand command =
                new SQLiteCommand(commandText, connection);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();

                string commandText2 = String.Format("select * from Users; ");
                SQLiteCommand command1 =
                    new SQLiteCommand(commandText2, connection);
                using (var reader1 = command1.ExecuteReader())
                {
                    if (!reader1.Read())
                    {
                        commandText2 = String.Format("insert into Users (name, p_hash) values (\"admin\", \"21232f297a57a5a743894a0e4a801fc3\"); ");
                        SQLiteCommand command2 =
                        new SQLiteCommand(commandText2, connection);
                        command2.ExecuteNonQuery();
                    }
                }

                commandText2 = String.Format("select * from Groups; ");
                SQLiteCommand command3 =
                    new SQLiteCommand(commandText2, connection);
                using (var reader2 = command3.ExecuteReader())
                {
                    if (!reader2.Read())
                    {
                        commandText2 = String.Format("insert into Groups (Groupname) values (\"admin\"); ");
                        SQLiteCommand command4 =
                        new SQLiteCommand(commandText2, connection);
                        command4.ExecuteNonQuery();
                    }
                }

                commandText2 = String.Format("select * from User_to_Group; ");
                SQLiteCommand command5 =
                    new SQLiteCommand(commandText2, connection);
                using (var reader3 = command5.ExecuteReader())
                {
                    if (!reader3.Read())
                    {
                        commandText2 = String.Format("insert into User_to_Group (Userid, Groupid) values (1, 1); ");
                        SQLiteCommand command6 =
                        new SQLiteCommand(commandText2, connection);
                        command6.ExecuteNonQuery();
                    }
                }

                commandText2 = String.Format("select * from FilesystemObject; ");
                SQLiteCommand command7 =
                    new SQLiteCommand(commandText2, connection);
                using (var reader4 = command7.ExecuteReader())
                {
                    if (!reader4.Read())
                    {
                        commandText2 = String.Format("insert into FilesystemObject (directory_flag, filepath, filesize, ownerid, Groupid, access_bitset) values (1, \"{0}\", 0, 1, null, 63); ", currentDirectory + mainDirectoryName);
                        SQLiteCommand command8 =
                        new SQLiteCommand(commandText2, connection);
                        command8.ExecuteNonQuery();
                    }
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

            mConnection = new SQLiteConnection(string.Format("Data Source={0};", SingletHelper.Instance.currentDirectory + SingletHelper.Instance.databaseName));
        }

        public void Deinitialization()
        {
            mConnection.Close();
        }

        ~SingletHelper()
        {
            Deinitialization();
        }
    }
}
