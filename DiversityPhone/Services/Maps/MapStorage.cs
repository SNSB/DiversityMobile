using System;
using System.Linq;
using System.Collections.Generic;
using DiversityPhone.Model;
using ReactiveUI;
using DiversityPhone.Messages;
using System.IO.IsolatedStorage;

namespace DiversityPhone.Services
{
    public class MapStorageService : IMapStorageService
    {
        protected IMessageBus Messenger;

        public MapStorageService(IMessageBus messenger)
        {
            this.Messenger = messenger;


            Messenger.Listen<Map>(MessageContracts.SAVE)
                .Subscribe(map => addMap(map));
            Messenger.Listen<Map>(MessageContracts.DELETE)
                .Subscribe(map => deleteMap(map));


        }

        private void withDataContext(Action<DiversityDataContext> operation)
        {
            using (var ctx = new DiversityDataContext())
                operation(ctx);
        }

        private IList<T> uncachedQuery<T>(Func<DiversityDataContext, IQueryable<T>> query)
        {
            IList<T> result = null;
            withDataContext(ctx => result = query(ctx).ToList());
            return result;
        }

        public IList<Map> getAllMaps()
        {
            return uncachedQuery(ctx => from m in ctx.Maps
                                        select m);
        }

        public bool isPresent(String key)
        {
            bool res = false;

            withDataContext(ctx => res = (from map in ctx.Maps
                                          where map.ServerKey == key
                                          select map).Any());
            return res;
        }

        public void addMap(Map map)
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
    } 
}
