using NUnit.Framework;
using PokemonUnity.Interface.UnityEngine;

namespace PokemonUnity.Interface.UnityEngine.Tests
{
    public class ServiceLocatorTests
    {
        [SetUp]
        public void SetUp() => ServiceLocator.Global.Clear();

        [TearDown]
        public void TearDown() => ServiceLocator.Global.Clear();

        [Test]
        public void Register_And_Get_ReturnsSameInstance()
        {
            var svc = new DummyService();
            ServiceLocator.Global.Register(svc);
            Assert.AreSame(svc, ServiceLocator.Global.Get<DummyService>());
        }

        [Test]
        public void Get_Unregistered_ThrowsInvalidOperationException()
        {
            Assert.Throws<System.InvalidOperationException>(
                () => ServiceLocator.Global.Get<DummyService>());
        }

        [Test]
        public void TryGet_Unregistered_ReturnsFalse()
        {
            bool found = ServiceLocator.Global.TryGet<DummyService>(out var svc);
            Assert.IsFalse(found);
            Assert.IsNull(svc);
        }

        [Test]
        public void TryGet_Registered_ReturnsTrueAndInstance()
        {
            var svc = new DummyService();
            ServiceLocator.Global.Register(svc);
            bool found = ServiceLocator.Global.TryGet<DummyService>(out var result);
            Assert.IsTrue(found);
            Assert.AreSame(svc, result);
        }

        [Test]
        public void Register_Twice_OverwritesPrevious()
        {
            var a = new DummyService();
            var b = new DummyService();
            ServiceLocator.Global.Register(a);
            ServiceLocator.Global.Register(b);
            Assert.AreSame(b, ServiceLocator.Global.Get<DummyService>());
        }

        [Test]
        public void Unregister_RemovesService()
        {
            ServiceLocator.Global.Register(new DummyService());
            ServiceLocator.Global.Unregister<DummyService>();
            Assert.Throws<System.InvalidOperationException>(
                () => ServiceLocator.Global.Get<DummyService>());
        }

        [Test]
        public void Clear_RemovesAll()
        {
            ServiceLocator.Global.Register(new DummyService());
            ServiceLocator.Global.Clear();
            Assert.Throws<System.InvalidOperationException>(
                () => ServiceLocator.Global.Get<DummyService>());
        }

        private class DummyService { }
    }
}
