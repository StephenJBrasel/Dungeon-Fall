public class Noise
{
	private const uint BIT_NOISE1 = 0xB5297A4D;
	private const uint BIT_NOISE2 = 0x68E31DA4;
	private const uint BIT_NOISE3 = 0x1B56C4E9;
	private const uint dimensionalPrime1 = 198491317;
	private const uint dimensionalPrime2 = 6542989;

	private uint seed;
	public long position;

	public Noise() : this((uint)System.DateTime.Now.Ticks) {}
	public Noise(uint seed) {
		this.seed = seed;
	}

	/// <summary>
	/// Generates SquirrelRNG Noise between 0 (inclusive) and uint.MaxValue (inclusive)
	/// </summary>
	/// <param name="position">The position in the Noise Map to generate the value for</param>
	/// <returns>A generated value between 0 (inclusive) and uint.MaxValue (inclusive)</returns>
	public uint generate(uint position) {
		uint mangled = position;
		mangled *= BIT_NOISE1;
		mangled += seed;
		mangled ^= (mangled >> 8);
		mangled += BIT_NOISE2;
		mangled ^= (mangled << 8);
		mangled *= BIT_NOISE3;
		mangled ^= (mangled >> 8);
		
		return mangled;
	}
	public ulong generate(ulong position) {
		ulong mangled = position;
		mangled *= BIT_NOISE1;
		mangled += seed;
		mangled ^= (mangled >> 8);
		mangled += BIT_NOISE2;
		mangled ^= (mangled << 8);
		mangled *= BIT_NOISE3;
		mangled ^= (mangled >> 8);
		
		return mangled;
	}

	/// <summary>
	/// Generates SquirrelRNG Noise between 0 (inclusive) and uint.MaxValue (inclusive)
	/// </summary>
	/// <param name="position">The position in the Noise Map to generate the value for</param>
	/// <returns>A generated value between 0 (inclusive) and uint.MaxValue (inclusive)</returns>
	public uint generate(int position) { return generate((uint)position); }
	public ulong generate(long position) { return generate((ulong)position); }

	/// <summary>
	/// Provides the next generated value in the SquirrelRNG Noise Map. 
	/// </summary>
	/// <returns>A generated value between 0 (inclusive) and uint.MaxValue (inclusive)</returns>
	public uint NextUint() {return generate((uint)position++); }

	/// <summary>
	/// Provides a value between 0 (inclusive) and end (inclusive) from the SuirrelRNG Noise Map
	/// </summary>
	/// <param name="end">The maximum value (inclusive) allowed to be generated</param>
	/// <returns>A generated value between 0 (inclusive) and end (inclusive)</returns>
	public uint NextUint(int end) {
		return (uint)(NextUint() % (end + 1));
	}

	/// <summary>
	/// Provides a value between 0 (inclusive) and end (inclusive) from the SuirrelRNG Noise Map
	/// </summary>
	/// <param name="end">The maximum value (inclusive) allowed to be generated</param>
	/// <returns>A generated value between 0 (inclusive) and end (inclusive)</returns>
	public uint NextUint(uint end) {
		return NextUint() % (end + 1);
	}

	/// <summary>
	/// Provides a value between start (inclusive) and end (inclusive) from the SuirrelRNG Noise Map
	/// </summary>
	/// <param name="start">The minimum value (inclusive) allowed to be generated.</param>
	/// <param name="end">The maximum value (inclusive) allowed to be generated.</param>
	/// <returns></returns>
	public uint NextUint(int start, int end) {
		return (uint)((NextUint() % (end - start + 1)) + start);
	}

	/// <summary>
	/// Provides a value between int.MinValue (inclusive) and int.MaxValue (inclusive) from the SquirrelRNG Noise Map.
	/// </summary>
	/// <returns>A value between int.MinValue (inclusive) and int.MaxValue (inclusive).</returns>
	public int Next() {
		return (int)NextUint();
	}
	public int Next(int end) {
		return (int)NextUint(end);
	}
	public int Next(int start, int end) {
		return (int)NextUint(start, end);
	}

	public uint Asymptotic(uint count) {
		uint i = 0;
		double f = 0d, c = 1d, n = NextDouble();
		while ((f += (c *= 0.5)) < n) 
			if(++i >= count) return count;
		return i;
	}

	/// <summary>
	/// Returns a random number between 0 (inclusive) and number (inclusive)
	/// that falls in the ratio where each successive number has 1/((n*n+n)/2) less of a chance to be chosen, 
	/// e.g., if number = 4, the chances to return are:
	/// <list type=">">
	/// <listheader>
	/// <term>n</term>
	/// <description>*chance*</description>
	/// </listheader>
	/// <item>
	/// <term>1</term>
	/// <description>4/10</description>
	/// </item>
	/// <item>
	/// <term>2</term>
	/// <description>3/10</description>
	/// </item>
	/// <item>
	/// <term>3</term>
	/// <description>2/10</description>
	/// </item>
	/// <item>
	/// <term>4</term>
	/// <description>1/10</description>
	/// </item>
	/// </list>
	/// </summary>
	/// <param name="number">the max (inclusive) number to be returned out of the triangle ratio between 0 and number</param>
	/// <returns>an unsigned integer in the triangle ratio between 0 and number</returns>
	public uint triangleRatio(uint number) {
		uint i = 0, 
			b = tNum(number), 
			n = NextUint(b);
		while((b - tNum(i++)) > n){}
		return i+1;
	}

	public uint tNum(uint number) {
		return number * (number + 1) / 2;
	}

	/// <summary>
	/// Provides a floating point value between 0 (inclusive) and 1 (inclusive) from the SquirrelRNG Noise Map. 
	/// </summary>
	/// <returns>A floating point value between 0.0 (inclusive) and 1.0 (inclusive).</returns>
	public float NextFloat() {
		return (float)NextUint() / uint.MaxValue;
	}
	
	public double NextDouble() {
		return (double)generate(position++) / ulong.MaxValue;
	}

	public uint GetNoiseUintDimension1(int position) {
		return generate(position);
	}

	public uint GetNoiseUintDimension2(int x, int y) {
		return generate((uint)(x + (dimensionalPrime1 * y)));
	}

	public uint GetNoiseUintDimension3(int x, int y, int z) {
		return generate((uint)(x + (dimensionalPrime1 * y) + (dimensionalPrime2 * z)));
	}
}
