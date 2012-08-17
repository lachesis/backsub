uniform sampler2D SumTx; // Sum of a/n; Same as mean
uniform sampler2D IntermedTx; // Sum of (a/n)^2
uniform sampler2D FrameTx;
uniform sampler2D StdDevTx;
uniform float NumFrames;

uniform int Mode;

#define samp(tex) texture2D(tex, gl_TexCoord[0].xy)
#define sq(v) pow(v, vec4(2.0f,2.0f,2.0f,2.0f))
#define V4_ZERO vec4(0f,0f,0f,0f)
#define V4_ONE vec4(1f,1f,1f,1f)
#define clamp10(v) clamp(v, V4_ZERO, V4_ONE)

float calc_xi(vec4 inp, vec4 mean, vec4 stdev) // This is really alpha_I, eq5
{
	return (inp.x*mean.x/pow(stdev.x,2.0f) + inp.y*mean.y/pow(stdev.y,2.0f) + inp.z*mean.z/pow(stdev.z,2.0f)) / (pow(mean.x/stdev.x,2.0f)+pow(mean.y/stdev.y,2.0f)+pow(mean.z/stdev.z,2.0f));
}

float calc_cdi(vec4 inp, vec4 mean, vec4 stdev, float alfi)
{
	return sqrt(pow((inp.r-alfi*mean.r)/stdev.r,2.0f) + pow((inp.g-alfi*mean.g)/stdev.g,2.0f) + pow((inp.b-alfi*mean.b)/stdev.b,2.0f)) / 200.0f;
}

void main()
{
	float alfi, cdi;
	vec4 s;
	if(Mode == 0) // Passthrough
		gl_FragColor = samp(FrameTx);
	if(Mode == 1) // Sum of scaled values
		gl_FragColor = clamp10(samp(SumTx) + samp(FrameTx) / NumFrames);
	if(Mode == 2) // SumSquares of scaled values
		gl_FragColor = clamp10(samp(IntermedTx) + sq(samp(FrameTx))/NumFrames);
	if(Mode == 3) // StdDev of scaled values
		gl_FragColor = clamp10(sqrt(samp(IntermedTx) - sq(samp(SumTx))));
	if(Mode == 4) // (xi-1)**2/N sum
	{
		alfi = calc_xi(samp(FrameTx), samp(SumTx), samp(StdDevTx));
		cdi = calc_cdi(samp(FrameTx), samp(SumTx), samp(StdDevTx), alfi);
		
		// In this case, x will be AlfI sum and y will be CdI sum
		s = samp(IntermedTx);
		
		gl_FragColor = vec4(s.x + pow(alfi-1f,2f)/NumFrames, s.y + pow(cdi,2f)/NumFrames, 0f, 0f);
		gl_FragColor.x = 0f;
	}
	if(Mode == 5) 
	{
		gl_FragColor = sqrt(samp(IntermedTx));
	}
}