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
    public interface NavigatorIService
    {

        [OperationContract]
        AuthenticationResult Authentication(string username, string p_hash);

        [OperationContract]
        BrowserDataResult GetRootDirForUser(int idUser);

        [OperationContract]
        BrowserDataResult GetListOfData(int idUser, string path);

        [OperationContract]
        ManFilesystemObjResult AddFile(RemoteFileInfo files);

        [OperationContract]
        ManFilesystemObjResult AddDirectory(UserNostreamOperationRequest req);
    
        [OperationContract]
        ManFilesystemObjResult DeleteObject(UserNostreamOperationRequest req);

        /*[OperationContract]
        DownloadFileResult DownloadFile(UserOperationRequest req);*/
        /*[OperationContract]
        DownloadFileResult DownloadFile(FilesystemObject reqObj);*/
        [OperationContract]
        DownloadFileResult DownloadFile(DownloadFilesystemRequest req);


        // Admin
        [OperationContract]
        OperationResult AddUser(int idUser, string Username, string password_hash);

        [OperationContract]
        OperationResult DeleteUser(int idUser, int idUserToDelete);

        [OperationContract]
        GetUsersResult GetUsers(int idUser);

        [OperationContract]
        GetGroupsResult GetGroups(int idUser);

        [OperationContract]
        OperationResult AddGroup(int idUser, string groupname);

        [OperationContract]
        OperationResult DeleteGroup(int idUser, int idGroup);

        [OperationContract]
        OperationResult AddUserToGroup(int idUser, int targetUserID, int targetGroupID);

        [OperationContract]
        OperationResult DeleteUserFromGroup(int idUser, int targetUserID, int targetGroupID);

        [OperationContract]
        OperationResult ChangeAccess(int idUser, int idObject, int newAccessBitset, int newOwnerID, int newGroupID);

        // TODO: Добавьте здесь операции служб

    }


    // Используйте контракт данных, как показано в примере ниже, чтобы добавить составные типы к операциям служб.

    [MessageContract]
    public class DownloadFilesystemRequest
    {
        [MessageBodyMember]
        public string mFilename { get; set; }

        [MessageBodyMember]
        public int mUserId { get; set; }
    }

    [MessageContract]
    public class FilesystemObject
    {
        [MessageBodyMember]
        public static int OWNER_MASK = 48;
        [MessageBodyMember]
        public static int GROUP_MASK = 12;
        [MessageBodyMember]
        public static int OTHER_MASK = 3;
        [MessageBodyMember]
        public static int OWNER_READ = 32;
        [MessageBodyMember]
        public static int OWNER_WRITE = 16;
        [MessageBodyMember]
        public static int GROUP_READ = 8;
        [MessageBodyMember]
        public static int GROUP_WRITE = 4;
        [MessageBodyMember]
        public static int OTHER_READ = 2;
        [MessageBodyMember]
        public static int OTHER_WRITE = 1;
        [MessageBodyMember]
        public static int ROOT = OWNER_MASK | GROUP_MASK | OTHER_MASK;
        [MessageBodyMember]
        public static int READ = 32;
        [MessageBodyMember]
        public static int WRITE = 16;
        [MessageBodyMember]
        public static int READ_AND_WRITE = READ | WRITE;

        [MessageBodyMember]
        public int mId = -1;
        [MessageBodyMember]
        public string mFilepath = "";
        [MessageBodyMember]
        public string mObjName { get { return  mFilepath != null && mFilepath.Length > 1 ? mFilepath.Substring(mFilepath.LastIndexOf('\\') + 1) : ""; } set { } }
        [MessageBodyMember]
        public bool mIsDirFlag = false;
        [MessageBodyMember]
        public bool mIsExistsFlag = false;
        [MessageBodyMember]
        public int mFilesize = 0;

        // for admin only
        [MessageBodyMember]
        public int mUid = -1;
        [MessageBodyMember]
        public int mGid = -1;
        [MessageBodyMember]
        public int mAccessBitset = 0;

        

        public FilesystemObject setId(int id) { mId = id; return this; }
        public FilesystemObject setFilepath(string filepath) { mFilepath = filepath; return this; }
        public FilesystemObject setDirFlag(bool isDir) { mIsDirFlag = isDir; return this; }
        public FilesystemObject setExistsFlag(bool isExists) { mIsExistsFlag = isExists; return this; }
        public FilesystemObject setFilesize(int filesize)
        {
            if (mIsDirFlag)
                return this;
            mFilesize = filesize;
            return this;
        }
        public FilesystemObject setUid(int uid) { mUid = uid; return this; }
        public FilesystemObject setGid(int gid) { mGid = gid; return this; }
        public FilesystemObject setAccessBitset(int bitset)
        {
            if (bitset < 0 || bitset > ROOT)
                return this;
            mAccessBitset = bitset;
            return this;
        }

        public int getId() { return mId; }
        public string getFilepath() { return mFilepath; }
        public bool getDirFlag() { return mIsDirFlag; }
        public bool getIsExistsFlag() { return mIsExistsFlag; }
        public int getFilesize() { return mFilesize; }
        public int getUid() { return mUid; }
        public int getGid() { return mGid; }
        public int getAccessBitset() { return mAccessBitset; }

        public static FilesystemObject CreateStockFile()
        {
            FilesystemObject f = new FilesystemObject();
            return f;
        }

        public static FilesystemObject CreateStockDir()
        {
            FilesystemObject d = new FilesystemObject();
            d.setDirFlag(true);
            return d;
        }

        public static FilesystemObject CreateDir(DirectoryInfo dirInfo, FilesystemObject fsObj = null)
        {
            FilesystemObject d = null;
            if (fsObj == null)
            {
                d = new FilesystemObject();
                d.setDirFlag(true);
                d.setExistsFlag(dirInfo.Exists);
                d.setFilepath(dirInfo.FullName);
                d.setFilesize(0);
            }
            else
            {
                d = fsObj;
            }
            return d;
        }
    }

    [DataContract]
    public class OperationResult
    {
        [DataMember]
        private int mErrCode = -1;
        [DataMember]
        private string mErrMessage = "";

        public OperationResult setErrCode(int code) { mErrCode = code; return this; }
        public OperationResult setErrMessage(string message) { mErrMessage = message; return this; }

        public int getErrCode() { return mErrCode; }
        public string getErrMessage() { return mErrMessage; }

    }

    [DataContract]
    public class AuthenticationResult
    {
        [DataMember]
        public OperationResult mResult { get; set; }

        [DataMember]
        public int mUserId { get; set; }

        [DataMember]
        public List<string> mGroupNames { get; set; }

        public AuthenticationResult()
        {
            mUserId = -1;
            mGroupNames = new List<string>();
            mResult = new OperationResult();
        }
    }

    [DataContract]
    public class GetUsersResult
    {
        [DataMember]
        public List<User> mUsers { get; set; }

        [DataMember]
        public OperationResult mResult { get; set; }

        public GetUsersResult()
        {
            mUsers = new List<User>();
            mResult = new OperationResult();
        }
    }

    public class GetGroupsResult
    {
        [DataMember]
        public List<Group> mGroups { get; set; }

        [DataMember]
        public OperationResult mResult { get; set; }

        public GetGroupsResult()
        {
            mGroups = new List<Group>();
            mResult = new OperationResult();
        }
    }

    [DataContract]
    public class User
    {
        [DataMember]
        public int mId { get; set; }

        [DataMember]
        public string mName { get; set; }

        [DataMember]
        public string mPasswordHash { get; set; }

        [DataMember]
        public List<Group> Groups { get; set; } 

        public User()
        {
            mId = -1;
            mName = "";
            mPasswordHash = "";
            Groups = new List<Group>();
        }
    }

    [DataContract]
    public class Group
    {
        [DataMember]
        public int mId { get; set; }

        [DataMember]
        public string mName { get; set; }

        public Group()
        {
            mId = -1;
            mName = "";
        }

    }

    [MessageContract]
    public class UserNostreamOperationRequest
    {
        [MessageBodyMember]
        public FilesystemObject obj;

        [MessageBodyMember]
        public int idUser;

        [MessageBodyMember]
        public string currDir;

        [MessageBodyMember]
        public string newDirName;

        [MessageBodyMember]
        public int idGroup;

        [MessageBodyMember]
        public int accessBitset;
    }

    [MessageContract]
    public class RemoteFileInfo : IDisposable
    {
        [MessageHeader]
        public FilesystemObject mMetaFile;

        /*public void setBaseClassValues(FilesystemObject fs)
        {
            base.setId(fs.getId());
            base.setFilepath(fs.getFilepath());
            base.setFilesize(fs.getFilesize());
            base.setDirFlag(fs.getDirFlag());
            base.setExistsFlag(fs.getIsExistsFlag());
            base.setUid(fs.getUid());
            base.setGid(fs.getGid());
            base.setAccessBitset(fs.getAccessBitset());
        }*/

        [MessageBodyMember]
        public System.IO.Stream mFileByteStream;

        public RemoteFileInfo()
        {
            mMetaFile = new FilesystemObject();
            mFileByteStream = new MemoryStream();
        }

        public void Dispose()
        {
            if (mFileByteStream != null)
            {
                mFileByteStream.Close();
                mFileByteStream = null;
            }
        }
    }

    [MessageContract]
    public class DownloadFileResult : IDisposable
    {
        [MessageHeader]
        public FilesystemObject mObj { get; set; }

        [MessageHeader]
        public OperationResult mResult { get; set; }

        [MessageHeader]
        public int mOrigFilesize { get; set; }

        [MessageBodyMember]
        public System.IO.Stream mFileByteStream;

        public void Dispose()
        {
            if (mFileByteStream != null)
            {
                mFileByteStream.Close();
                mFileByteStream = null;
            }
        }

        public DownloadFileResult()
        {
            mResult = new OperationResult();
            mOrigFilesize = 0;
            mFileByteStream = new MemoryStream();
            mObj = new FilesystemObject();
        }
    }

    [MessageContract]
    public class ManFilesystemObjResult
    {
        [MessageBodyMember]
        public int fileId { get; set; }

        [MessageBodyMember]
        public OperationResult mResult { get; set; }

        public ManFilesystemObjResult()
        {
            fileId = -1;
            mResult = new OperationResult();
        }
    }

    [DataContract]
    public class BrowserDataResult
    {
        [DataMember]
        public List<FilesystemObject> mFiles { get; set; }

        [DataMember]
        public List<FilesystemObject> mDirectories { get; set; }

        [DataMember]
        public string rootPath { get; set; }

        [DataMember]
        public string currPath { get; set; }

        [DataMember]
        public OperationResult mResult { get; set; }

        public BrowserDataResult()
        {
            mResult = new OperationResult();
        }
    }

}
