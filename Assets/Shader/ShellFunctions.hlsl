#ifndef SHELL_FUNCS
#define SHELL_FUNCS 

static const int SHELL_COUNT = 256;

int _ShellIndex;
uniform float _ShellLength; 
uniform float _Density;
uniform float _NoiseMin, _NoiseMax;
uniform float _Thickness;
uniform float _Attenuation;
uniform float _OcclusionBias;
uniform float _ShellDistanceAttenuation;
uniform float _Curvature;
uniform float _DisplacementStrength;
uniform float3 _ShellColor;
#if !UNITY_ANY_INSTANCING_ENABLED
float3 _ShellDirection;
#endif

int GetShellIndex()
{
#if UNITY_ANY_INSTANCING_ENABLED
	return unity_InstanceID; 
#endif
    return _ShellIndex;
}

float hash(uint n)
{ 
    n = (n << 13U) ^ n;
    n = n * (n * n * 15731U + 0x789221U) + 0x1376312589U;
    return float(n & uint(0x7fffffffU)) / float(0x7fffffff);
} 
inline half DotClamp(half3 a, half3 b)
{
#if (SHADER_TARGET < 30)
    return saturate(dot(a, b));
#else
			return max(0.0h, dot(a, b));
#endif
}
void ShellPos_float(float3 pos, out float3 outPos)
{ 
    float shellHeight = (float) GetShellIndex() / (float) SHELL_COUNT;
	
	shellHeight = pow(shellHeight, _ShellDistanceAttenuation);
	
	outPos = pos + pos * _ShellLength * shellHeight;
	
	float k = pow(shellHeight, _Curvature);
	
	outPos += k * _DisplacementStrength;
	 
    outPos += k * 
#if UNITY_ANY_INSTANCING_ENABLED 
	_PerInstanceData[unity_InstanceID].direction;
	#else
	_ShellDirection; 
#endif
	
#if SHADERGRAPH_PREVIEW
	outPos = pos;
#endif
}
 
void ShellMat_float(float2 uv, float3 normal, float3 lightDir, out float3 _outColor, out float _outAlpha)
{
	float2 newUV = uv * _Density;
	
	float2 localUV = frac(newUV) * 2 - 1;
	
	float localDistanceFromCenter = length(localUV);
	
	uint2 tid = newUV;
    uint seed = tid.x + 100 * tid.y + 100 * 10;

	float shellIndex = GetShellIndex();
    float shellCount = SHELL_COUNT;
	
	float rand = lerp(_NoiseMin, _NoiseMax, hash(seed));
	
	float h = shellIndex / shellCount;

	int outsideThickness = (localDistanceFromCenter) > (_Thickness * (rand - h));
			
	_outAlpha = 1;
    if (outsideThickness && GetShellIndex() > 0)
    {
        _outAlpha = 0;
        return;
    }
	
	float ndotl = DotClamp(normal, lightDir) * 0.5f + 0.5f;

	ndotl = ndotl * ndotl;

	float ambientOcclusion = pow(h, _Attenuation);
	
	ambientOcclusion += _OcclusionBias;
	
	ambientOcclusion = saturate(ambientOcclusion);
	
    _outColor = _ShellColor; //float3(_ShellColor * ndotl * ambientOcclusion);
}

#endif 