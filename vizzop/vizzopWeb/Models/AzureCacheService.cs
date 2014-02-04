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

        public bool Insert(string key, object obj)
        {
            try
            {
                if (obj != null)
                {
                    _cache.Put(key, obj, TimeSpan.FromHours(1));
                }
                return true;
            }
            catch (Exception ex)
            {
                utils.GrabaLogExcepcion(ex);
                return false;
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