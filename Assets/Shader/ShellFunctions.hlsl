#ifndef SHELL_FUNCS
#define SHELL_FUNCS
  
int _ShellIndex; // This is the current shell layer being operated on, it ranges from 0 -> _ShellCount 
int _ShellCount; // This is the total number of shells, useful for normalizing the shell index
float _ShellLength; // This is the amount of distance that the shells cover, if this is 1 then the shells will span across 1 world space unit
float _Density; // This is the density of the strands, used for initializing the noise
float _NoiseMin, _NoiseMax; // This is the range of possible hair lengths, which the hash then interpolates between 
float _Thickness; // This is the thickness of the hair strand
float _Attenuation; // This is the exponent on the shell height for lighting calculations to fake ambient occlusion (the lack of ambient light)
float _OcclusionBias; // This is an additive constant on the ambient occlusion in order to make the lighting less harsh and maybe kind of fake in-scattering
float _ShellDistanceAttenuation; // This is the exponent on determining how far to push the shell outwards, which biases shells downwards or upwards towards the minimum/maximum distance covered
float _Curvature; // This is the exponent on the physics displacement attenuation, a higher value controls how stiff the hair is
float _DisplacementStrength; // The strength of the displacement (very complicated)
float3 _ShellColor; // The color of the shells (very complicated)
float3 _ShellDirection;

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
    float shellHeight = (float) GetShellIndex() / (float) _ShellCount;

	// Since the height is now normalized, this exponent will behave a bit differently when applied to a number between 0 and 1, instead of
	// sending it off to infinity it instead biases the number closer to 0 or 1 depending on if the exponent is <1 or >1
	// I recommend looking at this kind of math in desmos so you can properly visualize how the exponent is affecting these numbers
    shellHeight = pow(shellHeight, _ShellDistanceAttenuation);

	// This is the line of code that extrudes the shells along the base vertex normal
	// Since the normal is a normalized vector (yes i know the terminology is confusing) then multiplying this changes
	// The displacement direction to align with the normal, this is then multiplied with the shell length to control how far the
	// shell extrudes and then it is lastly multiplied with the normalized height so that the shell falls into its proper place
	// in the layer cake of meshes
    outPos = pos + pos * _ShellLength * shellHeight;
	
	// This is for the "physics" this is what controls the curvature/stiffness of the hair, the higher the exponent the more the displacement
	// will only affect the top of the hair, this is something you can visualize in desmos pretty easily just like the shell height distance
	// attenuation calculation above. This is actually kind of a really common operation in graphics and why we keep most values normalized to 0-1
    float k = pow(shellHeight, _Curvature);

	// This displaces the shells after they have extruded according to the direction the cpu has told the shader we are moving, at rest this is going
	// to displace the hair downwards and since it's anchored at the root due to the variable 'k' above, only the tips of the hair will fall downwards
    outPos += k * _DisplacementStrength;
    outPos += k * _ShellDirection; 
	
	
#if SHADERGRAPH_PREVIEW
	outPos = pos;
#endif
}
 
void ShellMat_float(float2 uv, float3 normal, float3 lightDir, out float3 _outColor, out float _outAlpha)
{
	// As explained in the video, this multiplies the uv coordinates to create more strands because it generates more seeds
    float2 newUV = uv * _Density;

	// In order to operate in the local space uv coordinates after expanding them to a wider range, we take the fractional component
	// since uv coordinates by default range from 0 to 1 so then the fractional part is in 0 to 1 so it just works (tm) also we multiply
	// by 2 and subtract 1 to convert from 0 to 1 to -1 to 1 in order to shift the origin of these local uvs to the center for a calculation below
    float2 localUV = frac(newUV) * 2 - 1;
				
	// This is the local distance from the local center, the pythagorean distance technically
    float localDistanceFromCenter = length(localUV);

	// This casts the above uvs to uint so it can be more easily passed into the hashing function without doing a ton of annoying casts because
	// type casting can be really annoying and really ruin your day and you will generally not notice for potentially hours sometimes
    uint2 tid = newUV;
    uint seed = tid.x + 100 * tid.y + 100 * 10;

	// This also just casts the integer uniforms to floats for easier fractional computation below, this is technically unnecessary we could just say (float)_ShellIndex
	// but it's annoying to do that a lot so instead we use a temporary float variable
    float shellIndex = GetShellIndex();
    float shellCount = _ShellCount;

	// This is kind of complicated, we generate a random number from our seed which returns a number from 0 -> 1, which is then used
	// as an interpolator argument between the minimum noise value and the maximum noise value, which controls how short the hair can be
	// and how long the hair can be. We could just use the hash output itself, but this gives a little bit more control over the appearance
	// and length of the hair instead of giving all the power to the rng
    float rand = lerp(_NoiseMin, _NoiseMax, hash(seed));

	// This is the normalized shell height as described above in the vertex shader
    float h = shellIndex / shellCount;

	// This is the condition for discarding pixels, if the distance from the local center exceeds the thickness parameter we discard it,
	// and we also modify the thickness and make it thinner as height increases based on the height of the blade occupying this space that way
	// there aren't like weird hard cutoff tapers, you can try deleting the rand or replacing it with like 1 or something to see how this changes
	// the appearance of the grass or hair
    int outsideThickness = (localDistanceFromCenter) > (_Thickness * (rand - h));
				
	// This culls the pixel if it is outside the thickness of the strand, it also ensures that the base shell is fully opaque that way there aren't
	// any real holes in the mesh, although there's certainly better ways to do that
    _outAlpha = 1;
    if (outsideThickness && GetShellIndex() > 0)
    {
        _outAlpha = 0;
        return;
    }
	// This is the lighting output since at this point we have determined we are not discarding the pixel, so we have to color it
	// This lighting model is a modification of the Valve's half lambert as described in the video. It is not physically based, but it looks cool I think.
	// What's going on here is we take the dot product between the normal and the direction of the main Unity light source (the sun) which returns a value
	// between -1 to 1, which is then clamped to 0 to 1 by the DotClamped function provided by Unity, we then convert the 0 to 1 to 0.5 to 1 with the following
	// multiplication and addition.
    float ndotl = DotClamp(normal, lightDir) * 0.5f + 0.5f;

	// Valve's half lambert squares the ndotl output, which is going to bring values down, once again you can see how this looks on desmos by graphing x^2
    ndotl = ndotl * ndotl;

	// In order to fake ambient occlusion, we take the normalized shell height and take it to an attenuation exponent, which will do the same exact thing
	// I have explained that exponents will do to numbers between 0 and 1. A higher attenuation value means the occlusion of ambient light will become much stronger,
	// as the number is brought down closer to 0, and if we multiply a color with 0 then it'll be black aka in shadow.
    float ambientOcclusion = pow(h, _Attenuation);

	// This is a additive bias on the ambient occlusion, if you don't want the gradient to go towards black then you can add a bit to this in order to prevent
	// such a harsh gradient transition
    ambientOcclusion += _OcclusionBias;

	// Since the bias can push the ambient occlusion term above 1, we want to clamp it to 0 to 1 in order to prevent breaking the laws of physics by producing
	// more light than was received since if you multiply a color with a number greater than 1, it'll become brighter, and that just physically does not make
	// sense in this context
    ambientOcclusion = saturate(ambientOcclusion);

	// We put it all together down here by multiplying the color with Valve's half lambert and our fake ambient occlusion. You can remove some of these terms
	// to see how it changes the lighting and shadowing.
    _outColor = float3(_ShellColor * ndotl * ambientOcclusion);
}

#endif 