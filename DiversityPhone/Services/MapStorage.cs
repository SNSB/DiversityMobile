using System;
using System.Linq;
using System.Collections.Generic;
using DiversityPhone.Model;
using ReactiveUI;
using DiversityPhone.Messages;
using DiversityPhone.Common;
using System.Data.Linq;
using System.Text;
using System.Linq.Expressions;
using Svc = DiversityPhone.DiversityService;
using System.IO.IsolatedStorage;

namespace DiversityPhone.Services
{
    public class MapStorageService :IMapStorageService
    {
        protected IMessageBus _messenger;
        protected IList<IDisposable> _subscriptions;

        public MapStorageService(IMessageBus messenger)
        {
            this._messenger = messenger;

            _subscriptions = new List<IDisposable>()
            {

                _messenger.Listen<Map>(MessageContracts.SAVE)
                    .Subscribe(map => addOrUpdateMap(map)),
                _messenger.Listen<Map>(MessageContracts.DELETE)
                    .Subscribe(map => deleteMap(map)),
            };

        }

        #region Database Opereations

        public IList<Map> getAllMaps()
        {
            return uncachedQuery(ctx => from m in ctx.Maps
                                        select m);
        }


        public Map getMapbyServerKey(String serverKey)
        {
            return singletonQuery(ctx => from map in ctx.Maps
                                         where map.ServerKey == serverKey
                                         select map);
        }


        public Map getMapByURI(string uri)
        {
            IList<Map> objects = uncachedQuery(ctx => from map in ctx.Maps
                                                      where map.Uri == uri
                                                      select map);
            if (objects.Count == 0)
                throw new KeyNotFoundException();
            if (objects.Count > 1)
                throw new DuplicateKeyException(objects);
            return objects[0];
        }


        public IList<Map> getMapsForPoint(double latitude, double longitude)
        {
            return uncachedQuery(ctx => from m in ctx.Maps
                                        where Math.Max(m.NWLat,m.NELat) >= latitude
                                            && Math.Min(m.SWLat,m.SELat) <= latitude
                                            && Math.Max(m.NELong,m.SELong) >= longitude
                                            && Math.Min(m.NWLong,m.SWLong) <= longitude
                                        select m);
        }

        public bool isPresent(String key)
        {
            Map map = getMapbyServerKey(key);
            if (map != null)
                return true;
            else
                return false;
        }

        public void addOrUpdateMap(Map map)
        {
            if (isPresent(map.ServerKey))
                deleteMap(map);
            using (var ctx = new DiversityDataContext())
            {
                ctx.Maps.InsertOnSubmit(map);
                ctx.SubmitChanges();
            }

        }

        public void deleteAllMaps()
        {
            IList<Map> maps = getAllMaps();
            foreach (Map map in maps)
                deleteMap(map);
        }

        public void deleteMap(Map map)
        {
            var myStore = IsolatedStorageFile.GetUserStoreForApplication();
            if (myStore.FileExists(map.Uri))
            {
                myStore.DeleteFile(map.Uri);
            }
            using (var ctx = new DiversityDataContext())
            {
                Map deleteableMap =
                    (from maps in ctx.Maps
                     where maps.ServerKey == map.ServerKey
                     select maps).First();
                ctx.Maps.DeleteOnSubmit(deleteableMap);
                ctx.SubmitChanges();
            }
        }


        #endregion

        #region Generische Implementierungen
     
        private T singletonQuery<T>(QueryProvider<T> queryProvider)
        {
            T result = default(T);
            withDataContext(ctx =>
            {
                var query = queryProvider(ctx);
                result = query
                    .FirstOrDefault();
            });
            return result;
        }


        private void withDataContext(Action<DiversityDataContext> operation)
        {
            using (var ctx = new DiversityDataContext())
                operation(ctx);
        }


        private IList<T> uncachedQuery<T>(QueryProvider<T> query)
        {
            IList<T> result = null;
            withDataContext(ctx => result = query(ctx).ToList());
            return result;
        }


        private class QueryCacheSource<T> : ICacheSource<T>
        {
            IQueryOperations<T> operations;
            QueryProvider<T> queryProvider;

            public QueryCacheSource(IQueryOperations<T> operations, QueryProvider<T> queryProvider)
            {
                this.operations = operations;
                this.queryProvider = queryProvider;
            }

            public IEnumerable<T> retrieveItems(int count, int offset)
            {
                using (var ctx = new DiversityDataContext())
                {
                    return queryProvider(ctx)
                        .Skip(offset)
                        .Take(count)
                        .ToList();
                }
            }

            public int Count
            {
                get
                {
                    using (var ctx = new DiversityDataContext())
                    {
                        return queryProvider(ctx)
                            .Count();
                    }
                }
            }


            public int IndexOf(T item)
            {
                using (var ctx = new DiversityDataContext())
                {
                    return operations.WhereKeySmallerThan(queryProvider(ctx), item)
                        .Count();
                }
            }

        }

        private delegate Table<T> TableProvider<T>(DiversityDataContext ctx) where T : class;

        private delegate IQueryable<T> QueryProvider<T>(DiversityDataContext ctx);

        #endregion


    }
}
