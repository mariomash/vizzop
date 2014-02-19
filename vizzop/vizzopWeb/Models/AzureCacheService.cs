using System;
using Microsoft.ApplicationServer.Caching;

namespace vizzopWeb.Models
{
    sealed class SingletonCache
    {

        public static readonly SingletonCache Instance = new SingletonCache();

        private DataCacheFactory _factory = new DataCacheFactory();
        private DataCache _cache = new DataCache();
        private Utils utils = new Utils();
        private TimeSpan LockTimeout = TimeSpan.FromMinutes(1);
        private TimeSpan ObjTimeout = TimeSpan.FromHours(1);
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
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                _cache = null;
            }
        }

        public object Get(string key)
        {
            try
            {
                return _cache.Get(key);
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
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
                        ObjCache = _cache.GetAndLock(key, LockTimeout, out lockHandle);
                    }
                    catch (Exception _ex)
                    {
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
                    _cache.Put(key, obj, ObjTimeout);
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
                        _cache.PutAndUnlock(key, obj, lockHandle, ObjTimeout);
                    }
                    else
                    {
                        return this.Insert(key, obj);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return this.Insert(key, obj);
                //return false;
            }
        }

        public void Remove(string key)
        {
            try
            {
                _cache.Remove(key);
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
            }
        }

    }

}