using System;
using Microsoft.ApplicationServer.Caching;
using System.Collections.Generic;

namespace vizzopWeb.Models
{
    sealed class SingletonCache
    {

        public static readonly SingletonCache Instance = new SingletonCache();

        private DataCacheFactory _factory = new DataCacheFactory();
        private DataCache _cache = new DataCache();
        private Utils utils = new Utils();
        private TimeSpan LockTimeout = TimeSpan.FromSeconds(55);
        private TimeSpan ObjTimeout = TimeSpan.FromHours(1);
        private string region = @"vizzop";
        //private DataCacheLockHandle lockHandle;

        /*
         * Al pedir un objeto a cache... en caso de que falle lo intenta una y otra vez hasta que bloquea....
         * Al meter un objeto en la cache quita el bloqueo....
         * 
         */


        private SingletonCache()
        {
            try
            {
                _cache = _factory.GetDefaultCache();
                _cache.CreateRegion(this.region);
            }
            catch (Exception)
            {
                //utils.GrabaLogExcepcion(ex);
                _cache = null;
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

        public object GetWithLock(string key, out DataCacheLockHandle _lockHandle)
        {
            DataCacheLockHandle lockHandle = null;
            object ObjCache = null;
            try
            {
                bool islocked = true;
                while (islocked == true)
                {
                    islocked = false;
                    try
                    {
                        ObjCache = _cache.GetAndLock(key, LockTimeout, out lockHandle, region);
                    }
                    catch (Exception _ex)
                    {
                        /*
                         || (_ex.Message.Contains(@"ErrorCode<ERRCA0006>:SubStatus<ES0001>:") == true))
                         */
                        if (_ex.Message.Contains(@"ErrorCode<ERRCA0011>:SubStatus<ES0001>:") == true)
                        {
                            islocked = true;
                        }
                    }
                }
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

    }

}