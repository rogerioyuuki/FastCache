﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Jitbit.Utils;
using System.Collections.Concurrent;
using System.Runtime.Caching;

BenchmarkRunner.Run<BenchMark>();

[ShortRunJob, MemoryDiagnoser]
public class BenchMark
{
	private static FastCache<string, int> _cache = new FastCache<string, int>(1000);
	private static ConcurrentDictionary<string, int> _dict = new();

	private static DateTime _dtPlus10Mins = DateTime.Now.AddMinutes(10);

	[GlobalSetup]
	public void GlobalSetup()
	{
		//add 10000 values
		for (int i = 0; i < 1000; i++)
		{
			_dict.TryAdd("test" + i, i);
			_cache.AddOrUpdate("test" + i, i, TimeSpan.FromMinutes(1));
			MemoryCache.Default.Add("test" + i, i, _dtPlus10Mins);
		}
	}

	[Benchmark]
	public void DictionaryLookup()
	{
		_cache.TryGet("test123", out _);
		_cache.TryGet("test234", out _);
		_cache.TryGet("test673", out _);
		_cache.TryGet("test987", out _);
	}

	[Benchmark]
	public void FastCacheLookup()
	{
		_cache.TryGet("test123", out _);
		_cache.TryGet("test234", out _);
		_cache.TryGet("test673", out _);
		_cache.TryGet("test987", out _);
	}

	[Benchmark]
	public void MemoryCacheLookup()
	{
		var x = MemoryCache.Default["test123"];
		x = MemoryCache.Default["test234"];
		x = MemoryCache.Default["test673"];
		x = MemoryCache.Default["test987"];
	}

	[Benchmark]
	public void FastCacheGetOrAdd()
	{
		_cache.GetOrAdd("test123", k => 123, TimeSpan.FromSeconds(1));
		_cache.GetOrAdd("test234", k => 124, TimeSpan.FromSeconds(1));
		_cache.GetOrAdd("test673", k => 125, TimeSpan.FromSeconds(1));
		_cache.GetOrAdd("test987", k => 126, TimeSpan.FromSeconds(1));
	}

	[Benchmark]
	public void FastCacheAddRemove()
	{
		_cache.AddOrUpdate("1111", 42, TimeSpan.FromMinutes(1));
		_cache.Remove("1111");
	}

	[Benchmark]
	public void MemoryCacheAddRemove()
	{
		MemoryCache.Default.Add("1111", 42, _dtPlus10Mins);
		MemoryCache.Default.Remove("1111");
	}

}