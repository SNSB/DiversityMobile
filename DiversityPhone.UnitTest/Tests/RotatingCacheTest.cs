using System;
using System.Collections.Generic;
using System.Text;
using DiversityPhone.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace DiversityPhone.Test
{
    [TestClass]
    public class RotatingCacheTest
    {
        RotatingCache<string> _target;
        

        private class StringCacheSource : ICacheSource<string>
        {
            public int LastOffset { get; set; }
            public int LastCount { get; set; }


            public IEnumerable<string> retrieveItems(int count, int offset)
            {
                
                for (int i = offset; i < offset + count; i++)
                {
                    if(i < 100)
                        yield return i.ToString();
                }
            }

            public int Count
            {
                get { return 100; }
            }


            public int IndexOf(string item)
            {
                var res = Int16.Parse(item);
                if (res > -1 && res < Count)
                    return res;
                else
                    return -1;
            }
        }
      
        public RotatingCacheTest()
        {
            _target = new RotatingCache<string>(new StringCacheSource());
        }

        [TestMethod]
        public void Cache_should_return_data_correctly()
        {
            for (int i = 0; i < 100; i++)
            {
                Assert.Equals(i.ToString(), _target[i]);
            }
        }

        [TestMethod]
        public void Cache_should_not_return_data_out_of_range()
        {
            //Assert.Throws<IndexOutOfRangeException>(() => _target[-1]);
            //Assert.Throws<IndexOutOfRangeException>(() => _target[_target.Count + 1] );
        }

        [TestMethod]
        public void Cache_should_correctly_do_searches()
        {            
            Assert.Equals(21, _target.IndexOf("21"));
            Assert.Equals(-1, _target.IndexOf("1001"));

        }
    }
}
