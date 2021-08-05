#ifndef FLAME
#define FLAME


float3 p_polar(float2 uv)
{
	uv=center(uv);
	return float3(theta(uv),phi(uvx),len(uv))
}

float theta(float2 uv)
{
	return atan(uv.x/uv.y);
}

float phi(float2 uv)
{
	return atan(uv.y/uv.x);
}

//**********************************************//
float2 center(float2 uv)
{
	uv.x=uv.x*2.0-1.0;
	uv.y=uv.y*2.0-1.0;
}

float2 sinusoidal(float2 uv)
{
	uv=center(uv);
	return float2(sin(uv.x),sin(uv.y));
}

float2 spherical(float2 uv)
{
	uv=center(uv);
	return len(uv)*uv;
}

float2 swirl(float2 uv)
{
	uv=center(uv);
	float r = len(uv);
	r = r*r;
	return float2(uv.x*sin(r)-uv.y*cos(r),uv.x*cos(r)+uv.y*sin(r));
}

float2 horseshoe(float2 uv)
{
	uv=center(uv);
	float r = len(uv);
	return float2((uv.x-uv.y)*(uv.x+uv.y),2*uv.x*uv.y)/r;
}

float2 polar(float2 uv)
{
	uv=center(uv);
	float r = len(uv);
	return float2((uv.x-uv.y)*(uv.x+uv.y),2*uv.x*uv.y)/r;
}


#endif