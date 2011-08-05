using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xunit;

namespace DiversityPhone.Test
{
    public class TestTest : SilverlightTest
    {
        [Fact]
        public void AlwaysPass()
        {
            Assert.True(true, "method intended to always pass");
        }

        
        [Fact]
        public void AlwaysFail()
        {
            Assert.False(true, "method intended to always fail");
        }
    }
}
