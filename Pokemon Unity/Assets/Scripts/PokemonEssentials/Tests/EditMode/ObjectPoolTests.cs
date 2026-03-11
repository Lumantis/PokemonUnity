using NUnit.Framework;
using PokemonUnity.Interface.UnityEngine;

namespace PokemonUnity.Interface.UnityEngine.Tests
{
    /// <summary>EditMode NUnit tests for <see cref="PlainObjectPool{T}"/>.</summary>
    public class ObjectPoolTests
    {
        [Test]
        public void Rent_ReturnsNonNull()
        {
            var pool = new PlainObjectPool<PoolItem>(() => new PoolItem());
            Assert.IsNotNull(pool.Rent());
        }

        [Test]
        public void Return_And_Rent_ReusesInstance()
        {
            var pool = new PlainObjectPool<PoolItem>(() => new PoolItem());
            var item = pool.Rent();
            pool.Return(item);
            var item2 = pool.Rent();
            Assert.AreSame(item, item2, "Pool should return the previously returned instance.");
        }

        [Test]
        public void Pool_Grows_WhenExhausted()
        {
            var pool = new PlainObjectPool<PoolItem>(() => new PoolItem(), initialSize: 2);
            // Rent 3 items (more than initialSize)
            var a = pool.Rent();
            var b = pool.Rent();
            var c = pool.Rent();
            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
            Assert.IsNotNull(c);
            Assert.AreEqual(3, pool.TotalCreated);
        }

        [Test]
        public void Prewarm_Creates_CorrectCount()
        {
            var pool = new PlainObjectPool<PoolItem>(() => new PoolItem());
            pool.Prewarm(5);
            Assert.AreEqual(5, pool.TotalCreated);
            Assert.AreEqual(5, pool.Available);
        }

        [Test]
        public void OnRent_Callback_IsCalled()
        {
            int rentCount = 0;
            var pool = new PlainObjectPool<PoolItem>(
                () => new PoolItem(),
                onRent: _ => rentCount++);
            pool.Rent();
            pool.Rent();
            Assert.AreEqual(2, rentCount);
        }

        [Test]
        public void OnReturn_Callback_IsCalled()
        {
            int returnCount = 0;
            var pool = new PlainObjectPool<PoolItem>(
                () => new PoolItem(),
                onReturn: _ => returnCount++);
            var item = pool.Rent();
            pool.Return(item);
            Assert.AreEqual(1, returnCount);
        }

        [Test]
        public void Return_Null_DoesNotThrow()
        {
            var pool = new PlainObjectPool<PoolItem>(() => new PoolItem());
            Assert.DoesNotThrow(() => pool.Return(null));
        }

        private class PoolItem { }
    }
}
