using Jitbit.Utils;


namespace UnitTests
{
	[TestClass]
	public class UnitTests
	{
		[TestMethod]
		public async Task TestGetSetCleanup()
		{
			var _cache = new FastCache<int, int>(cleanupJobInterval: 200);
			_cache.AddOrUpdate(42, 42, TimeSpan.FromMilliseconds(100));
			Assert.IsTrue(_cache.TryGet(42, out int v));
			Assert.IsTrue(v == 42);

			await Task.Delay(300);
			Assert.IsTrue(_cache.Count == 0); //cleanup job has run?
		}

		[TestMethod]
		public async Task Shortdelay()
		{
			var cache = new FastCache<int, int>();
			cache.AddOrUpdate(42, 42, TimeSpan.FromMilliseconds(500));

			await Task.Delay(50);

			Assert.IsTrue(cache.TryGet(42, out int result)); //not evicted
			Assert.IsTrue(result == 42);
		}

		[TestMethod]
		public async Task TestWithDefaultJobInterval()
		{
			var _cache2 = new FastCache<string, int>();
			_cache2.AddOrUpdate("42", 42, TimeSpan.FromMilliseconds(100));
			Assert.IsTrue(_cache2.TryGet("42", out _));
			await Task.Delay(150);
			Assert.IsFalse(_cache2.TryGet("42", out _));
		}

		[TestMethod]
		public void TestRemove()
		{
			var cache = new FastCache<string, int>();
			cache.AddOrUpdate("42", 42, TimeSpan.FromMilliseconds(100));
			cache.Remove("42");
			Assert.IsFalse(cache.TryGet("42", out _));
		}

		[TestMethod]
		public async Task TestTryAdd()
		{
			var cache = new FastCache<string, int>();
			Assert.IsTrue(cache.TryAdd("42", 42, TimeSpan.FromMilliseconds(100)));
			Assert.IsFalse(cache.TryAdd("42", 42, TimeSpan.FromMilliseconds(100)));

			await Task.Delay(120); //wait for it to expire

			Assert.IsTrue(cache.TryAdd("42", 42, TimeSpan.FromMilliseconds(100)));
		}

		[TestMethod]
		public async Task TestGetOrAdd()
		{
			var cache = new FastCache<string, int>();
			cache.GetOrAdd("key", k => 1024, TimeSpan.FromMilliseconds(100));
			Assert.IsTrue(cache.TryGet("key", out int res) && res == 1024);
			await Task.Delay(110);

			Assert.IsFalse(cache.TryGet("key", out _));
		}

		[TestMethod]
		public void TestClear()
		{
			var cache = new FastCache<string, int>();
			cache.GetOrAdd("key", k => 1024, TimeSpan.FromSeconds(100));

			cache.Clear();

			Assert.IsTrue(!cache.TryGet("key", out int res));
		}

		[TestMethod]
		public async Task TestTryAddAtomicness()
		{
			int i = 0;
			
			var cache = new FastCache<int, int>();
			cache.TryAdd(42, 42, TimeSpan.FromMilliseconds(50)); //add item with short TTL

			await Task.Delay(100); //wait for tha value to expire

			await TestHelper.RunConcurrently(20, () => {
				if (cache.TryAdd(42, 42, TimeSpan.FromSeconds(1)))
					i++;
			});

			Assert.IsTrue(i == 1, i.ToString());
		}

		[TestMethod]
		public async Task TestGetOrAddAtomicNess()
		{
			int i = 0;

			var cache = new FastCache<int, int>();
			
			cache.GetOrAdd(42, k => {
				i++;
				return 1024;
			}, TimeSpan.FromMilliseconds(100));


			await TestHelper.RunConcurrently(20, () => {
				if (cache.TryAdd(42, 42, TimeSpan.FromSeconds(1)))
					i++;
			});

			//test that add factory was called only once
			Assert.IsTrue(i == 1, i.ToString());
		}

		[TestMethod]
		public async Task Enumerator()
		{
			var cache = new FastCache<string, int>(); //now with default cleanup interval
			cache.GetOrAdd("key", k => 1024, TimeSpan.FromMilliseconds(100));

			Assert.IsTrue(cache.FirstOrDefault().Value == 1024);

			await Task.Delay(110);

			Assert.IsFalse(cache.Any());
		}

		[TestMethod]
		public async Task TestTtlExtended()
		{
			var _cache = new FastCache<int, int>();
			_cache.AddOrUpdate(42, 42, TimeSpan.FromMilliseconds(300));

			await Task.Delay(50);
			Assert.IsTrue(_cache.TryGet(42, out int result)); //not evicted
			Assert.IsTrue(result == 42);

			_cache.AddOrUpdate(42, 42, TimeSpan.FromMilliseconds(300));

			await Task.Delay(250);

			Assert.IsTrue(_cache.TryGet(42, out int result2)); //still not evicted
			Assert.IsTrue(result2 == 42);
		}
	}
}