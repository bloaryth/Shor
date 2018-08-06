namespace myShor {
    open Microsoft.Quantum.Primitive;
	open Microsoft.Quantum.Canon;
	open Microsoft.Quantum.Extensions.Math;

	operation OrderFindingOracle(a : Int, N : Int, x : Qubit[]) : () 
	{
		body
		{
			ModularMultiplyByConstantLE(a, N, LittleEndian(x));
		}
		controlled auto
	}

	function Exp2(x : Int) : (Double)
	{
		mutable ans = 1.0;
		for(i in 0..x-1)
		{
			set ans = ans * 2.0;
		}
		return ans;
	}

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

