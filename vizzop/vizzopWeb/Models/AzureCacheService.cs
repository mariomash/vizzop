using System;
using Microsoft.ApplicationServer.Caching;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure;

namespace vizzopWeb.Models
{
    public sealed class SingletonCache
    {

        //public static readonly SingletonCache Instance = new SingletonCache();

        private static volatile SingletonCache instance;
        private static object syncRoot = new Object();

        private DataCacheFactory _factory = new DataCacheFactory();
        private DataCache _cache = new DataCache();
        private Utils utils = new Utils();
        private TimeSpan LockTimeout = TimeSpan.FromSeconds(5);
        private TimeSpan ObjTimeout = TimeSpan.FromMinutes(15);
        private string region = @"vizzop";


        CloudStorageAccount storageAccount;
        CloudBlobClient blobClient;


        private SingletonCache()
        {
            try
            {
                _cache = _factory.GetDefaultCache();
                _cache.CreateRegion(this.region);

                storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
                blobClient = storageAccount.CreateCloudBlobClient();

            }
            catch (Exception)
            {
                //utils.GrabaLogExcepcion(ex);
                _cache = null;
            }
        }


        public static SingletonCache Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new SingletonCache();
                    }
                }

                return instance;
            }
        }

        public bool InsertScreenCaptureInStorage(ScreenCapture sc, string key)
        {
            try
            {
                // Retrieve storage account from connection string.
                CloudBlobContainer container = blobClient.GetContainerReference("rtscreencaptures");
                // Create the container if it does not already exist.
                container.CreateIfNotExists();

                CloudBlockBlob blockBlob = container.GetBlockBlobReference(key);
                blockBlob.DeleteIfExists();
                blockBlob.UploadText(sc.Data);
                blockBlob.Metadata["checksum"] = sc.checksum;
                blockBlob.Metadata["CreatedOn"] = sc.CreatedOn.ToString();
                //blockBlob.Metadata["Blob"] = sc.Blob;
                blockBlob.Metadata["GUID"] = sc.GUID;
                //blockBlob.Metadata["Headers"] = sc.Headers;
                blockBlob.Metadata["Height"] = sc.Height.ToString();
                blockBlob.Metadata["ID"] = sc.ID.ToString();
                blockBlob.Metadata["MouseX"] = sc.MouseX.ToString();
                blockBlob.Metadata["MouseY"] = sc.MouseY.ToString();
                blockBlob.Metadata["PicturedOn"] = sc.PicturedOn.ToString();
                blockBlob.Metadata["ReceivedOn"] = sc.ReceivedOn.ToString();
                blockBlob.Metadata["ScrollLeft"] = sc.ScrollLeft.ToString();
                blockBlob.Metadata["ScrollTop"] = sc.ScrollTop.ToString();
                blockBlob.Metadata["ThumbNail"] = sc.ThumbNail;
                blockBlob.Metadata["Url"] = sc.Url;
                blockBlob.Metadata["Width"] = sc.Width.ToString();
                blockBlob.Metadata["WindowName"] = sc.WindowName;
                blockBlob.SetMetadata();

                return true;
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return false;
            }
        }

        public ScreenCapture GetScreenCaptureInStorage(string key)
        {
            try
            {
                // Retrieve storage account from connection string.
                CloudBlobContainer container = blobClient.GetContainerReference("rtscreencaptures");
                // Create the container if it does not already exist.
                container.CreateIfNotExists();
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(key);
                if (blockBlob.Exists())
                {
                    container.FetchAttributes();
                    ScreenCapture sc = new ScreenCapture();
                    sc.Data = blockBlob.DownloadText();
                    sc.checksum = blockBlob.Metadata["checksum"];
                    sc.CreatedOn = Convert.ToDateTime(blockBlob.Metadata["CreatedOn"]);
                    //sc.Blob = blockBlob.Metadata["Blob"];
                    sc.GUID = blockBlob.Metadata["GUID"];
                    //sc.Headers = blockBlob.Metadata["Headers"];
                    sc.Height = Convert.ToInt16(blockBlob.Metadata["Height"]);
                    sc.ID = Convert.ToInt16(blockBlob.Metadata["ID"]);
                    sc.MouseX = Convert.ToInt16(blockBlob.Metadata["MouseX"]);
                    sc.MouseY = Convert.ToInt16(blockBlob.Metadata["MouseY"]);
                    sc.PicturedOn = Convert.ToDateTime(blockBlob.Metadata["PicturedOn"]);
                    sc.ReceivedOn = Convert.ToDateTime(blockBlob.Metadata["ReceivedOn"]);
                    sc.ScrollLeft = Convert.ToInt16(blockBlob.Metadata["ScrollLeft"]);
                    sc.ScrollTop = Convert.ToInt16(blockBlob.Metadata["ScrollTop"]);
                    sc.ThumbNail = blockBlob.Metadata["ThumbNail"];
                    sc.Url = blockBlob.Metadata["Url"];
                    sc.Width = Convert.ToInt16(blockBlob.Metadata["Width"]);
                    sc.WindowName = blockBlob.Metadata["WindowName"];

                    return sc;
                }

                return null;
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return null;
            }
        }

        public object Get(string key)
        {
            try
            {
                return _cache.Get(key);
            }
            catch (Exception)
            {
                //utils.GrabaLogExcepcion(ex);
                return null;
            }
        }

        public object GetInRegion(string key, string _region)
        {
            try
            {
                return _cache.Get(key, _region);
            }
            catch (Exception)
            {
                //utils.GrabaLogExcepcion(ex);
                return null;
            }
        }

        public object GetInRegionWithLock(string key, string _region, out DataCacheLockHandle _lockHandle)
        {
            DataCacheLockHandle lockHandle = null;
            object ObjCache = null;
            try
            {
                bool islocked = true;
                DateTime start_time = DateTime.Now;
                while ((islocked == true) && (DateTime.Now < start_time.AddSeconds(20)))
                {
                    islocked = false;
                    try
                    {
                        ObjCache = _cache.GetAndLock(key, LockTimeout, out lockHandle, _region);
                        islocked = false;
                    }
                    catch (Exception _ex)
                    {
                        islocked = false;
                        if (_ex.Message.Contains(@"ErrorCode<ERRCA0011>:SubStatus<ES0001>:") == true)
                        {
                            islocked = true;
                        }
                    }
                }
                /*
                if (ObjCache == null)
                {
                    ObjCache = GetInRegion(key, _region);
                }
                 */
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
            }
            _lockHandle = lockHandle;
            return ObjCache;
        }

        public object GetByTag(string _tag)
        {
            try
            {
                DataCacheTag tag = new DataCacheTag(_tag);
                return _cache.GetObjectsByTag(tag, region);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object GetAllInRegion(string _region)
        {
            try
            {
                //DataCacheTag tag = new DataCacheTag(_tag);
                return _cache.GetObjectsInRegion(_region);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object GetWithLock(string key, out DataCacheLockHandle _lockHandle)
        {
            DataCacheLockHandle lockHandle = null;
            object ObjCache = null;
            try
            {
                bool islocked = true;
                DateTime start_time = DateTime.Now;
                while ((islocked == true) && (DateTime.Now < start_time.AddSeconds(20)))
                {
                    try
                    {
                        ObjCache = _cache.GetAndLock(key, LockTimeout, out lockHandle, region);
                        islocked = false;
                    }
                    catch (Exception _ex)
                    {
                        islocked = false;
                        if (_ex.Message.Contains(@"ErrorCode<ERRCA0011>:SubStatus<ES0001>:") == true)
                        {
                            islocked = true;
                        }
                    }
                }
                /*
                if (ObjCache == null)
                {
                    ObjCache = GetInRegion(key, region);
                }
                */
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
            }
            _lockHandle = lockHandle;
            return ObjCache;
        }

        public bool Insert(string key, object obj)
        {
            try
            {
                if (obj != null)
                {
                    _cache.Put(key, obj, ObjTimeout, region);
                }
                return true;
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return false;
            }
        }

        public bool InsertInRegion(string key, object obj, string _region)
        {
            try
            {
                if (obj != null)
                {
                    if (_region != null)
                    {
                        _cache.CreateRegion(_region);
                        _cache.Put(key, obj, ObjTimeout, _region);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return false;
            }
        }

        public bool InsertInRegionWithLock(string key, object obj, string _region, DataCacheLockHandle lockHandle)
        {
            try
            {
                if (obj != null)
                {
                    if (lockHandle != null)
                    {
                        _cache.PutAndUnlock(key, obj, lockHandle, ObjTimeout, _region);
                    }
                    else
                    {
                        this.InsertInRegion(key, obj, _region);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return this.InsertInRegion(key, obj, _region);
                //return false;
            }
        }

        public bool InsertWithTags(string key, object obj, List<DataCacheTag> tags)
        {
            try
            {
                if (obj != null)
                {
                    _cache.Put(key, obj, ObjTimeout, tags, region);
                }
                return true;
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return false;
            }
        }

        public bool InsertWithLock(string key, object obj, DataCacheLockHandle lockHandle)
        {
            try
            {
                if (obj != null)
                {
                    if (lockHandle != null)
                    {
                        _cache.PutAndUnlock(key, obj, lockHandle, ObjTimeout, region);
                    }
                    else
                    {
                        return this.Insert(key, obj);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return this.Insert(key, obj);
                //return false;
            }
        }

        public bool InsertWithLockAndTags(string key, object obj, DataCacheLockHandle lockHandle, List<DataCacheTag> tags)
        {
            try
            {
                if (obj != null)
                {
                    if (lockHandle != null)
                    {
                        _cache.PutAndUnlock(key, obj, lockHandle, ObjTimeout, tags, region);
                    }
                    else
                    {
                        return this.InsertWithTags(key, obj, tags);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                try
                {
                    this.UnLock(key, lockHandle);
                    return this.InsertWithTags(key, obj, tags);
                }
                catch (Exception ex)
                {
                    utils.GrabaLogExcepcion(ex);
                    return false;
                }
            }
        }

        public bool UnLock(string key, DataCacheLockHandle lockHandle)
        {
            try
            {
                if (lockHandle != null)
                {
                    _cache.Unlock(key, lockHandle, region);
                }
                return true;
            }
            catch (Exception)
            {
                //utils.GrabaLogExcepcion(ex);
                return false;
            }
        }

        public bool UnLockInRegion(string key, DataCacheLockHandle lockHandle, string _region)
        {
            try
            {
                if (lockHandle != null)
                {
                    _cache.Unlock(key, lockHandle, _region);
                }
                return true;
            }
            catch (Exception)
            {
                //utils.GrabaLogExcepcion(ex);
                return false;
            }
        }

        public void Remove(string key)
        {
            try
            {
                _cache.Remove(key, region);
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
            }
        }



        public void RemoveInRegion(string key, string _region)
        {
            try
            {
                _cache.Remove(key, _region);
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
            }
        }

        public void CleanRegion(string p)
        {
            try
            {
                _cache.RemoveRegion(p);
                _cache.CreateRegion(p);
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
            }
            //throw new NotImplementedException();
        }
    }

}