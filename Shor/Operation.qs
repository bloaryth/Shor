// Created by Jiuqin Zhou and Yaoyao Ding.
// Licensed under the MIT License.

namespace myShor {
    open Microsoft.Quantum.Primitive;
	open Microsoft.Quantum.Canon;
	open Microsoft.Quantum.Extensions.Math;

    ///////////////////////////////////////////////////////////////////////////////////////////////
    // Introduction ///////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////
    ///
    /// This sample contains Q# code implementing Shor's quantum algorithm for
    /// factoring integers. The underlying modular arithmetic is implemented 
    /// in phase encoding, based on a paper by Stephane Beauregard who gave a
    /// quantum circuit for factoring n-bit numbers that needs 2n+3 qubits and 
    /// O(n³log(n)) elementary quantum gates.
    ///
    ///////////////////////////////////////////////////////////////////////////////////////////////

	/// # Summary 
    /// U|x> -> |ax mod N >
    ///
    /// # Input 
    /// ## a
    /// Constant by which multiplier is being multiplied. Must be co-prime to modulus.
    /// ## N
	/// The multiplication operation is performed modulo modulus.
	/// ## x
	/// The number being multiplied by a constant. This is an array of qubits 
	/// representing integer in little-endian bit order.
    ///
    /// # Output 
    /// None.
	operation OrderFindingOracle(a : Int, N : Int, x : Qubit[]) : () 
	{
		body
		{
			ModularMultiplyByConstantLE(a, N, LittleEndian(x));
		}
		controlled auto
	}

	/// # Summary
	/// Exponentiation function with base 2 and exponent x.
	///
	/// # Input
	/// ## x
	/// An Integer representing exponent.
	/// 
	/// # Output
	/// Exp(2, x)
	function Exp2(x : Int) : (Double)
	{
		mutable ans = 1.0;
		for(i in 0..x-1)
		{
			set ans = ans * 2.0;
		}
		return ans;
	}


	/// # Summary
	/// operating quantum fourier transform on x.
	///
	/// # Input
	/// ## x
	/// The array of qubits to be operated on with QFT.
	/// 
	/// # Output
	/// None.
	operation QuantumFourierTransform(x : Qubit[]) : () 
	{
		body 
		{
			let n = Length(x);
			for(i in 0..n-1) 
			{
				H(x[i]);
				for(j in i+1..n-1)
				{
					let theta = PI() * 1.0 / Exp2(j-i);
					(Controlled R1) ([x[j]], (theta, x[i]));
				}
			}
			for(i in 0..n/2-1)
			{
				SWAP(x[i], x[n-1-i]);
			}
		}
		adjoint auto
	}

	/// # Summary
	/// In the order finding with Exp(x, r)=1 mod N, find approximate s/r 
	/// with the numerator ans and the dominator Exp(2, t)
	///
	/// # Input
	/// ## x
	/// Order finding with Exp(x, r)=1 mod N.
	/// ## N
	/// Order finding with Exp(x, r)=1 mod N.
	///
	/// # Output
	/// ## ans
	/// ans is the numerator of approximate s/r
	/// ## t
	/// Exp(2, t) is the dominator of approximate s/r
	operation OrderFindingPhase(x : Int, N : Int) : (Int, Int) 
	{
		body
		{
			let n = BitSize(N);
			let t = 2 * n + 1;
			//let t = 7;
			mutable ans = 0;
			using(qubits = Qubit[t + n]) 
			{
				let r1 = qubits[0..t-1];
				let r2 = qubits[t..t+n-1];
				ApplyToEach(H, r1);
				X(r2[0]);
				mutable a = x;
				for(i in 0..t-1)
				{
					(Controlled OrderFindingOracle) ([r1[t-1-i]], (a, N, r2));
					set a = a * a % N;
				}
				(Adjoint QuantumFourierTransform) (r1);

				for(i in 0..t-1)
				{
					set ans = ans * 2;
					if(M(r1[i]) == One)
					{
						set ans = ans + 1;
					}
				}
				ResetAll(qubits);
			}
			return (ans, t);
		}
	}

}

