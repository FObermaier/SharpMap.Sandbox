using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace SharpMap.Data.Providers.Business.Tests
{
    [TestFixture]
    public class TypeUtilityTest
    {
        private class Entity
        {
            public int Field;

            public int Property { get; set; }
        }

        [Test]
        public void TestMemberType()
        {
            var df = TypeUtility<Entity>.GetMemberGetDelegate<int>("Field");
            var dt = TypeUtility<Entity>.GetMemberGetDelegate<int>("Property");
            
            var e = new Entity {Field = 1, Property = 2};

            Assert.AreEqual(e.Field, df(e));
            Assert.AreEqual(e.Property, dt(e));
        }
    }
}
