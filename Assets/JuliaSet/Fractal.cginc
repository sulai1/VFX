//UNITY_SHADER_NO_UPGRADE

void JuliaSet_float(float2 UV, float2 C, out float Distance)
{
	int max_iterations = 10000;
	float2 z = UV;
	float2 nz = UV;
	int iterations = 0;
	while (nz.x * nz.x + nz.y * nz.y <= 4 &&  iterations<max_iterations)
	{
		z=nz;
		nz = float2(
			z.x*z.x-z.y*z.y+C.x,
			2*z.x*z.y+C.y
		);
		iterations++;
	}
	Distance = (iterations%(max_iterations/2))/(max_iterations/2.00);
}