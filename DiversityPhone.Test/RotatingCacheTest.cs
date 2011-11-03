using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DiversityPhone.Test
{
    public class RotatingCacheTest
    {
        RotatingCache<int> _target;
        int _offset, _count;


        private IEnumerable<int> cachesource(int count, int offset)
        {
           
            for (int i = offset; i < offset+count; i++)
            {
                yield return i;
            }
        }
        public RotatingCacheTest()
        {
            _target = new RotatingCache<int>(10, cachesource);
        }

        [Fact]
        public void Cache_should_return_data_correctly()
        {
            for (int i = 0; i < 100; i++)
            {
                Assert.Equal(i, _target[i]);
            }
        }
    }
}
