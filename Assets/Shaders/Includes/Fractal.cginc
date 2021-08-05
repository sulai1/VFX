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

float fbm( float2 p,float _octaves,float _lacunarity,float _gain,float _amplitude,float _frequency)
{
    float _value=0;
    for( int i = 0; i < _octaves; i++ )
    {
        float2 i = floor( p * _frequency );
        float2 f = frac( p * _frequency );      
        float2 t = f * f * f * ( f * ( f * 6.0 - 15.0 ) + 10.0 );
        float2 a = i + float2( 0.0, 0.0 );
        float2 b = i + float2( 1.0, 0.0 );
        float2 c = i + float2( 0.0, 1.0 );
        float2 d = i + float2( 1.0, 1.0 );
        a = -1.0 + 2.0 * frac( sin( float2( dot( a, float2( 127.1, 311.7 ) ),dot( a, float2( 269.5,183.3 ) ) ) ) * 43758.5453123 );
        b = -1.0 + 2.0 * frac( sin( float2( dot( b, float2( 127.1, 311.7 ) ),dot( b, float2( 269.5,183.3 ) ) ) ) * 43758.5453123 );
        c = -1.0 + 2.0 * frac( sin( float2( dot( c, float2( 127.1, 311.7 ) ),dot( c, float2( 269.5,183.3 ) ) ) ) * 43758.5453123 );
        d = -1.0 + 2.0 * frac( sin( float2( dot( d, float2( 127.1, 311.7 ) ),dot( d, float2( 269.5,183.3 ) ) ) ) * 43758.5453123 );
        float A = dot( a, f - float2( 0.0, 0.0 ) );
        float B = dot( b, f - float2( 1.0, 0.0 ) );
        float C = dot( c, f - float2( 0.0, 1.0 ) );
        float D = dot( d, f - float2( 1.0, 1.0 ) );
        float noise = ( lerp( lerp( A, B, t.x ), lerp( C, D, t.x ), t.y ) );              
        _value += _amplitude * noise;
        _frequency *= _lacunarity;
        _amplitude *= _gain;
    }
    return _value;
}